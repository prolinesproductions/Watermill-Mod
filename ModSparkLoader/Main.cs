using Depender;
using UnityEngine;

namespace Custom_Scenery
{
    public class Main : IMod
    {
        private GameObject _go;
        string name = "Could not load name";
        string description = "Could not load description";
        public void onEnabled()
        {
            _go = new GameObject();

            _go.AddComponent<ModLoader>();

            _go.GetComponent<ModLoader>().Path = Path;

            _go.GetComponent<ModLoader>().Identifier = Identifier;

            _go.GetComponent<ModLoader>().LoadScenery();

            name = _go.GetComponent<ModLoader>().modName;

            description = _go.GetComponent<ModLoader>().modDiscription;
        }

        public void onDisabled()
        {
            Registar.UnRegister();
            Object.Destroy(_go);
        }

        public string Name { get { return name; } }
        public string Description { get { return description; } }
        public string Path { get; set; }
        public string Identifier { get; set; }
    }
}
