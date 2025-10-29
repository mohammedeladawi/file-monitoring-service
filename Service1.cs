using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;

namespace FileMonitoringService
{
    public partial class Service1 : ServiceBase
    {
        private readonly string source = ConfigurationManager.AppSettings["SourceFolder"];
        private readonly string destination = ConfigurationManager.AppSettings["DestinationFolder"];
        private readonly string logFolder = ConfigurationManager.AppSettings["LogFolder"];
        private FileSystemWatcher watcher;
        
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Log("Service is started");
            StartFileMonitoring();
        }

        protected override void OnStop()
        {
            if (watcher != null)
            {
                watcher.Created -= OnFileCreated;
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }

            Log("Service is stopped");
        }

        private void StartFileMonitoring()
        {
            // check source folder exist or return
            if (!Directory.Exists(source))
                return;

            // check destination folder exist or create
            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }

            MoveExistingFiles();

            // Move any added file while service running
            watcher = new FileSystemWatcher(source)
            {
                Filter = "*.*",
                NotifyFilter = NotifyFilters.FileName
            };

            watcher.Created += OnFileCreated;
            watcher.EnableRaisingEvents = true;

            Log($"Started monitoring folder: {source}");
        }

        private void MoveExistingFiles()
        {
            string[] files = Directory.GetFiles(source);
            foreach (string filePath in files)
            {
                DetectAndMoveFile(filePath);
            }
        }

        private void DetectAndMoveFile(string filePath)
        {
            Log($"File detected: {filePath}");

            // Check if it is available
            int retries = 0;
            const int maxRetries = 10;

            while (!IsFileReady(filePath))
            {
                System.Threading.Thread.Sleep(500);
                retries++;
                if (retries > maxRetries)
                {
                    Log($"File '{filePath}' is locked and could not be moved after multiple attempts.");
                    return;
                }
            }

            // Move
            try
            {
                string newName = Guid.NewGuid().ToString() + Path.GetExtension(filePath);
                string destinationPath = Path.Combine(destination, newName);
               
                // Should wait until the load complete 
                File.Move(filePath, destinationPath);
                Log($"File moved: {filePath} -> {destinationPath}");
            }
            catch (Exception ex)
            {
                Log($"Error moving file '{filePath}': {ex.Message}");
            }
        }

        private void Log(string message)
        {
            try
            {
                // Ensure log directory exists
                if (!Directory.Exists(logFolder))
                    Directory.CreateDirectory(logFolder);

                string logFilePath = Path.Combine(logFolder, "service.log");

                // Format log message with timestamp
                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";

                // Append message to log file (creates file automatically if missing)
                File.AppendAllText(logFilePath, logMessage + Environment.NewLine);

                // Also write to console in interactive mode (debug mode)
                if (Environment.UserInteractive)
                    Console.WriteLine(logMessage);
            }
            catch (Exception ex)
            {
                // Avoid crashing service if logging fails
                Debug.WriteLine($"Logging failed: {ex.Message}");
            }

        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            DetectAndMoveFile(e.FullPath);
        }

        private bool IsFileReady(string path)
        {
            try
            {
                using (FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None))
                    return true;
            }
            catch
            {
                return false;
            }
        }
    
        // Test service
        public void StartInConsole()
        {
            OnStart(null);
            Console.WriteLine("Press enter to stop the service...");
            Console.ReadLine();
            OnStop();
        }
    }
}
