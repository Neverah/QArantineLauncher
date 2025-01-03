﻿using Avalonia;
using Avalonia.ReactiveUI;
using System;

namespace QArantineLauncher.Code.LauncherGUI;

sealed class AvaloniaStarter
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void StartAvaloniaApp()
    {
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(Array.Empty<string>());
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}