using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace ConsoleInterceptor
{
     
    public class SharedRotateFile
    {
        private string filename;

        public SharedRotateFile(string filename)
        {
            this.filename = filename;
        }

        public string getLogfilename()
        {
            return filename;
        }
        public void setLogfilename(string name)
        {
            filename = name;
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
            }      catch (Exception)  {      }

            //for local testing
            if (instanceId == null)
            {
                instanceId = "unknown_instance";
            }
           
            instanceId = instanceId.Substring(0, 6);


            var process = new LoggedProcess(args).getHandler();
            if (process == null) { return; }
            process.Start();




            string logFolder = Environment.ExpandEnvironmentVariables(@"%HOME%\LogFiles\Application");
            Directory.CreateDirectory(logFolder);
            string STR_LOG_PREFIX = $"logs_sdtout_{ instanceId}";
            string logStdoutFile = Path.Combine(logFolder, $"{STR_LOG_PREFIX}.log");
            string logStderrFile = Path.Combine(logFolder, $"logs_sderrt_{instanceId}.log");
            

            SharedRotateFile stdoutFile = new SharedRotateFile(logStdoutFile);
            var r = new Rotation(logFolder, stdoutFile, 2000, 10, STR_LOG_PREFIX);         
            var stdout = new Reader(stdoutFile, process.StandardOutput);         
            Thread stdoutWorker = new Thread(stdout.writeStream);
            stdoutWorker.Start();
            while (!stdoutWorker.IsAlive);

            
            //do not rotate stderr for now
            var stderr = new Reader(logStderrFile, process.StandardError);
            Thread stderrWorker = new Thread(stderr.writeStream);
            stderrWorker.Start();
            while (!stderrWorker.IsAlive) ;

            process.WaitForExit();
            Console.WriteLine("Ending console interceptor");
            //TODO need to free rotation once done? on process exit
        }
    }

}
