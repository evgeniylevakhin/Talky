using System;
using NLog;

namespace Shared
{
    public class TalkyLog
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static bool IsVerbose { get; set; } = true;

        public static void Error(Exception ex)
        {
            Logger.Error(ex);
        }

        public static void Info(string p)
        {
            Logger.Info(p);
        }

        public static void Debug(string p)
        {
            if (IsVerbose) Logger.Debug(p);
        }

        public static void Error(string p)
        {
            Logger.Error(p);
        }
    }
}
