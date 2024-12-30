namespace QArantineLauncher.Code.Init
{
    public class LauncherInitializer
    {
        private static bool _isQArantineInitialized = false;

        public static void StartGUI()
        {
            if (!IsQArantineInitialized())
            {
                AvaloniaStarter.StartAvaloniaApp();
                _isQArantineInitialized = true;
            }
            else
            {
                LogWarning("It seems that the QArantine has been asked to initialize when it was already initialized, make sure that this is intentional");
            }
        }

        public static bool IsQArantineInitialized()
        {
            return _isQArantineInitialized;
        }
    }
}