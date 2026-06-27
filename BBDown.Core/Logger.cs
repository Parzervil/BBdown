namespace BBDown.Core
{
    public enum LogLevel
    {
        Info,
        Warning,
        Error,
        Color,
        Debug
    }

    public class LogEventArgs : EventArgs
    {
        public string Message { get; set; } = "";
        public LogLevel Level { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class Logger
    {
        public static event EventHandler<LogEventArgs>? LogMessage;

        private static void RaiseLogEvent(string message, LogLevel level)
        {
            LogMessage?.Invoke(null, new LogEventArgs
            {
                Message = message,
                Level = level,
                Timestamp = DateTime.Now
            });
        }

        public static void Log(object text, bool enter = true)
        {
            var msg = DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss.fff]") + " - " + text;
            if (enter)
            {
                Console.WriteLine(msg);
                RaiseLogEvent(msg, LogLevel.Info);
            }
            else
            {
                Console.Write(msg);
                RaiseLogEvent(msg, LogLevel.Info);
            }
        }

        public static void LogError(object text)
        {
            var msg = DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss.fff]") + " - " + text;
            Console.Write(DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss.fff]") + " - ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(text);
            Console.ResetColor();
            Console.WriteLine();
            RaiseLogEvent(msg, LogLevel.Error);
        }

        public static void LogColor(object text, bool time = true)
        {
            string msg;
            if (time)
            {
                msg = DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss.fff]") + " - " + text;
                Console.Write(msg);
            }
            else
            {
                msg = "                            " + text;
                Console.Write(msg);
            }
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine();
            Console.ResetColor();
            RaiseLogEvent(msg, LogLevel.Color);
        }

        public static void LogWarn(object text, bool time = true)
        {
            string msg;
            if (time)
            {
                msg = DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss.fff]") + " - " + text;
                Console.Write(msg);
            }
            else
            {
                msg = "                            " + text;
                Console.Write(msg);
            }
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine();
            Console.ResetColor();
            RaiseLogEvent(msg, LogLevel.Warning);
        }

        public static void LogDebug(string toFormat, params object[] args)
        {
            if (Config.DEBUG_LOG)
            {
                string msg;
                Console.ForegroundColor = ConsoleColor.DarkGray;
                if (args.Length > 0)
                    msg = DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss.fff]") + " - " + string.Format(toFormat, args).Trim();
                else
                    msg = DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss.fff]") + " - " + toFormat;
                Console.Write(msg);
                Console.ResetColor();
                Console.WriteLine();
                RaiseLogEvent(msg, LogLevel.Debug);
            }
        }
    }
}
