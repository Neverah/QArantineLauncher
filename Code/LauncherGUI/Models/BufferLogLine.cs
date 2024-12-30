namespace QArantineLauncher.Code.LauncherGUI.Models
{
    public class BufferLogLine(
            string timestamp,
            string timestampForegroundHexCode,
            string testTag,
            string testTagForegroundHexCode,
            string logBody,
            string logBodyForegroundHexCode
        ) : EventArgs
    {
        public string Timestamp { get; set; } = timestamp ?? "";
        public string TimestampForegroundHexCode { get; set; } = timestampForegroundHexCode ?? "#FFFFFF";
        public string TestTag { get; set; } = testTag ?? "";
        public string TestTagForegroundHexCode { get; set; } = testTagForegroundHexCode ?? "#FFFFFF";
        public string LogBody { get; set; } = logBody ?? "";
        public string LogBodyForegroundHexCode { get; set; } = logBodyForegroundHexCode ?? "#FFFFFF";
    }
}