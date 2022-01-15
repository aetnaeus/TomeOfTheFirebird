﻿using Newtonsoft.Json;
using System.IO;
using System.Reflection;
using static UnityModManagerNet.UnityModManager;

namespace TomeOfTheFirebird.Config
{
    class ModSettings
    {
        public static ModEntry ModEntry;
        public static NewContent NewContent;
        public static Tweaks Tweaks;
        public static Bugfixes Bugfixes;
        public static Blueprints Blueprints;
        public static ContentModifications ContentModifications;
        private static string userConfigFolder => ModEntry.Path + "UserSettings";
        private static JsonSerializerSettings cachedSettings;
        private static JsonSerializerSettings SerializerSettings
        {
            get
            {
                if (cachedSettings == null)
                {
                    cachedSettings = new JsonSerializerSettings
                    {
                        CheckAdditionalContent = false,
                        ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                        DefaultValueHandling = DefaultValueHandling.Include,
                        FloatParseHandling = FloatParseHandling.Double,
                        Formatting = Formatting.Indented,
                        MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                        MissingMemberHandling = MissingMemberHandling.Ignore,
                        NullValueHandling = NullValueHandling.Ignore,
                        ObjectCreationHandling = ObjectCreationHandling.Replace,
                        StringEscapeHandling = StringEscapeHandling.Default,
                    };
                }
                return cachedSettings;
            }
        }

        public static void LoadAllSettings()
        {
            LoadSettings("NewContent.json", ref NewContent);
            LoadSettings("Tweaks.json", ref Tweaks);
            LoadSettings("Bugfixes.json", ref Bugfixes);
            LoadSettings("Blueprints.json", ref Blueprints);
            LoadSettings("ContentModifications.json", ref ContentModifications);
        }
        private static void LoadSettings<T>(string fileName, ref T setting) where T : IUpdatableSettings
        {
            JsonSerializer serializer = JsonSerializer.Create(SerializerSettings);
            var assembly = Assembly.GetExecutingAssembly();
            var resourcePath = $"TomeOfTheFirebird.Config.{fileName}";
            var userPath = $"{userConfigFolder}{Path.DirectorySeparatorChar}{fileName}";

            Directory.CreateDirectory(userConfigFolder);
            using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
            using (StreamReader streamReader = new StreamReader(stream))
            using (JsonReader jsonReader = new JsonTextReader(streamReader))
            {
                setting = serializer.Deserialize<T>(jsonReader);
                setting.Init();
            }
            if (File.Exists(userPath))
            {
                using (StreamReader streamReader = File.OpenText(userPath))
                using (JsonReader jsonReader = new JsonTextReader(streamReader))
                {
                    try
                    {
                        T userSettings = serializer.Deserialize<T>(jsonReader);
                        setting.OverrideSettings(userSettings);
                    }
                    catch
                    {
                        Main.Error("Failed to load user settings. Settings will be rebuilt.");
                        try { File.Copy(userPath, userConfigFolder + $"{Path.DirectorySeparatorChar}BROKEN_{fileName}", true); } catch { Main.Error("Failed to archive broken settings."); }
                    }
                }
            }
            SaveSettings(fileName, setting);
        }

        public static void SaveSettings(string fileName, IUpdatableSettings setting)
        {
            Directory.CreateDirectory(userConfigFolder);
            var userPath = $"{userConfigFolder}{Path.DirectorySeparatorChar}{fileName}";

            JsonSerializer serializer = JsonSerializer.Create(SerializerSettings);
            using (StreamWriter streamWriter = new StreamWriter(userPath))
            using (JsonWriter jsonWriter = new JsonTextWriter(streamWriter))
            {
                serializer.Serialize(jsonWriter, setting);
            }
        }
    }
}
