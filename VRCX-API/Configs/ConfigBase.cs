using Newtonsoft.Json;

namespace VRCX_API.Configs
{
    public class Config<T> where T : new()
    {
        public T Instance;
        private readonly string FileLocation;
        private readonly JsonSerializerSettings JsonSerializerSettings = new();
        public static string ConfigPath => MainfolderLocation + "config/";

        public Config(string fileName)
        {
            FileLocation = ConfigPath + fileName;
            Instance = new();
            Load();
        }

        public void Load()
        {
            Instance = new();
            if (File.Exists(FileLocation))
            {
                var cfgInstance = JsonConvert.DeserializeObject<T>(File.ReadAllText(FileLocation), JsonSerializerSettings);

                if (cfgInstance != null)
                {
                    Instance = cfgInstance;
                }
            }
            Save();
        }

        public void Save()
        {
            File.WriteAllText(FileLocation, JsonConvert.SerializeObject(Instance, Formatting.Indented));
        }

        private static string? _mainfolderLocation = null;
        public static string MainfolderLocation
        {
            get
            {
                if (_mainfolderLocation == null)
                {
                    if (File.Exists("_MainFolderPath.txt"))
                    {
                        MainfolderLocation = File.ReadAllText("_MainFolderPath.txt").Trim();
                    }
                    else
                    {
                        MainfolderLocation = "";
                        File.WriteAllText("_MainFolderPath.txt", "");
                    }
                }
                return _mainfolderLocation!;
            }
            private set
            {
                _mainfolderLocation = value;
                if (!Directory.Exists(ConfigPath))
                {
                    Directory.CreateDirectory(ConfigPath);
                }
            }
        }
    }
}
