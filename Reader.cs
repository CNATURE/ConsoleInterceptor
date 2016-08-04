using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleInterceptor
{
    public class Reader
    {
        private string logFile;
        
        private StreamReader s;
        private SharedRotateFile sharedLogFile;

        public Reader(SharedRotateFile logfile, StreamReader s)
        {
            this.sharedLogFile = logfile;
            this.s = s;
        }

        public Reader(String logFile, System.IO.StreamReader s)
        {
            this.logFile = logFile;
            this.s = s;
        }

        private string getFileName()
        {
            return this.sharedLogFile == null ? this.logFile : sharedLogFile.getLogfilename();
        }
        public void writeStream()
        {
            
            for (;;)
            {
                string line = "";
                line = line.Length == 0 ? s.ReadLine() : line;
                if (line == null) break;
                try
                {
                    File.AppendAllLines(getFileName(), new string[] { line });
                    Console.WriteLine(line);
                    line = "";
                }         
                catch (Exception e)
                {
                    line = "";
                    Console.WriteLine(e.Message);
                }
                
            }

        }
    }

}
