using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BlackBird;
using static BlackBird.MessageHelper;

namespace Quelea
{
    class Program
    {
        public static readonly string SETTING_SAVE_FILE_PATH = "app.settings";
        public static Settings settings;

        private static Client client;
        private static Worker worker;

        static void Main(string[] args)
        {
            if (File.Exists(SETTING_SAVE_FILE_PATH))
            {
                string json = File.ReadAllText(SETTING_SAVE_FILE_PATH);
                settings = Settings.CreateFromJson(ref json);
            }
            else
            {
                settings = Settings.CreateDefaultSettings();
                File.WriteAllText(SETTING_SAVE_FILE_PATH, settings.ToJson());
            }

            worker = new Worker();

            bool connected = false;
            int tryCount = 0;
            while (!connected && tryCount < 10) {
                try
                {
                    client = new Client(new ServerInfo
                    {
                        ipAddress = IPAddress.Parse(settings.HawkAdress),
                        port = settings.HawkPort
                    });
                    connected = true;
                    
                }
                catch (Exception _)
                {
                    tryCount++;
                    Console.WriteLine($"Hawk server coudn't be reached. Trying again ({tryCount}/10)");
                    Thread.Sleep(1000);
                }
            }

            if(connected)
            {
                client.SendMessage(QUELEA.READY.ToMessage());

                while(true)
                {
                    client.WaitForMessage();
                    Message received = client.ReceiveMessage();

                    string receivedStr = received.ToString();
                    if(receivedStr == HAWK.ORDER.BASE)
                    {
                        received = client.ReceiveMessage();
                        Command command = received.ToCommand();
                        Console.WriteLine($"Command : {command.command}");
                        Console.WriteLine($"Parameters : {command.parameters}");
                        Console.WriteLine($"Working directory : {command.workingDirector}");
                    }
                    else if (receivedStr == HAWK.ORDER.BUILD)
                    {
                        Console.WriteLine(receivedStr);
                        
                        worker.Execute("Unity.exe",
                            $"-buildWindows64Player {@"F:\UNITY_BUILD\" + DateTime.Now + @"\build.exe"} -projectPath {settings.WORKING_PROJETC_PATH}",
                            settings.UNITY_PATH);

                        client.SendMessage(QUELEA.DONE.ToMessage());
                    }
                    else
                    {
                        Console.WriteLine(receivedStr);
                    }
                }
            }
            else
            {
                Console.WriteLine("Hawk server coudn't be reached. Maximum number of tentative reached, aborting process");
            }
        }
    }
}