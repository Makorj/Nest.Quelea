using System;
using System.IO;
using System.Net;
using System.Reflection;
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
        public static readonly string VERSION = "Version " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static readonly string SETTING_SAVE_FILE_PATH = "app.settings";
        public static Settings settings;

        private static Client client;
        private static Worker worker;

        static void Main(string[] args)
        {                
            Console.Write("Loading app settings... ");
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
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("DONE");
            Console.ResetColor();

            Console.Write("Lauching a worker... ");
            worker = new Worker();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("DONE");
            Console.ResetColor();

            Console.Write($"Connecting to Hawk server on {settings.HawkAdress}:{settings.HawkPort}... ");
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

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("DONE");
                    Console.ResetColor();

                }
                catch (Exception _)
                {
                    tryCount++;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("ERROR");
                    Console.ResetColor();
                    Console.Write($"Hawk server coudn't be reached. Trying again ({tryCount}/10)");
                    Thread.Sleep(1000);
                }
            }
            Console.WriteLine();

            if(connected)
            {
                Console.WriteLine(LOGO);
                Console.WriteLine(VERSION);


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

                        client.WaitForMessage();
                        received = client.ReceiveMessage();

                        File.WriteAllText(settings.WORKING_PROJETC_PATH + "\\selectedSettings.json", received.ToString());

                        worker.Execute("Unity.exe",
                            $"-projectPath {settings.WORKING_PROJETC_PATH} -executeMethod BatchBuilderObject.Build -settingsInput {settings.WORKING_PROJETC_PATH}\\selectedSettings.json",
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

        public const string LOGO = @"________/\\\________/\\\________/\\\__/\\\\\\\\\\\\\\\__/\\\______________/\\\\\\\\\\\\\\\_____/\\\\\\\\\____        
 _____/\\\\/\\\\____\/\\\_______\/\\\_\/\\\///////////__\/\\\_____________\/\\\///////////____/\\\\\\\\\\\\\__       
  ___/\\\//\////\\\__\/\\\_______\/\\\_\/\\\_____________\/\\\_____________\/\\\______________/\\\/////////\\\_      
   __/\\\______\//\\\_\/\\\_______\/\\\_\/\\\\\\\\\\\_____\/\\\_____________\/\\\\\\\\\\\_____\/\\\_______\/\\\_     
    _\//\\\______/\\\__\/\\\_______\/\\\_\/\\\///////______\/\\\_____________\/\\\///////______\/\\\\\\\\\\\\\\\_    
     __\///\\\\/\\\\/___\/\\\_______\/\\\_\/\\\_____________\/\\\_____________\/\\\_____________\/\\\/////////\\\_   
      ____\////\\\//_____\//\\\______/\\\__\/\\\_____________\/\\\_____________\/\\\_____________\/\\\_______\/\\\_  
       _______\///\\\\\\___\///\\\\\\\\\/___\/\\\\\\\\\\\\\\\_\/\\\\\\\\\\\\\\\_\/\\\\\\\\\\\\\\\_\/\\\_______\/\\\_ 
        _________\//////______\/////////_____\///////////////__\///////////////__\///////////////__\///________\///__

";
    }
}