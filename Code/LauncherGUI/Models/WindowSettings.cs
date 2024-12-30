namespace QArantineLauncher.Code.LauncherGUI.Models
{
    public class WindowSettings(string windowTitle, double width, double height, int posX, int posY)
    {
        public string WindowTitle { get; set; } = windowTitle;
        public double Width { get; set; } = width;
        public double Height { get; set; } = height;
        public int PosX { get; set; } = posX;
        public int PosY { get; set; } = posY;
    }
}