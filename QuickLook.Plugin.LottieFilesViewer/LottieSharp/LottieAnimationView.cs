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

using QuickLook.Plugin.LottieFilesViewer.LottieSharp.Transforms;
using SkiaSharp;
using SkiaSharp.Skottie;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Resources;
using System.Windows.Threading;

namespace QuickLook.Plugin.LottieFilesViewer.LottieSharp;

public class LottieAnimationView : SKElement
{
    private readonly Stopwatch watch = new();
    private Animation animation = null!;
    private DispatcherTimer timer = null!;
    private int loopCount;

    public AnimationInfo Info
    {
        get => (AnimationInfo)GetValue(InfoProperty);
        set => SetValue(InfoProperty, value);
    }

    public event EventHandler? OnStop;

    public string FileName
    {
        get => (string)GetValue(FileNameProperty);
        set => SetValue(FileNameProperty, value);
    }

    public string ResourcePath
    {
        get => (string)GetValue(ResourcePathProperty);
        set => SetValue(ResourcePathProperty, value);
    }

    public string JsonContent
    {
        get => (string)GetValue(JsonContentProperty);
        set => SetValue(JsonContentProperty, value);
    }

    public virtual void PlayAnimation()
    {
        timer.Start();
        watch.Start();
        IsPlaying = true;
    }

    public virtual void StopAnimation()
    {
        loopCount = RepeatCount;
        timer.Stop();
        watch.Reset();
        IsPlaying = false;

        OnStop?.Invoke(this, null);
    }

    public int RepeatCount
    {
        get => (int)GetValue(RepeatCountProperty);
        set => SetValue(RepeatCountProperty, value);
    }

    public static readonly DependencyProperty RepeatCountProperty =
        DependencyProperty.Register(nameof(RepeatCount), typeof(int), typeof(LottieAnimationView), new PropertyMetadata(0, RepeatCountChangedCallback));

