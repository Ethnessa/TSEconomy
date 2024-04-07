using TSEconomy.Database.Models;
using TSEconomy.Database.Models.Properties;
using TShockAPI;

namespace TSEconomy.Logging
{
    public class TransactionLogging
    {
        public static DateTime? SessionLog { get; set; }
        public static string SafeLogSession
        {
            get
            {
                string formattedSessionLog = SessionLog?.ToString("yyyy-MM-dd HH-mm-ss tt") ?? "Unknown";
                return formattedSessionLog.Replace(':', '-').Replace(' ', '_');
            }
        }
        public static string LogPath => TSEconomy.Config.TransactionLogPath;
        public static List<string> LogsToWrite { get; set; }
        public static void Log(string str, bool timeStamp = true)
        {
            LogsToWrite.Add($"{(timeStamp ? ("[" + DateTime.UtcNow + "] ") : "")}" + str);

            if (LogsToWrite.Count > TSEconomy.Config.MaxLogFilesAllowed) ForceWrite();
        }
        public static void ForceWrite()
        {
            if (LogsToWrite.Count == 0) return;

            if (!Directory.Exists(LogPath))
            {
                Directory.CreateDirectory(LogPath);
            }

            if (SessionLog == null)
            {
                SessionLog = DateTime.Now;
            }

            var sessionLogFile = Path.Combine(LogPath, $"{SafeLogSession}.txt");
            if (!File.Exists(sessionLogFile))
            {
                File.Create(sessionLogFile).Close(); // Close the file stream after creating it.
            }

            using StreamWriter writer = File.AppendText(sessionLogFile);

            foreach (string str in LogsToWrite) writer.WriteLine(str);

            LogsToWrite.Clear();
            
        }

        public static void Log(Transaction trans)
        {
            string flag = trans.Flags == TransactionProperties.Set ? "Set" : "Add";
            Log($"[{trans.Timestamp}] [{flag}] [Cur:{trans.InternalCurrencyName}] [ID:{trans.UserID}]: {trans.TransactionDetails}", false);

        }

        public static async Task LogAsync(Transaction trans)
        {
            await Task.Run(() => Log(trans));
        }

        public static async Task LogAsync(string str, bool timeStamp = true)
        {
            await Task.Run(() => Log(str, timeStamp));
        }

        public static async Task ForceWriteAsync()
        {
            await Task.Run(() => ForceWrite());
        }
        public static void PurgeOldLogs()
        {
            if (!Directory.Exists(LogPath))
            {
                Directory.CreateDirectory(LogPath);
                return;
            }

            var files = Directory.GetFiles(LogPath);

            if (files.Length <= Api.Config.MaxLogFilesAllowed) return;

            Dictionary<string,DateTime> fileDates = new();
            foreach ( var file in files)
                fileDates.Add( file, File.GetCreationTimeUtc(Path.Combine(LogPath, file)));

            var fileDatesList = fileDates.ToList();

            fileDatesList.OrderBy(i => i.Value);

            foreach ( var file in fileDatesList)
            {
                File.Delete(Path.Combine(LogPath, file.Key));

                fileDates.Remove(file.Key);

                if (fileDates.Count <= Api.Config.MaxLogFilesAllowed) return;
            }
        }



    }
}
