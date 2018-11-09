using DMO.Services.SettingsServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System.Threading;

namespace DMO.Utility.Logging
{
    /// <summary> 
    /// This is an advanced useage, where you want to intercept the logging messages and devert them somewhere 
    /// besides ETW. 
    /// </summary> 
    sealed class StorageFileEventListener : EventListener
    {
        /// <summary> 
        /// Storage file to be used to write logs 
        /// </summary> 
        private StorageFile _storageFile = null;

        /// <summary> 
        /// Name of the current event listener 
        /// </summary> 
        private string _name;

        /// <summary> 
        /// The format to be used by logging. 
        /// </summary> 
        private readonly string _format = "{0:yyyy-MM-dd HH\\:mm\\:ss\\:fff} \tLevel: {1, -13}\tType: {2, -13}\tMessage: '{3}'";

        /// <summary>
        /// Used to synchronize.
        /// </summary>
        private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);

        /// <summary>
        /// List of lines to log when _periodicTimer logs to file.
        /// </summary>
        private List<string> _logLines;

        /// <summary>
        /// Timer that logs to file every 5 seconds.
        /// </summary>
        private ThreadPoolTimer _periodicTimer;

        public StorageFileEventListener(string name)
        {
            _name = name;
            _logLines = new List<string>();

            Debug.WriteLine("StorageFileEventListener for {0} has name {1}", GetHashCode(), name);

            AssignLocalFile();
            Initialize();
        }

        private async void AssignLocalFile()
        {
            // Lock while setting up files.
            await _semaphoreSlim.WaitAsync();

            try
            {
                var logFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Logs", CreationCollisionOption.OpenIfExists);
                _storageFile = await logFolder.CreateFileAsync(_name.Replace(" ", "_") + ".log", CreationCollisionOption.OpenIfExists);

                var logProperties = await _storageFile.GetBasicPropertiesAsync();
                // If log file size is over 50 MegaBytes.
                if (logProperties.Size >= 50_000_000)
                {
                    // Delete old file.
                    await _storageFile.DeleteAsync();
                    // Create new.
                    AssignLocalFile();
                }
                else if (logProperties.Size > 0) // Only do the following for non-empty log files.
                {
                    // Append a few empty lines to show start of new application session.
                    await FileIO.AppendLinesAsync(_storageFile, new[] { "", "", "" });
                }
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        private void Initialize()
        {
            // We don't want to write to disk every single time a log event occurs, so let's schedule a
            // thread pool task
            _periodicTimer = ThreadPoolTimer.CreatePeriodicTimer(async (source) =>
            {
                // We have to lock when writing to disk as well, otherwise the in memory cache could change
                // or we might try to write lines to disk more than once
                await _semaphoreSlim.WaitAsync();
                try
                {
                    if (_logLines.Count > 0)
                    {
                        // Write synchronously here. We'll never be called on a UI thread and you
                        // cannot make an async call within a lock statement
                        FileIO.AppendLinesAsync(_storageFile, _logLines).AsTask().Wait();
                        _logLines.Clear();
                    }
                }
                finally
                {
                    _semaphoreSlim.Release();
                }
            }, TimeSpan.FromMilliseconds(100));
        }

        protected override async void OnEventWritten(EventWrittenEventArgs eventData)
        {
            if (_storageFile == null) return;

            // This could be called from any thread, and we want our logs in order, so lock here
            await _semaphoreSlim.WaitAsync();
            try
            {
                var newFormattedLine = string.Format(_format, DateTime.Now, eventData.Level, eventData.Payload[1], eventData.Payload[0]);

                // Only log to Debug console for the verbose StorageFileEventListener.
                if (_name.Equals("verbose"))
                    Debug.WriteLine(newFormattedLine);

                _logLines.Add(newFormattedLine);
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            Debug.WriteLine("OnEventSourceCreated for Listener {0} - {1} got eventSource {2}", GetHashCode(), _name, eventSource.Name);
        }
    }
}
