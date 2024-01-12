﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using ClassIsland.Controls;
using ClassIsland.Models;
using ClassIsland.Services;
using ClassIsland.ViewModels;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using WindowsShortcutFactory;
using Path = System.IO.Path;

namespace ClassIsland.Views;
/// <summary>
/// WelcomeWindow.xaml 的交互逻辑
/// </summary>
public partial class WelcomeWindow : MyWindow
{
    public WelcomeViewModel ViewModel
    {
        get;
        set;
    } = new();

    public SettingsService SettingsService { get; } = App.GetService<SettingsService>();

    public WelcomeWindow()
    {
        DataContext = this;
        InitializeComponent();
        var reader = new StreamReader(Application.GetResourceStream(new Uri("/Assets/License.txt", UriKind.Relative))!
            .Stream);
        ViewModel.License = reader.ReadToEnd();
        ViewModel.Settings = SettingsService.Settings;
    }

    protected override async void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        ViewModel.MasterTabIndex = 1;
    }

    private void ButtonClose_OnClick(object sender, RoutedEventArgs e)
    {
        ViewModel.IsExitConfirmed = true;
        DialogResult = true;
        var startupPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "ClassIsland.lnk");
        var startMenuPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "ClassIsland.lnk");
        var desktopPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "ClassIsland.lnk");
        using var shortcut = new WindowsShortcut
        {
            Path = Environment.ProcessPath,
            WorkingDirectory = Environment.CurrentDirectory
        };
        try
        {
            if (ViewModel.CreateStartupShortcut)
                shortcut.Save(startupPath);
            if (ViewModel.CreateStartMenuShortcut)
                shortcut.Save(startMenuPath);
            if (ViewModel.CreateDesktopShortcut)
                shortcut.Save(desktopPath);
        }
        catch (Exception ex)
        {
            App.GetService<ILogger<WelcomeWindow>>().LogError(ex, "无法创建快捷方式。");
        }
        
        Close();
    }

    private async void WelcomeWindow_OnClosing(object? sender, CancelEventArgs e)
    {
        if (ViewModel.IsExitConfirmed)
        {
            return;
        }

        e.Cancel = true;
        var r = await DialogHost.Show(FindResource("ExitAppConfirmDialog"), ViewModel.DialogId);
        if ((bool?)r == true)
        {
            ViewModel.IsExitConfirmed = true;
            Close();
        }
    }

    private void ButtonViewHelp_OnClick(object sender, RoutedEventArgs e)
    {
        ViewModel.IsExitConfirmed = true;
        DialogResult = true;
        Close();

        var mw = (MainWindow)Application.Current.MainWindow!;
        mw.OpenHelpsWindow();
    }

    private void HyperlinkMsAppCenter_OnClick(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo()
        {
            FileName = "https://learn.microsoft.com/zh-cn/appcenter/sdk/data-collected",
            UseShellExecute = true
        });
    }

    private void ButtonFlipNext_OnClick(object sender, RoutedEventArgs e)
    {
        ViewModel.FlipNextCount++;
        ViewModel.FlipIndex = ViewModel.FlipIndex + 1 >= 3 ? 0 : ViewModel.FlipIndex + 1;
        if (ViewModel.FlipNextCount >= 2)
            ViewModel.IsFlipEnd = true;
    }
}