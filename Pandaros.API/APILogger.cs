using System;

namespace Pandaros.API
{
    internal static class APILogger
    {
        private static CSConsoleAndFileLogger _logger = new CSConsoleAndFileLogger(GameInitializer.NAMESPACE, "APILog", "<Panaros => API>");

        public static void LogToFile(string message, params object[] args)
        {
            _logger.LogToFile(message, args);
        }

        public static void Log(ChatColor color, string message, params object[] args)
        {
            _logger.Log(color, message, args);
        }

        public static void Log(string message, params object[] args)
        {
            _logger.Log(message, args);
        }

        public static void Log(string message)
        {
            _logger.Log(message);
        }

        public static void LogError(Exception e, string message)
        {
            _logger.LogError(e, message);
        }

        public static void LogError(Exception e, string message, params object[] args)
        {
            _logger.LogError(e, message, args);
        }

        public static void LogError(Exception e)
        {
            _logger.LogError(e);
        }
    }

}