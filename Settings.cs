using System;
using System.Net;
using System.Text.Json;

namespace Quelea
{

    public struct Settings
    {
        public string HawkAdress { get; set; }
        public int HawkPort { get; set; }

        public string UNITY_PATH { get; set; }
        public string WORKING_PROJETC_PATH { get; set; }

        public static Settings CreateDefaultSettings()
        {
            Settings defaultSettings = new Settings
            {
                HawkAdress = "127.0.0.1",
                HawkPort = 8001,

                UNITY_PATH = @"F:\Programmes\Unity\2019.4.0f1\Editor",
                WORKING_PROJETC_PATH = @"C:\ProjetsUnity\OprhanAge"
            };

            return defaultSettings;
        }

        public static Settings CreateFromJson(ref string json)
        {
            return JsonSerializer.Deserialize<Settings>(json);
        }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}