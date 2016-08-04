using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleInterceptor
{

    public class emptyLocked
    {


        private string fullPath;

        protected virtual bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }
        public emptyLocked(String fullPath)
        {
            this.fullPath = fullPath;
        }

        public void empty()
        {
            FileInfo file = new FileInfo(this.fullPath);
            while (IsFileLocked(file))
                Thread.Sleep(10);
            file.Create();

        }
    }
    public class deleteLocked
    {
        private string fullPath;

        protected virtual bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }
        public deleteLocked(String fullPath)
        {
            this.fullPath = fullPath;
        }

        public void delete()
        {
            FileInfo file = new FileInfo(this.fullPath);
            while (IsFileLocked(file))
                Thread.Sleep(10);
            file.Delete();
        }
    }


    public class Rotation
    {
        private readonly int maxSize;
        private FileSystemWatcher watcher;

        private string path;
        private string prefix;
        private int maxFileNumber;



        private SharedRotateFile sharedFile;

        public Rotation(string path, string filename, int maxSize, int maxFileNumber, string prefix)
        {
            this.maxSize = maxSize;
            this.maxFileNumber = maxFileNumber;
            this.path = path;
            this.prefix = prefix;

        }



        public Rotation(string logFolder, SharedRotateFile sharedFile, int maxSize, int maxFileNumber, string prefix)
        {
            this.path = logFolder;
            this.sharedFile = sharedFile;
            this.maxSize = maxSize;
            this.maxFileNumber = maxFileNumber;
            this.prefix = prefix;
            createWatcher(logFolder, sharedFile.getLogfilename());
        }

        public void stop()
        {
            if (watcher == null)
            {
                return;
            }
            watcher.Dispose();
        }

        private void createWatcher(string path, string fileName)
        {
            Console.WriteLine("watching file {0} at {1} ", Path.GetFileName(fileName), path);
            watcher = new FileSystemWatcher();
            watcher.Path = path;
            watcher.NotifyFilter = NotifyFilters.Size;
            watcher.Filter = Path.GetFileName(fileName);
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.EnableRaisingEvents = true;



        }

        public void rotate(String fullPath)
        {
            if (!File.Exists(fullPath))
            {
                return;
            }


            string[] files = System.IO.Directory.GetFiles(path, prefix + "*");

            //TODO think if this sort the files in the right direction...
            Array.Sort(files, (f1, f2) => f1.CompareTo(f2));

            var filename = Path.GetFileNameWithoutExtension(files[files.Length - 1]);

            Console.WriteLine(filename);
            var n = 1;
            var counter = filename.Substring(filename.LastIndexOf("_") + 1);
            try
            {
                Console.WriteLine(counter);
                n = int.Parse(counter);
            }
            catch (Exception e)
            {

            }








            var ext = Path.GetExtension(fullPath);
            var newFilename = String.Format("{0}_{1}{2}", prefix, (n + 1).ToString("D8"), ext);
            string newFullpath = Path.Combine(path, newFilename);
            //File.Copy(fullPath, newFullpath, true);
            using (var f = new FileStream(fullPath, FileMode.Open,
                FileAccess.Read,
                    FileShare.ReadWrite))
            {
                using (var outputFile = new FileStream(newFullpath, FileMode.Create))
                {
                    f.CopyTo(outputFile);
                }

            }



            // this.sharedFile.setLogfilename(newFullpath);

            //decide if to delete it or to clear it 
            var emptyLocked = new emptyLocked(fullPath);
            Thread emptyit = new Thread(emptyLocked.empty);
            emptyit.Start();

            //do not delete it for now
            //File.Delete(fullPath);
            //var deleteLocked = new deleteLocked(fullPath);
            //Thread deleter = new Thread(deleteLocked.delete);
            //deleter.Start();

        }
        public void limitNumOfFiles()
        {
            string[] files = System.IO.Directory.GetFiles(path, prefix + "*");

            //TODO think if this sort the files in the right direction...
            Array.Sort(files, (f1, f2) => f1.CompareTo(f2));

            for (int i = 0; i < files.Length - this.maxFileNumber; i++)
            {
                if (files[i].CompareTo(this.sharedFile.getLogfilename()) == 0)
                {
                    continue;
                }
                Console.Write("deleting file {0}", files[i]);
                File.Delete(files[i]);
            }

        }
        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("new file change file");
            FileInfo fInfo = new FileInfo(e.FullPath);
            if (fInfo.Length < this.maxSize)
            {
                return;
            }
            this.rotate(e.FullPath);
            this.limitNumOfFiles();
        }


    }
}
