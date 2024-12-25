// Copyright © 2024 QL-Win Contributors
//
// This file is part of QuickLook program.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using QuickLook.Common.Annotations;
using QuickLook.Common.Helpers;
using QuickLook.Common.Plugin;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace QuickLook.Plugin.LottieFilesViewer;

public partial class LottieFilesPanel : UserControl, INotifyPropertyChanged
{
    private const double zoomFactor = 0.1d;
    private const double minScale = 0.05d;
    private const double maxScale = 5d;
    private DispatcherTimer _timer = new();

    private ContextObject _contextObject = null!;

    public ContextObject ContextObject
    {
        get => _contextObject;
        set
        {
            _contextObject = value;
            OnPropertyChanged();
        }
    }

    public Themes Theme
    {
        get => ContextObject?.Theme ?? Themes.Dark;
        set
        {
            ContextObject.Theme = value;
            OnPropertyChanged();
        }
    }

    private string _fileName = null!;

    public string FileName
    {
        get => _fileName;
        set
        {
            if (value == _fileName) return;
            _fileName = value;
            OnPropertyChanged();
        }
    }

    private string _jsonContent = null!;

    public string JsonContent
    {
        get => _jsonContent;
        set
        {
            if (value == _jsonContent) return;
            _jsonContent = value;
            OnPropertyChanged();
        }
    }

    public double OriginalWidth = 500d;
    public double OriginalHeight = 500d;

    private double _viewerWidth = 500d;

    public double ViewerWidth
    {
        get => _viewerWidth;
        set
        {
            if (value == _viewerWidth) return;
            _viewerWidth = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CenterX));
        }
    }

    private double _viewerHeight = 500d;

    public double ViewerHeight
    {
        get => _viewerHeight;
        set
        {
            if (value == _viewerHeight) return;
            _viewerHeight = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CenterY));
        }
    }

    public double CenterX => ViewerWidth / 2d;

    public double CenterY => ViewerHeight / 2d;

    private double _scaleX = 1d;

    public double ScaleX
    {
        get => _scaleX;
        set
        {
            if (value == _scaleX) return;
            _scaleX = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Scaling));
        }
    }

    private double _scaleY = 1d;

    public double ScaleY
    {
        get => _scaleY;
        set
        {
            if (value == _scaleY) return;
            _scaleY = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Scaling));
        }
    }

    private double _scaling = 1d;

    public double Scaling
    {
        get => _scaling;
        set
        {
            if (value == _scaling) return;
            _scaling = value;
            OnPropertyChanged();
        }
    }

    private bool _showZoomLevelInfo = true;

    public bool ShowZoomLevelInfo
    {
        get => _showZoomLevelInfo;
        set
        {
            if (value == _showZoomLevelInfo) return;
            _showZoomLevelInfo = value;
            OnPropertyChanged();
        }
    }

    public LottieFilesPanel(ContextObject context)
    {
        ContextObject = context;
        Theme = ContextObject.Theme;
        DataContext = this;
        InitializeComponent();

        buttonBackgroundColour.Click += OnBackgroundColourOnClick;

        Loaded += LottieFilesPanel_Loaded;

        _timer.Interval = TimeSpan.FromMilliseconds(5);
        _timer.Tick += Timer_Tick;
    }

    private void OnBackgroundColourOnClick(object sender, RoutedEventArgs e)
    {
        Theme = Theme == Themes.Dark ? Themes.Light : Themes.Dark;

        SettingHelper.Set("LastTheme", (int)Theme, "QuickLook.Plugin.ImageViewer");
    }

    private void LottieFilesPanel_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= LottieFilesPanel_Loaded;

        if (Window.GetWindow(this) is Window window)
        {
            window.MouseWheel += Window_MouseWheel;
            window.Unloaded += Window_Unloaded;
        }
    }

    private void Window_Unloaded(object sender, RoutedEventArgs e)
    {
        if (Window.GetWindow(this) is Window window)
        {
            window.MouseWheel -= Window_MouseWheel;
            window.Unloaded -= Window_Unloaded;
            _timer.Stop();
        }
    }

    private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        Debug.WriteLine($"LottieFilesPanel_MouseWheel_Before:ScaleX={ScaleX}|ScaleY={ScaleY}|Scaling={Scaling}|ViewerWidth={ViewerWidth}|ViewerHeight={ViewerHeight}|ViewerWidth / OriginalWidth={ViewerWidth / OriginalWidth}");

        double delta = e.Delta > 0 ? zoomFactor : -zoomFactor;

        // Blur zoom
        ScaleX = Math2.Clamp(Scaling + delta, minScale, maxScale);
        ScaleY = Math2.Clamp(Scaling + delta, minScale, maxScale);

        Scaling = ScaleX;

        Debug.WriteLine($"LottieFilesPanel_MouseWheel_After:ScaleX={ScaleX}|ScaleY={ScaleY}|Scaling={Scaling}|ViewerWidth={ViewerWidth}|ViewerHeight={ViewerHeight}|ViewerWidth / OriginalWidth={ViewerWidth / OriginalWidth}");

        if (ShowZoomLevelInfo)
        {
            if (zoomLevelInfo.FindResource("StoryboardShowZoomLevelInfo") is Storyboard storyboard)
            {
                storyboard.Begin();
            }
        }

        _timer.Stop();
        _timer.Start();
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        Debug.WriteLine($"LottieFilesPanel_TimerTick_Before:ScaleX={ScaleX}|ScaleY={ScaleY}|Scaling={Scaling}|ViewerWidth={ViewerWidth}|ViewerHeight={ViewerHeight}|ViewerWidth / OriginalWidth={ViewerWidth / OriginalWidth}");

        _timer.Stop();

        // Apply zoom
        ViewerWidth = ScaleX * OriginalWidth;
        ViewerHeight = ScaleY * OriginalHeight;

        ScaleX = 1d;
        ScaleY = 1d;

        Scaling = Math2.Clamp(ViewerWidth / OriginalWidth, minScale, maxScale);

        Debug.WriteLine($"LottieFilesPanel_TimerTick_After:ScaleX={ScaleX}|ScaleY={ScaleY}|Scaling={Scaling}|ViewerWidth={ViewerWidth}|ViewerHeight={ViewerHeight}|ViewerWidth / OriginalWidth={ViewerWidth / OriginalWidth}");
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

file static class Math2
{
    public static double Clamp(double value, double min, double max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    public static int Clamp(int value, int min, int max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }
}
