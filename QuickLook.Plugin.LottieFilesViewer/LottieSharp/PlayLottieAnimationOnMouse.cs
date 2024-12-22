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

using Microsoft.Xaml.Behaviors;
using System.Linq;
using System.Windows;

namespace QuickLook.Plugin.LottieFilesViewer.LottieSharp;

public class PlayLottieAnimationOnMouse : Behavior<UIElement>
{
    public string LottieView { get; set; } = null!;

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.MouseEnter += AssociatedObject_MouseEnter;
        AssociatedObject.MouseLeave += AssociatedObject_MouseLeave;
        AssociatedObject.TouchUp += AssociatedObject_TouchUp;
    }

    private void AssociatedObject_TouchUp(object sender, System.Windows.Input.TouchEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(LottieView)) return;

        LottieAnimationView lottieView = AssociatedObject.FindChildren<LottieAnimationView>().FirstOrDefault(x => x.Name.ToLower() == LottieView.ToLower());
        if (lottieView == null) return;

        if (!lottieView.IsPlaying)
        {
            lottieView.PlayAnimation();
        }
    }

    protected override void OnDetaching()
    {
        AssociatedObject.MouseEnter -= AssociatedObject_MouseEnter;
        AssociatedObject.MouseLeave -= AssociatedObject_MouseLeave;
        AssociatedObject.TouchUp -= AssociatedObject_TouchUp;
        base.OnDetaching();
    }

    private void AssociatedObject_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(LottieView)) return;

        LottieAnimationView lottieView = AssociatedObject.FindChildren<LottieAnimationView>().FirstOrDefault(x => x.Name.ToLower() == LottieView.ToLower());
        if (lottieView == null) return;

        lottieView.StopAnimation();
    }

    private void AssociatedObject_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(LottieView)) return;

        LottieAnimationView lottieView = AssociatedObject.FindChildren<LottieAnimationView>().FirstOrDefault(x => x.Name.ToLower() == LottieView.ToLower());
        if (lottieView == null) return;

        if (!lottieView.IsPlaying)
        {
            lottieView.PlayAnimation();
        }
    }
}
