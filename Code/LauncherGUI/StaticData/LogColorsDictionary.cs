namespace QArantineLauncher.Code.LauncherGUI.StaticData
{
    public static class LogColorsDictionary
    {
        public enum LogLevel
        {
            None,
            FatalError,
            Error,
            OK,
            Warning,
            Debug
        }

        public static readonly Dictionary<LogLevel, string> LogColorMap = new()
        {
            { LogLevel.None, "#99AAB5" },
            { LogLevel.FatalError, "#B44141" },
            { LogLevel.Error, "#DC4545" },
            { LogLevel.OK, "#3CB93C" },
            { LogLevel.Warning, "#DCB450" },
            { LogLevel.Debug, "#E6E6E6" },
        };
    }
}