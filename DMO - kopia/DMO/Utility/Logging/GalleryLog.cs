using System.Diagnostics;

namespace DMO.Utility.Logging
{
    public class GalleryLog
    {
        /// <summary>
        /// Logs file query start.
        /// </summary>
        public static void FileQueryBegin()
        {
            //EventLog.Log.Info("Started file query");
        }

        /// <summary>
        /// Logs file query end.
        /// </summary>
        /// <param name="filesFound">Amount of files found through query.</param>
        public static void FileQueryEnd(Stopwatch sw, int filesFound)
        {
            EventLog.Log.Info($"File query completed, {filesFound} files found. Took {sw.ElapsedMilliseconds} ms");
        }

        /// <summary>
        /// Logs file parsing start.
        /// </summary>
        public static void FileParseBegin()
        {
            //EventLog.Log.Info("Started file parsing");
        }

        /// <summary>
        /// Logs file parse end.
        /// </summary>
        /// <param name="filesParsed">Amount of files parsed.</param>
        public static void FileParseEnd(Stopwatch sw, int filesParsed)
        {
            EventLog.Log.Info($"File parsing completed, {filesParsed} files parsed. Took {sw.ElapsedMilliseconds} ms");
        }

        /// <summary>
        /// Logs duplicate scanning start.
        /// </summary>
        public static void DuplicateScanBegin()
        {
            //EventLog.Log.Info("Started duplicate scanning");
        }

        /// <summary>
        /// Logs end of duplicate scanning.
        /// </summary>
        /// <param name="filesScanned">Amount of files scanned.</param>
        /// <param name="duplicatePairsFound">Amount of duplicate pairs found.</param>
        public static void DuplicateScanEnd(Stopwatch sw, int filesScanned, int duplicatePairsFound)
        {
            EventLog.Log.Info($"Scanning for duplicates finished. Scanned {filesScanned} files and found {duplicatePairsFound} duplicate pairs. Took {sw.ElapsedMilliseconds} ms");
        }

        /// <summary>
        /// Logs online image evaluation start.
        /// </summary>
        public static void EvaluateOnlineBegin()
        {
            EventLog.Log.Info("Started online image evaluation");
        }

        /// <summary>
        /// Logs end of online image evaluation.
        /// </summary>
        /// <param name="filesEvaluated">Amount of files evaluated online.</param>
        public static void EvaluateOnlineEnd(Stopwatch sw, int filesEvaluated)
        {
            EventLog.Log.Info($"Image evaluation online completed, {filesEvaluated} files evaluated. Took {sw.ElapsedMilliseconds} ms");
            EventLog.Log.Info($"Average time per file {sw.ElapsedMilliseconds / ((float)filesEvaluated == 0 ? 1 : (float)filesEvaluated)} ms");
        }

        /// <summary>
        /// Logs local image evaluation start.
        /// </summary>
        public static void EvaluateLocalBegin()
        {
            EventLog.Log.Info("Started local image evaluation");
        }

        /// <summary>
        /// Logs end of local image evaluation.
        /// </summary>
        /// <param name="filesEvaluated">Amount of files evaluated locally.</param>
        public static void EvaluateLocalEnd(Stopwatch sw, int filesEvaluated)
        {
            EventLog.Log.Info($"Local image evaluation completed, {filesEvaluated} files evaluated. Took {sw.ElapsedMilliseconds} ms");
            EventLog.Log.Info($"Average time per file {sw.ElapsedMilliseconds / ((float)filesEvaluated == 0 ? 1 : (float)filesEvaluated)} ms");
        }

        /// <summary>
        /// Logs the reason for saving all Metadatas right before saving.
        /// </summary>
        public static void QueryContentsChangedSaving()
        {
            EventLog.Log.Info("Changes to folder has been recorded and processed. Saving results");
        }

        /// <summary>
        /// Logs cloud vision request sending.
        /// </summary>
        public static void CloudVisionRequestBegin()
        {
            //EventLog.Log.Debug("Cloud vision request sent. Now awaiting result");
        }

        /// <summary>
        /// Logs cloud vision request recieving.
        /// </summary>
        public static void CloudVisionRequestEnd(Stopwatch sw)
        {
            EventLog.Log.Debug($"Cloud vision request completed! Waited {sw.ElapsedMilliseconds} ms for response");
        }
    }
}
