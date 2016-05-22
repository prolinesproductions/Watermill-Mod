using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Depender.Types.FlatRides;
using Depender.AnimationMotors;
using Depender.AnimationEvents;

public class FlatRideLoader
{
    public static List<motor> LoadMotors(XmlNode ObjNode, GameObject GO)
    {
        List<motor> motors = new List<motor>();

        foreach (XmlNode motorN in ObjNode.SelectSingleNode("Animation/motors").ChildNodes)
        {
            switch (motorN.Name)
            {

                case "Rotator":
                    RotatorHP R = GO.AddComponent<RotatorHP>();
                    R.Identifier = motorN["Identifier"].InnerText;
                    R.axisPath = motorN["axis"].InnerText;
                    R.axis = GO.transform.FindChild(motorN["axis"].InnerText);
                    R.maxSpeed = float.Parse(motorN["maxSpeed"].InnerText);
                    R.accelerationSpeed = float.Parse(motorN["accelerationSpeed"].InnerText);
                    R.rotationAxis = getVector3(motorN["rotationAxis"].InnerText);
                    motors.Add(R);
                    break;
                case "RotateBetween":
                    RotateBetweenHP RB = GO.AddComponent<RotateBetweenHP>();
                    RB.Identifier = motorN["Identifier"].InnerText;
                    RB.axisPath = motorN["axis"].InnerText;
                    RB.axis = GO.transform.FindChild(motorN["axis"].InnerText);
                    RB.rotationAxis = getVector3(motorN["rotationAxis"].InnerText);
                    RB.duration = float.Parse(motorN["duration"].InnerText);
                    motors.Add(RB);
                    break;
                case "Mover":
                    MoverHP M = GO.AddComponent<MoverHP>();
                    M.Identifier = motorN["Identifier"].InnerText;
                    M.axisPath = motorN["axis"].InnerText;
                    M.axis = GO.transform.FindChild(motorN["axis"].InnerText);
                    M.toPosition = getVector3(motorN["toPosition"].InnerText);
                    M.duration = float.Parse(motorN["duration"].InnerText);
                    motors.Add(M);
                    break;
                case "MultipleRotations":
                    MultipleRotationsHP MR = GO.AddComponent<MultipleRotationsHP>();
                    MR.Identifier = motorN["Identifier"].InnerText;
                    MR.axisPath = motorN["MainAxis"].InnerText;
                    XmlNodeList axisNs = motorN.SelectNodes("axis");
                    foreach (XmlNode axisN in axisNs)
                    {
                        MR.AxissPath.Add(axisN.InnerText);
                    }
                    motors.Add(MR);
                    break;
                case "PendulumRotator":
                    PendulumRotatorHP PR = GO.AddComponent<PendulumRotatorHP>();
                    PR.Identifier = motorN["Identifier"].InnerText;
                    PR.axisPath = motorN["axis"].InnerText;
                    PR.axis = GO.transform.FindChild(motorN["axis"].InnerText);
                    PR.maxSpeed = float.Parse(motorN["maxSpeed"].InnerText);
                    PR.accelerationSpeed = float.Parse(motorN["accelerationSpeed"].InnerText);
                    PR.rotationAxis = getVector3(motorN["rotationAxis"].InnerText);
                    PR.angularFriction = float.Parse(motorN["angularFriction"].InnerText);
                    PR.gravity = float.Parse(motorN["gravity"].InnerText);
                    PR.pendulum = Boolean.Parse(motorN["pendulum"].InnerText);
                    PR.armLength = float.Parse(motorN["armLength"].InnerText);
                    motors.Add(PR);
                    break;

            }
        }

        return motors;
        throw new NotImplementedException();
    }
    public static List<Phase> LoadPhases(XmlNode ObjNode, GameObject GO)
    {
        List<Phase> Phases = new List<Phase>();

        foreach (XmlNode PhaseN in ObjNode.SelectSingleNode("Animation/phases").ChildNodes)
        {

            Phase phase = GO.AddComponent<Phase>();
            foreach (XmlNode EventN in PhaseN.SelectSingleNode("events").ChildNodes)
            {
                switch (EventN.Name)
                {
                    case "Wait":
                        Wait W = GO.AddComponent<Wait>();
                        W.seconds = float.Parse(EventN["Seconds"].InnerText);
                        phase.Events.Add(W);
                        break;
                    case "StartRotator":
                        StartRotator SR = GO.AddComponent<StartRotator>();
                        SR.identifierMotor = EventN["Identifier"].InnerText;
                        phase.Events.Add(SR);
                        break;
                    case "SpinRotator":
                        SpinRotater SpR = GO.AddComponent<SpinRotater>();
                        SpR.identifierMotor = EventN["Identifier"].InnerText;
                        SpR.spin = Boolean.Parse(EventN["spin"].InnerText);
                        SpR.spins = float.Parse(EventN["spins"].InnerText);
                        phase.Events.Add(SpR);
                        break;
                    case "StopRotator":
                        StopRotator StR = GO.AddComponent<StopRotator>();
                        StR.identifierMotor = EventN["Identifier"].InnerText;
                        phase.Events.Add(StR);
                        break;
                    case "FromToRot":
                        FromToRot FTR = GO.AddComponent<FromToRot>();
                        FTR.identifierMotor = EventN["Identifier"].InnerText;
                        phase.Events.Add(FTR);
                        break;
                    case "ToFromRot":
                        ToFromRot TFR = GO.AddComponent<ToFromRot>();
                        TFR.identifierMotor = EventN["Identifier"].InnerText;
                        phase.Events.Add(TFR);
                        break;
                    case "FromToMove":
                        FromToMove FTM = GO.AddComponent<FromToMove>();
                        FTM.identifierMotor = EventN["Identifier"].InnerText;
                        phase.Events.Add(FTM);
                        break;
                    case "ToFromMove":
                        ToFromMove TFM = GO.AddComponent<ToFromMove>();
                        TFM.identifierMotor = EventN["Identifier"].InnerText;
                        phase.Events.Add(TFM);
                        break;
                    case "ApplyRotation":
                        ApplyRotation AR = GO.AddComponent<ApplyRotation>();
                        AR.identifierMotor = EventN["Identifier"].InnerText;
                        phase.Events.Add(AR);
                        break;
                    case "ChangePendulum":
                        ChangePendulum CP = GO.AddComponent<ChangePendulum>();
                        CP.identifierMotor = EventN["Identifier"].InnerText;
                        CP.Friction = float.Parse(EventN["Friction"].InnerText);
                        CP.Pendulum = Boolean.Parse(EventN["Pendulum"].InnerText);
                        phase.Events.Add(CP);
                        break;
                    default:
                        Debug.Log("Couln't detect the right event called: " + EventN.Name);
                        break;

                }
            }

            Phases.Add(phase);
        }
        return Phases;

        throw new NotImplementedException();
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
}

