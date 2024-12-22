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

using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace QuickLook.Plugin.LottieFilesViewer.LottieSharp;

/// <summary>
/// Helper methods for UI-related tasks.
/// Note: this is commom implementation found on web, i.e.: https://stackoverflow.com/questions/636383/how-can-i-find-wpf-controls-by-name-or-type
/// </summary>
public static class CustomVisualTreeHelper
{
    #region find parent

    /// <summary>
    /// Finds a parent of a given item on the visual tree.
    /// </summary>
    /// <typeparam name="T">The type of the queried item.</typeparam>
    /// <param name="child">A direct or indirect child of the
    /// queried item.</param>
    /// <returns>The first parent item that matches the submitted
    /// type parameter. If not matching item can be found, a null
    /// reference is being returned.</returns>
    public static T TryFindParent<T>(this DependencyObject child)
        where T : DependencyObject
    {
        //get parent item
        DependencyObject parentObject = GetParentObject(child);

        //we've reached the end of the tree
        if (parentObject == null) return null!;

        //check if the parent matches the type we're looking for
        if (parentObject is T parent)
        {
            return parent;
        }
        else
        {
            //use recursion to proceed with next level
            return TryFindParent<T>(parentObject);
        }
    }

    /// <summary>
    /// This method is an alternative to WPF's
    /// <see cref="VisualTreeHelper.GetParent"/> method, which also
    /// supports content elements. Keep in mind that for content element,
    /// this method falls back to the logical tree of the element!
    /// </summary>
    /// <param name="child">The item to be processed.</param>
    /// <returns>The submitted item's parent, if available. Otherwise
    /// null.</returns>
    public static DependencyObject GetParentObject(this DependencyObject child)
    {
        if (child == null) return null!;

        //handle content elements separately
        if (child is ContentElement contentElement)
        {
            DependencyObject parent = ContentOperations.GetParent(contentElement);
            if (parent != null) return parent;

            return contentElement is FrameworkContentElement fce ? fce.Parent : null!;
        }

        //also try searching for parent in framework elements (such as DockPanel, etc)
        if (child is FrameworkElement frameworkElement)
        {
            DependencyObject parent = frameworkElement.Parent;
            if (parent != null) return parent;
        }

        //if it's not a ContentElement/FrameworkElement, rely on VisualTreeHelper
        return VisualTreeHelper.GetParent(child);
    }

    #endregion find parent

    #region find children

    /// <summary>
    /// Analyzes both visual and logical tree in order to find all elements of a given
    /// type that are descendants of the <paramref name="source"/> item.
    /// </summary>
    /// <typeparam name="T">The type of the queried items.</typeparam>
    /// <param name="source">The root element that marks the source of the search. If the
    /// source is already of the requested type, it will not be included in the result.</param>
    /// <returns>All descendants of <paramref name="source"/> that match the requested type.</returns>
    public static IEnumerable<T> FindChildren<T>(this DependencyObject source) where T : DependencyObject
    {
        if (source != null)
        {
            IEnumerable<DependencyObject> childs = GetChildObjects(source);
            foreach (DependencyObject child in childs)
            {
                //analyze if children match the requested type
                if (child != null && child is T t)
                {
                    yield return t;
                }

                //recurse tree
                foreach (T descendant in FindChildren<T>(child!))
                {
                    yield return descendant;
                }
            }
        }
    }

    /// <summary>
    /// This method is an alternative to WPF's
    /// <see cref="VisualTreeHelper.GetChild"/> method, which also
    /// supports content elements. Keep in mind that for content elements,
    /// this method falls back to the logical tree of the element.
    /// </summary>
    /// <param name="parent">The item to be processed.</param>
    /// <returns>The submitted item's child elements, if available.</returns>
    public static IEnumerable<DependencyObject> GetChildObjects(this DependencyObject parent)
    {
        if (parent == null) yield break;

        if (parent is ContentElement || parent is FrameworkElement)
        {
            //use the logical tree for content / framework elements
            foreach (object obj in LogicalTreeHelper.GetChildren(parent))
            {
                if (obj is DependencyObject depObj) yield return (DependencyObject)obj;
            }
        }
        else
        {
            //use the visual tree per default
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                yield return VisualTreeHelper.GetChild(parent, i);
            }
        }
    }

    #endregion find children

    #region find from point

    /// <summary>
    /// Tries to locate a given item within the visual tree,
    /// starting with the dependency object at a given position.
    /// </summary>
    /// <typeparam name="T">The type of the element to be found
    /// on the visual tree of the element at the given location.</typeparam>
    /// <param name="reference">The main element which is used to perform
    /// hit testing.</param>
    /// <param name="point">The position to be evaluated on the origin.</param>
    public static T TryFindFromPoint<T>(UIElement reference, Point point)
        where T : DependencyObject
    {
        if (reference.InputHitTest(point) is not DependencyObject element) return null!;
        else return element is T t ? t : TryFindParent<T>(element);
    }

    #endregion find from point
}