    private static void RepeatCountChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LottieAnimationView lottieAnimationView)
        {
            lottieAnimationView.loopCount = (int)e.NewValue;
        }
    }

    public AnimationTransformBase AnimationScale
    {
        get => (AnimationTransformBase)GetValue(AnimationScaleProperty);
        set => SetValue(AnimationScaleProperty, value);
    }

    // Using a DependencyProperty as the backing store for AnimationScale.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty AnimationScaleProperty =
        DependencyProperty.Register(nameof(AnimationScale), typeof(AnimationTransformBase), typeof(LottieAnimationView), new PropertyMetadata(default(AnimationTransformBase)));

    public bool AutoPlay
    {
        get => (bool)GetValue(AutoStartProperty);
        set => SetValue(AutoStartProperty, value);
    }

    public static readonly DependencyProperty AutoStartProperty =
        DependencyProperty.Register(nameof(AutoPlay), typeof(bool), typeof(LottieAnimationView), new PropertyMetadata(false, AutoPlayPropertyChangedCallback));

    public static readonly DependencyProperty InfoProperty =
        DependencyProperty.Register(nameof(Info), typeof(AnimationInfo), typeof(LottieAnimationView));

    private static void AutoPlayPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        // Not in use at the moment
    }

    public bool IsPlaying
    {
        get => (bool)GetValue(IsPlayingProperty);
        set => SetValue(IsPlayingProperty, value);
    }

    public static readonly DependencyProperty IsPlayingProperty =
        DependencyProperty.Register(nameof(IsPlaying), typeof(bool), typeof(LottieAnimationView), new PropertyMetadata(false));

    public RepeatMode Repeat
    {
        get => (RepeatMode)GetValue(RepeatProperty);
        set => SetValue(RepeatProperty, value);
    }

    public static readonly DependencyProperty RepeatProperty =
        DependencyProperty.Register(nameof(Repeat), typeof(RepeatMode), typeof(LottieAnimationView), new PropertyMetadata(RepeatMode.Restart));

    public static readonly DependencyProperty FileNameProperty =
        DependencyProperty.Register(nameof(FileName), typeof(string), typeof(LottieAnimationView), new PropertyMetadata(null, FileNamePropertyChangedCallback));

    public static readonly DependencyProperty ResourcePathProperty =
        DependencyProperty.Register(nameof(ResourcePath), typeof(string), typeof(LottieAnimationView), new PropertyMetadata(null, ResourcePathPropertyChangedCallback));

    public static readonly DependencyProperty JsonContentProperty =
        DependencyProperty.Register(nameof(JsonContent), typeof(string), typeof(LottieAnimationView), new PropertyMetadata(null, JsonContentPropertyChangedCallback));

    private static void FileNamePropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is LottieAnimationView lottieAnimationView && e.NewValue is string assetName)
        {
            lottieAnimationView.SetAnimationFromFile(assetName);
        }
    }

    private static void ResourcePathPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is LottieAnimationView lottieAnimationView && e.NewValue is string assetName)
        {
            lottieAnimationView.SetAnimationFromResource(assetName);
        }
    }

    private static void JsonContentPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is LottieAnimationView lottieAnimationView && e.NewValue is string assetName)
        {
            lottieAnimationView.SetAnimationFromJsonContent(assetName);
        }
    }

    private void SetAnimationFromFile(string assetName)
    {
        try
        {
            if (!File.Exists(assetName))
            {
                return;
            }

            using FileStream stream = File.OpenRead(assetName);
            SetAnimation(stream);
        }
        catch (IOException)
        {
            Debug.WriteLine($"Failed to load {assetName}");
            throw;
        }
        catch (Exception)
        {
            Debug.WriteLine($"Unexpected error when loading {assetName}");
            throw;
        }
    }

    private void SetAnimationFromResource(string assetUri)
    {
        if (DesignerProperties.GetIsInDesignMode(this))
        {
            return;
        }

        try
        {
            var resourceUri = new Uri(assetUri);
            StreamResourceInfo resourceInfo = Application.GetResourceStream(resourceUri);

            SetAnimation(resourceInfo?.Stream!);
        }
        catch (IOException)
        {
            Debug.WriteLine($"Failed to load resource {assetUri}");
            throw;
        }
        catch (UriFormatException)
        {
            Debug.WriteLine($"Resource URI failure for resource {assetUri}");
            throw;
        }
        catch (Exception)
        {
            Debug.WriteLine($"Unexpected error when loading resource {assetUri}");
            throw;
        }
    }

    private void SetAnimationFromJsonContent(string assetContent)
    {
        try
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(assetContent);
            using var stream = new MemoryStream(byteArray);
            SetAnimation(stream);
        }
        catch (Exception)
        {
            Debug.WriteLine($"Unexpected error when loading {assetContent}");
            throw;
        }
    }

    private void SetAnimation(Stream stream)
    {
        using SKManagedStream fileStream = new(stream);

        if (Animation.TryCreate(fileStream, out animation!))
        {
            animation.Seek(0);
            Info = new AnimationInfo(animation.Version, animation.Duration, animation.Fps, animation.InPoint,
                animation.OutPoint);
        }
        else
        {
            Info = new AnimationInfo(string.Empty, TimeSpan.Zero, 0, 0, 0);
            throw new InvalidOperationException("Failed to load animation");
        }

        watch.Reset();
        if (timer == null)
        {
            timer = new DispatcherTimer(DispatcherPriority.Render);
            timer.Interval = TimeSpan.FromSeconds(Math.Max(1 / 60.0, 1 / animation.Fps));
            timer.Tick += (s, e) => { InvalidateVisual(); };
        }
        else
        {
            timer.Stop();
            timer.Interval = TimeSpan.FromSeconds(Math.Max(1 / 60.0, 1 / animation.Fps));
        }

        if (AutoPlay || IsPlaying)
        {
            PlayAnimation();
        }
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        base.OnPaintSurface(e);

        SKCanvas canvas = e.Surface.Canvas;
        canvas.Clear(SKColor.Empty);
        SKImageInfo info = e.Info;

        if (animation != null)
        {
            animation.SeekFrameTime((float)watch.Elapsed.TotalSeconds);

            if (watch.Elapsed.TotalSeconds > animation.Duration.TotalSeconds)
            {
                if (Repeat == RepeatMode.Restart)
                {
                    if (RepeatCount == Defaults.RepeatCountInfinite)
                    {
                        watch.Restart();
                    }
                    else if (RepeatCount > 0 && loopCount > 0)
                    {
                        loopCount--;
                        watch.Restart();
                    }
                    else
                    {
                        StopAnimation();
                    }
                }
            }

            if (AnimationScale is CenterTransform)
            {
                canvas.Scale(AnimationScale.ScaleX, AnimationScale.ScaleY, info.Width / 2, info.Height / 2);
            }
            else if (AnimationScale != null)
            {
                canvas.Scale(AnimationScale.ScaleX, AnimationScale.ScaleY, AnimationScale.CenterX, AnimationScale.CenterY);
            }

            animation.Render(canvas, new SKRect(0, 0, info.Width, info.Height));
        }
    }
}
