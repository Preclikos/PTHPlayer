using System;

namespace PTHLogger.Udp
{
    internal class UdpLogger : LoggerBase
    {
        private static readonly string[] LogLevelNames = Enum.GetNames(typeof(LogLevel));
        private delegate void LogMethod(string tag, string message, string file, string func, int line);

        private readonly Func<UdpLoggerService> GetService;

        public const string LogFormat = "{1}/{0}: {3}: {4}({5}) > {2}\r\n";

        public UdpLogger(string channel, LogLevel level, Func<UdpLoggerService> getService) : base(channel, level)
        {
            GetService = getService;
        }

        public override void PrintLog(LogLevel level, string message, string file, string method, int line)
        {
            var fullPathSpan = file.AsSpan();
            var fnameStart = fullPathSpan.LastIndexOfAny('/', '\\') + 1; //Handles found & not found cases (-1)
            var fileName = fullPathSpan.Slice(fnameStart).ToString();
            GetService()?.Log(Channel, LogLevelNames[(int)level], message, fileName, method, line);
        }
    }
}
