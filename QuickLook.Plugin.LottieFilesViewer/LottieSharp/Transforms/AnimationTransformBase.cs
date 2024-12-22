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

using System.Windows;
using System.Windows.Controls;

namespace QuickLook.Plugin.LottieFilesViewer.LottieSharp.Transforms;

public class AnimationTransformBase : Control
{
    public float ScaleX
    {
        get => (float)GetValue(ScaleXProperty);
        set => SetValue(ScaleXProperty, value);
    }

    // Using a DependencyProperty as the backing store for ScaleX.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ScaleXProperty =
        DependencyProperty.Register(nameof(ScaleX), typeof(float), typeof(AnimationTransformBase), new PropertyMetadata(0.0f));

    public float ScaleY
    {
        get => (float)GetValue(ScaleYProperty);
        set => SetValue(ScaleYProperty, value);
    }

    // Using a DependencyProperty as the backing store for ScaleY.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ScaleYProperty =
        DependencyProperty.Register(nameof(ScaleY), typeof(float), typeof(AnimationTransformBase), new PropertyMetadata(0.0f));

    public float CenterX
    {
        get => (float)GetValue(CenterXProperty);
        set => SetValue(CenterXProperty, value);
    }

    // Using a DependencyProperty as the backing store for CenterX.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty CenterXProperty =
        DependencyProperty.Register(nameof(CenterX), typeof(float), typeof(AnimationTransformBase), new PropertyMetadata(0.0f));

    public float CenterY
    {
        get => (float)GetValue(CenterYProperty);
        set => SetValue(CenterYProperty, value);
    }

    // Using a DependencyProperty as the backing store for CenterY.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty CenterYProperty =
        DependencyProperty.Register(nameof(CenterY), typeof(float), typeof(AnimationTransformBase), new PropertyMetadata(0.0f));
}
