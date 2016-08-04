using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleInterceptor
{
    class LoggedProcess
    {
        private readonly Process process;

        public Process getProcess()
        {
            return this.process;
        }

        public void start()
        {
            Console.WriteLine("Running: {0} with args: {1}", process.StartInfo.FileName, process.StartInfo.Arguments);
            process.Start();

        }


        public Process getHandler()
        {
            return this.process;
        }
        public LoggedProcess(string[] args) {

            try
            {
           
            this.process = new Process
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

            }
            catch (Exception e)

            {
                this.process = null;
                Console.WriteLine(e.Message);
            }
        }
    }
}
