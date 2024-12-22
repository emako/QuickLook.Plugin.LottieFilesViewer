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

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace QuickLook.Plugin.LottieFilesViewer.LottieSharp;

public class AnimationInfo : INotifyPropertyChanged
{
    private string version = null!;
    private TimeSpan duration;
    private double fps;
    private double inPoint;
    private double outPoint;

    public AnimationInfo(string version, TimeSpan duration, double fps, double inPoint, double outPoint)
    {
        Version = version;
        Duration = duration;
        Fps = fps;
        InPoint = inPoint;
        OutPoint = outPoint;
    }

    public string Version
    {
        get => version; private
        set
        {
            version = value;
            OnPropertyChanged();
        }
    }

    public TimeSpan Duration
    {
        get => duration;
        private set
        {
            duration = value;
            OnPropertyChanged();
        }
    }

    public double Fps
    {
        get => fps;
        private set
        {
            fps = value;
            OnPropertyChanged();
        }
    }

    public double InPoint
    {
        get => inPoint;
        private set
        {
            inPoint = value;
            OnPropertyChanged();
        }
    }

    public double OutPoint
    {
        get => outPoint;
        private set
        {
            outPoint = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    // Create the OnPropertyChanged method to raise the event
    // The calling member's name will be used as the parameter.
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
