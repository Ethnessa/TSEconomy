using TSEconomy.Database.Models;
using TSEconomy.Database.Models.Properties;

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

        public static void Log(String str, bool timeStamp = true)
        {
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

            writer.WriteLine($"{(timeStamp ? ("[" + DateTime.UtcNow + "] ") : "")}" + str);
        }


        public static void Log(Transaction trans)
        {
            string flag = trans.Flags == TransactionProperties.Set ? "Set" : "Add";

            Log($"[{trans.Timestamp}] [{flag}] [Cur:{trans.InternalCurrencyName}] [ID:{trans.UserID}]: {trans.TransactionDetails}", false);

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
