using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Xml;
using System.Linq;
using Depender.Types.Shops;
using Depender;
using Depender.Types;
using Depender.Types.FlatRides;

public class ModLoader : MonoBehaviour
{
    public string Path;

    public string Identifier;
    public string modName = "";
    public string modDiscription = "";
    
    private void LogException(Exception e)
    {
        StreamWriter sw = File.AppendText(Path + @"/mod.log");

        sw.WriteLine(e);

        sw.Flush();

        sw.Close();
    }
    public void LoadScenery()
    {
        try
        {
            GameObject hider = new GameObject();
            char dsc = System.IO.Path.DirectorySeparatorChar;

            using (WWW www = new WWW("file://" + Path + dsc + "assetbundle" + dsc + "mod"))
            {
                if (www.error != null)
                    throw new Exception("Loading had an error:" + www.error);

                AssetBundle bundle = www.assetBundle;
                try
                {


                    XmlDocument doc = new XmlDocument();
                    string[] files = System.IO.Directory.GetFiles(Path, "*.xml");
                    doc.Load(files[0]);
                    XmlElement xelRoot = doc.DocumentElement;
                    XmlNodeList ModNodes = xelRoot.SelectNodes("/Mod");

                    foreach (XmlNode Mod in ModNodes)
                    {

                        modName = Mod["ModName"].InnerText;
                        modDiscription = Mod["ModDiscription"].InnerText;
                    }
                    XmlNodeList ObjectNodes = xelRoot.SelectNodes("/Mod/Objects/Object");

                    foreach (XmlNode ParkOBJ in ObjectNodes)
                    {
                        try
                        {
                            
                            ModdedObject MO = null;
                            GameObject asset = Instantiate(bundle.LoadAsset(ParkOBJ["OBJName"].InnerText)) as GameObject;
                            asset.name =  Identifier + "@" + ParkOBJ["OBJName"].InnerText;
                            switch (ParkOBJ["Type"].InnerText)
                            {
                                case "deco":
                                    DecoMod DM = new DecoMod();
                                    DM.HeightDelta = float.Parse(ParkOBJ["heightDelta"].InnerText);
                                    DM.GridSubdivision = 1f;
                                    DM.SnapCenter = Convert.ToBoolean(ParkOBJ["snapCenter"].InnerText);
                                    DM.category = ParkOBJ["category"].InnerText;
                                    DM.BuildOnGrid = Convert.ToBoolean(ParkOBJ["grid"].InnerText);
                                    MO = DM;
                                    break;
                                case "trashbin":
                                    MO = new TrashBinMod();
                                    break;
                                case "seating":
                                    SeatingMod SM = new SeatingMod();
                                    SM.hasBackRest = false;
                                    MO = SM;
                                    break;
                                case "seatingAuto":
                                    SeatingAutoMod SMA = new SeatingAutoMod();
                                    SMA.hasBackRest = false;
                                    SMA.seatCount = 2;
                                    MO = SMA;
                                    break;
                                case "lamp":
                                    MO = new LampMod();
                                    break;
                                case "fence":
                                    FenceMod FM = new FenceMod();
                                    FM.FenceFlat = null;
                                    FM.FencePost = null;
                                    MO = FM;
                                    break;
                                case "FlatRide":
                                    FlatRideMod FR = new FlatRideMod();
                                    FR.XSize = (int)float.Parse(ParkOBJ["X"].InnerText);
                                    FR.ZSize = (int)float.Parse(ParkOBJ["Z"].InnerText);
                                    FR.Excitement = float.Parse(ParkOBJ["Excitement"].InnerText);
                                    FR.Intensity = float.Parse(ParkOBJ["Intensity"].InnerText);
                                    FR.Nausea = float.Parse(ParkOBJ["Nausea"].InnerText);
                                    FR.closedAngleRetraints = getVector3(ParkOBJ["RestraintAngle"].InnerText);
                                    RideAnimationMod RA = new RideAnimationMod();
                                    RA.motors = FlatRideLoader.LoadMotors(ParkOBJ, asset);
                                    RA.phases = FlatRideLoader.LoadPhases(ParkOBJ, asset);
                                    FR.Animation = RA;
                                    XmlNodeList WaypointsNodes = ParkOBJ.SelectNodes("Waypoints/Waypoint");
                                    foreach (XmlNode xndNode in WaypointsNodes)
                                    {

                                        Waypoint w = new Waypoint();
                                        w.isOuter = Convert.ToBoolean(xndNode["isOuter"].InnerText);
                                        w.isRabbitHoleGoal = Convert.ToBoolean(xndNode["isRabbitHoleGoal"].InnerText);
                                        if (xndNode["connectedTo"].InnerText != "")
                                        {
                                            w.connectedTo = xndNode["connectedTo"].InnerText.Split(',').ToList().ConvertAll(s => Int32.Parse(s));
                                        }
                                        w.localPosition = getVector3(xndNode["localPosition"].InnerText);
                                        FR.waypoints.Add(w);

                                    }
                                    MO = FR;
                                    break;
                                case "Shop":
                                    ShopMod S = new ShopMod();

                                    asset.SetActive(false);
                                    XmlNodeList ProductNodes = ParkOBJ.SelectNodes("Shop/Product");
                                    foreach (XmlNode ProductNode in ProductNodes)
                                    {
                                        ProductMod PM = new ProductMod();
                                        switch (ProductNode["Type"].InnerText)
                                        {
                                            case "consumable":
                                                consumable C = new consumable();
                                                C.ConsumeAnimation = (consumable.consumeanimation)Enum.Parse(typeof(consumable.consumeanimation), ProductNode["ConsumeAnimation"].InnerText);
                                                C.Temprature = (consumable.temprature)Enum.Parse(typeof(consumable.temprature), ProductNode["Temprature"].InnerText);
                                                C.portions = Int32.Parse(ProductNode["Portions"].InnerText);
                                                PM = C;
                                                break;
                                            case "wearable":
                                                wearable W = new wearable();
                                                W.BodyLocation = (wearable.bodylocation)Enum.Parse(typeof(wearable.bodylocation), ProductNode["BodyLocation"].InnerText);
                                                PM = W;
                                                break;
                                            case "ongoing":
                                                ongoing O = new ongoing();
                                                O.duration = Int32.Parse(ProductNode["Duration"].InnerText);
                                                PM = O;
                                                break;
                                            default:
                                                break;
                                        }
                                        PM.Name = ProductNode["Name"].InnerText;
                                        PM.GO = Instantiate(bundle.LoadAsset(ProductNode["Model"].InnerText)) as GameObject;
                                        PM.GO.SetActive(false);
                                        PM.Hand = (ProductMod.hand)Enum.Parse(typeof(ProductMod.hand), ProductNode["Hand"].InnerText);
                                        PM.price = Int32.Parse(ProductNode["Price"].InnerText);
                                        XmlNodeList IngredientNodes = ProductNode.SelectNodes("Ingredients/Ingredient");
                                        foreach (XmlNode IngredientNode in IngredientNodes)
                                        {
                                            ingredient I = new ingredient();
                                            I.Name = IngredientNode["Name"].InnerText;
                                            I.price = Int32.Parse(IngredientNode["Price"].InnerText);
                                            I.amount = Int32.Parse(IngredientNode["Amount"].InnerText);
                                            I.tweakable = Boolean.Parse(IngredientNode["tweakable"].InnerText);
                                            XmlNodeList EffectNodes = IngredientNode.SelectNodes("Effects/effect");
                                            foreach (XmlNode EffectNode in EffectNodes)
                                            {
                                                effect E = new effect();
                                                E.Type = (effect.Types)Enum.Parse(typeof(effect.Types), EffectNode["Type"].InnerText);
                                                E.amount = float.Parse(EffectNode["Amount"].InnerText);
                                                I.effects.Add(E);

                                            }
                                            PM.ingredients.Add(I);
                                        }
                                        S.products.Add(PM);
                                    }
                                    MO = S;
                                    break;
                                case "PathStyle":
                                    Registar.RegisterPath(asset.GetComponent<Renderer>().material.mainTexture, ParkOBJ["inGameName"].InnerText, (Registar.PathType) Enum.Parse(typeof(Registar.PathType), ParkOBJ["PathStyle"].InnerText) );
                                    break;
                                case "CoasterCar":
                                    Debug.Log("Test CoasterCar");
                                    CoasterCarMod CC = new CoasterCarMod();
                                    CC.CoasterName = ParkOBJ["CoasterName"].InnerText;
                                    CC.Name = ParkOBJ["inGameName"].InnerText;

                                    try
                                    {
                                        CC.closedAngleRetraints = getVector3(ParkOBJ["RestraintAngle"].InnerText);
                                        CC.FrontCarGO = Instantiate(bundle.LoadAsset(ParkOBJ["FrontCarGO"].InnerText)) as GameObject;
                                    }
                                    catch
                                    { }
                                    MO = CC;
                                    MO.Name = ParkOBJ["inGameName"].InnerText;
                                    MO.Object = asset;

                                    break;
                                default:
                                    break;
                            }
                            if (MO != null)
                            {
                                MO.Name = ParkOBJ["inGameName"].InnerText;
                                MO.Object = asset;
                                MO.Price = Int32.Parse(ParkOBJ["price"].InnerText);
                                MO.Shader = ParkOBJ["shader"].InnerText;
                                if (ParkOBJ["BoudingBoxes"] != null)
                                {

                                    XmlNodeList BoudingBoxNodes = ParkOBJ.SelectNodes("BoudingBoxes/BoudingBox");
                                    foreach (XmlNode Box in BoudingBoxNodes)
                                    {
                                        BoundingBox BB = MO.Object.AddComponent<BoundingBox>();
                                        BB.isStatic = false;
                                        BB.bounds.min = getVector3(Box["min"].InnerText);
                                        BB.bounds.max = getVector3(Box["max"].InnerText);
                                        BB.layers = BoundingVolume.Layers.Buildvolume;
                                        BB.isStatic = true;
                                    }
                                }
                                MO.Recolorable = Convert.ToBoolean(ParkOBJ["recolorable"].InnerText);
                                if (MO.Recolorable)
                                {

                                    List<Color> colors = new List<Color>();
                                    if (HexToColor(ParkOBJ["Color1"].InnerText) != new Color(0.95f, 0, 0))
                                        colors.Add(HexToColor(ParkOBJ["Color1"].InnerText));
                                    if (HexToColor(ParkOBJ["Color2"].InnerText) != new Color(0.32f, 1, 0))
                                        colors.Add(HexToColor(ParkOBJ["Color2"].InnerText));
                                    Debug.Log("Color 3" + HexToColor(ParkOBJ["Color3"].InnerText));
                                    if (ParkOBJ["Color3"].InnerText != "1C0FFF")
                                        colors.Add(HexToColor(ParkOBJ["Color3"].InnerText));
                                    if (HexToColor(ParkOBJ["Color4"].InnerText) != new Color(1, 0, 1))
                                        colors.Add(HexToColor(ParkOBJ["Color4"].InnerText));

                                    MO.Colors = colors.ToArray();
                                }
                                Registar.Register(MO).GetComponent<BuildableObject>();
                            }
                        }
                        catch (Exception e)
                        {
                            LogException(e);
                        }
                    }
                }
                catch (Exception e)
                {
                    LogException(e);
                }
                bundle.Unload(false);
                Debug.Log("Bundle Unloaded");


            }
        }
        catch (Exception e)
        {
            LogException(e);
        }
    }
    public void UnloadScenery()
    {
        Registar.UnRegister();
    }
    public static Vector3 getVector3(string rString)
    {
        string[] temp = rString.Substring(1, rString.Length - 2).Split(',');
        float x = float.Parse(temp[0]);
        float y = float.Parse(temp[1]);
        float z = float.Parse(temp[2]);
        Vector3 rValue = new Vector3(x, y, z);
        return rValue;
    }
    string ColorToHex(Color32 color)
    {
        string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
        return hex;
    }

    Color HexToColor(string hex)
    {
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        return new Color32(r, g, b, 255);
    }
}

