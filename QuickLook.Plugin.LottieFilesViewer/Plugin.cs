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

using QuickLook.Common.Helpers;
using QuickLook.Common.Plugin;
using QuickLook.Plugin.LottieFilesViewer.TinyJson;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace QuickLook.Plugin.LottieFilesViewer;

public class Plugin : IViewer
{
    private LottieFilesPanel? _panel;
    private string _jsonContent = null!;

    public int Priority => 2;

    public void Init()
    {
    }

    public bool CanHandle(string path)
    {
        if (!Directory.Exists(path))
        {
            if (new[] { ".lottie", ".lottie.json" }.Any(path.ToLower().EndsWith))
            {
                // Does not detect whether it is a lottie json file, try to read it directly
                return true;
            }
            else if (path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                var jsonString = File.ReadAllText(path);

                // No exception will be thrown here
                var json = jsonString.FromJson<Dictionary<string, object>>();

                if (json != null
                 && json.ContainsKey("v")
                 && json.ContainsKey("fr")
                 && json.ContainsKey("ip")
                 && json.ContainsKey("op")
                 && json.ContainsKey("layers"))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void Prepare(string path, ContextObject context)
    {
        if (path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            _jsonContent = File.ReadAllText(path);

            if (string.IsNullOrWhiteSpace(_jsonContent))
            {
                return;
            }

            // No exception will be thrown here
            var json = _jsonContent.FromJson<Dictionary<string, object>>();

            if (json != null)
            {
                if (json.ContainsKey("w")
                 && json.ContainsKey("h")
                 && double.TryParse(json["w"].ToString(), out double width)
                 && double.TryParse(json["h"].ToString(), out double height))
                {
                    context.PreferredSize = new Size(width, height);
                }
            }
        }
        else if (path.EndsWith(".lottie", StringComparison.OrdinalIgnoreCase))
        {
            using (var fileStream = File.OpenRead(path))
            using (var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Read))
            {
                var manifestEntry = zipArchive.GetEntry("manifest.json");
                List<string> idEntries = [];

                if (manifestEntry != null)
                {
                    using (var stream = manifestEntry.Open())
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        string content = reader.ReadToEnd();

                        if (!string.IsNullOrEmpty(content))
                        {
                            var manifestJson = content.FromJson<Dictionary<string, object>>();

                            if (manifestJson.ContainsKey("animations"))
                            {
                                object animations = manifestJson["animations"];

                                if (manifestJson["animations"] is IEnumerable<object> animationsEnumerable)
                                {
                                    foreach (var animationsItem in animationsEnumerable.ToArray())
                                    {
                                        if (animationsItem is Dictionary<string, object> animationsItemDict)
                                        {
                                            if (animationsItemDict.ContainsKey("id"))
                                            {
                                                idEntries.Add($"animations/{animationsItemDict["id"]}");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Read animations error from manifest.json and fallback to read all entries
                if (idEntries.Count == 0)
                {
                    foreach (var entry in zipArchive.Entries)
                    {
                        if (entry.FullName.StartsWith("animations"))
                        {
                            idEntries.Add(entry.FullName);
                        }
                    }
                }

                // Read the all animations
                if (idEntries.Count != 0)
                {
                    // I don't know if there are multiple animations
                    // But only support the first animation
                    var idEntry = $"{idEntries[0]}.json";
                    var animationEntry = zipArchive.GetEntry(idEntry);

                    if (animationEntry != null)
                    {
                        using (var stream = animationEntry.Open())
                        using (var reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            _jsonContent = reader.ReadToEnd();

                            // No exception will be thrown here
                            var json = _jsonContent.FromJson<Dictionary<string, object>>();

                            if (json != null)
                            {
                                if (json.ContainsKey("w")
                                 && json.ContainsKey("h")
                                 && double.TryParse(json["w"].ToString(), out double width)
                                 && double.TryParse(json["h"].ToString(), out double height))
                                {
                                    context.PreferredSize = new Size(width, height);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public void View(string path, ContextObject context)
    {
        var size = context.PreferredSize;

        context.Theme = (Themes)SettingHelper.Get("LastTheme", 1, "QuickLook.Plugin.ImageViewer");

        _panel = new LottieFilesPanel(context)
        {
            OriginalWidth = size.Width,
            OriginalHeight = size.Height,
            ViewerWidth = size.Width,
            ViewerHeight = size.Height,
        };

        if (!string.IsNullOrWhiteSpace(_jsonContent))
            _panel.JsonContent = _jsonContent;
        else
            _panel.FileName = path;

        context.ViewerContent = _panel;
        context.Title = size.IsEmpty
            ? $"{Path.GetFileName(path)}"
            : $"{size.Width}×{size.Height}: {Path.GetFileName(path)}";

        _panel.Dispatcher.Invoke(() => { context.IsBusy = false; }, DispatcherPriority.Loaded);
    }

    public void Cleanup()
    {
        GC.SuppressFinalize(this);
        _panel = null!;
    }
}
