using TSEconomy.Database.Models;
using TSEconomy.Database.Models.Properties;

namespace TSEconomy.Logging
{
    public class TransactionLogging
    {
        public static DateTime? SessionLog { get; set; }
        public static string safeLogSession
        {
            get
            {
                string formattedSessionLog = SessionLog?.ToString("yyyy-MM-dd HH-mm-ss tt") ?? "Unknown";
                return formattedSessionLog.Replace(':', '-').Replace(' ', '_');
            }
        }
        public static string logPath => TSEconomy.Config.TransactionLogPath;

        public static void Log(Transaction trans)
        {
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }

            if (SessionLog == null)
            {
                SessionLog = DateTime.Now;
            }

            var sessionLogFile = Path.Combine(logPath, $"{safeLogSession}.txt");
            if (!File.Exists(sessionLogFile))
            {
                File.Create(sessionLogFile).Close(); // Close the file stream after creating it.
            }

            using (StreamWriter writer = File.AppendText(sessionLogFile))
            {
                string flag = trans.Flags == TransactionProperties.Set ? "Set" : "Add";

                writer.WriteLine($"[{trans.Timestamp}] [{flag}] [Cur:{trans.InternalCurrencyName}] [ID:{trans.UserID}]: {trans.TransactionDetails}");
            }
        }


    }
}
