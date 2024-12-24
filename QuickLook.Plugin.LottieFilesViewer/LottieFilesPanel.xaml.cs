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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace QuickLook.Plugin.LottieFilesViewer;

public partial class LottieFilesPanel : UserControl, INotifyPropertyChanged
{
    private const double ZoomFactor = 0.1d;
    private const double MinScale = 0.05d;
    private const double MaxScale = 5d;

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
        }
    }

    private string _scaling = "100%";

    public string Scaling
    {
        get => _scaling;
        set
        {
            if (value == _scaling) return;
            _scaling = value;
            OnPropertyChanged();
            if (ShowZoomLevelInfo)
                ((Storyboard)zoomLevelInfo.FindResource("StoryboardShowZoomLevelInfo")).Begin();
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

    public LottieFilesPanel()
    {
        DataContext = this;
        InitializeComponent();
        Loaded += LottieFilesPanel_Loaded;
    }

    private void LottieFilesPanel_Loaded(object sender, RoutedEventArgs e)
    {
        if (Window.GetWindow(this) is Window window)
        {
            window.MouseWheel += LottieFilesPanel_MouseWheel;
            window.Unloaded += LottieFilesPanel_Unloaded;
        }
    }

    private void LottieFilesPanel_Unloaded(object sender, RoutedEventArgs e)
    {
        if (Window.GetWindow(this) is Window window)
        {
            window.MouseWheel -= LottieFilesPanel_MouseWheel;
            window.Unloaded -= LottieFilesPanel_Unloaded;
        }
    }

    private void LottieFilesPanel_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        double delta = e.Delta > 0 ? ZoomFactor : -ZoomFactor;

        ScaleX = Math2.Clamp(ScaleX + delta, MinScale, MaxScale);
        ScaleY = Math2.Clamp(ScaleY + delta, MinScale, MaxScale);

        Scaling = $"{(int)(ScaleX * 100d)}%";
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
