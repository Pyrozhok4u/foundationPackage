#if ENABLE_FILE_LOGGER
using System;
using System.IO;
using Foundation.ConfigurationResolver;
using Foundation.ServicesResolver;
using UnityEngine;

namespace Foundation.FileLogger
{
    public class FileLoggerService : BaseService
    {
        private StreamWriter _writer;
        private string _currentLogPath;
        private readonly string _logFolderPath = Application.persistentDataPath + "/Logs";
        private readonly string _dateFormat = "dd.MM.yyyy";
        private readonly string _timeFormat = "HH-mm-ss";

        #region BaseService

        protected override void Initialize()
        {
            if (!GetConfig<FileLoggerConfig>().IsEnabled)
            {
                return;
            }

            CreateFile();
            Application.logMessageReceivedThreaded += OnLogReceived;
        }

        public override void Dispose()
        {
            if (!GetConfig<FileLoggerConfig>().IsEnabled)
            {
                return;
            }

            Application.logMessageReceivedThreaded -= OnLogReceived;
            _currentLogPath = string.Empty;
            _writer?.Close();
            _writer?.Dispose();
            _writer = null;
        }

        #endregion

        #region API
        /// <summary>
        /// Deletes all log files except curent
        /// </summary>
        public void ClearAllLogs()
        {
            DirectoryInfo di = new DirectoryInfo(_logFolderPath);

            foreach (FileInfo file in di.GetFiles())
            {
                if (file.FullName.Equals(_currentLogPath))
                {
                    continue;
                }

                file.Delete();
            }
        }

        #endregion

        #region Internal
        private void OnLogReceived(string condition, string stacktrace, LogType type)
        {
            string logLine = "[" + DateTime.Now.ToLongTimeString() + "]" + " (" + type.ToString() + ")" + condition +
                             "\n" + stacktrace;
            _writer.WriteLine(logLine);
            _writer.Flush();
        }

        private void CreateFile()
        {
            if (!Directory.Exists(_logFolderPath))
            {
                Directory.CreateDirectory(_logFolderPath);
            }

            string date = DateTime.Now.ToString("(" + _dateFormat + " " + _timeFormat + ")");
            _currentLogPath = _logFolderPath + "/" + date + "_logs.txt";
            _writer = File.CreateText(_currentLogPath);
            _writer.AutoFlush = true;
            _writer.WriteLine("========= Log for " + Application.identifier + "(" + Application.version + ") Date: " +
                              DateTime.Now.ToShortDateString() + " started: " + DateTime.Now.ToShortTimeString() + " =========");
        }

        #endregion
    }
}
#endif
