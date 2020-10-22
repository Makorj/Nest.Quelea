using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Quelea
{
    class Worker
    {
        private readonly string WINDOWS_Bash = @"C:\Windows\System32\cmd.exe";

        private static uint _nextID = 0;
        public static uint GetNextID() { return _nextID++; }

        public uint ID { get; private set; }

        private Process process;

        public Worker()
        {
            ID = GetNextID();
        }

        public int Execute(string command, string parameters, string workingDirectory)
        {
            process = CreateProcess(command, parameters, workingDirectory);

            process.Start();
            process.WaitForExit();

            return process.ExitCode;
        }

        private Process CreateProcess(string command, string parameters, string workingDirectory)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return new Process
                {
                    StartInfo = new ProcessStartInfo
                    {

                        FileName = WINDOWS_Bash,
                        Arguments = $"/C " + command + " " + parameters,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                        WorkingDirectory = workingDirectory,
                    }
                };
            }
            else // *NIX system and MACOS
            {
                return new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = command,
                        Arguments = parameters,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                        WorkingDirectory = workingDirectory,
                    }
                };
            }
        }

        public async Task ExecuteAsync(string command, string parameters, string workingDirectory, int delayInMS = 2500)
        {
            process = CreateProcess(command, parameters, workingDirectory);

            process.Start();

            while(process.HasExited)
            {
                await Task.Delay(delayInMS);
            }
        }

        public int GetExitCode()
        {
            return process?.ExitCode ?? -1;
        }

        public StreamReader GetOutputStream()
        {
            return process?.StandardOutput;
        }
    }
}
