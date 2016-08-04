using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace ConsoleInterceptor
{

    public class Reader
    {
        private string logFile;
        private StreamReader s;

        public Reader(String logFile, System.IO.StreamReader s)
        {
            this.logFile = logFile;
            this.s = s;
        }
            public void writeStream()
            {
            for (;;)
            {
                string line =s.ReadLine();
                if (line == null) break;
                File.AppendAllLines(logFile, new string[] { line });
                Console.WriteLine(line);
            }

        }




    }
    class Program
    {

        static void Main(string[] args)
        {


            Console.WriteLine("Starting console interceptor");
            // For local testing
            //Environment.SetEnvironmentVariable("HOME", @"d:\tmp");
            //Environment.SetEnvironmentVariable("WEBSITE_INSTANCE_ID", @"qwertyuiop");

            // Get the first 6 chars of the instance ID to differentiate from other instances

            string instanceId = "";

            try
            {
                instanceId = Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID");
            }
            catch (Exception)
            {


            }

            //for local testing
            if (instanceId == null)
            {
                instanceId = "unknown_instance";
            }


            instanceId = instanceId.Substring(0, 6);

            string logFolder = Environment.ExpandEnvironmentVariables(@"%HOME%\LogFiles\Application");
            Directory.CreateDirectory(logFolder);
            string logStdoutFile = Path.Combine(logFolder, $"logs_sdtout_{instanceId}.txt");
            string logStderrFile = Path.Combine(logFolder, $"logs_sderrt_{instanceId}.txt");

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        // First arg is the program to run
                        FileName = args[0],
                        //FileName= @"D:\Program Files (x86)\apache-tomcat-8.0.23\bin\startup.bat",


                    // Rest of the args are passed to the program
                        Arguments = String.Join(" ", args, 1, args.Length - 1),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            try
            {
                Console.WriteLine("Running: {0} with args: {1}", process.StartInfo.FileName,process.StartInfo.Arguments);
                process.Start();
            } catch (Exception e)

            {
                Console.WriteLine(e.Message);
            }
            
            var stdout = new Reader(logStdoutFile, process.StandardOutput);

            Thread stdoutWorker = new Thread(stdout.writeStream);
            stdoutWorker.Start();
            while (!stdoutWorker.IsAlive) ;


            var stderr = new Reader(logStderrFile, process.StandardError);
            Thread stderrWorker = new Thread(stderr.writeStream);
            stderrWorker.Start();
            while (!stderrWorker.IsAlive) ;

            //for (;;)
            //{
            //    string line = process.StandardOutput.ReadLine();
            //    if (line == null) break;
            //    File.AppendAllLines(logStdoutFile, new string[] { line });
            //    Console.WriteLine(line);
            //}

            process.WaitForExit();
            Console.WriteLine("Ending console interceptor");
        }
    }   
    
}
