using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace MALClient.WinUI.Compat
{
    /// <summary>
    ///     Replacement for the WinRTXamlToolkit visual tree helpers used by the ported UWP pages.
    /// </summary>
    public static class VisualTreeExtensions
    {
        public static IEnumerable<DependencyObject> GetChildren(this DependencyObject parent)
        {
            var count = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < count; i++)
                yield return VisualTreeHelper.GetChild(parent, i);
        }

        /// <summary>
        ///     Breadth first enumeration of all descendants.
        /// </summary>
        public static IEnumerable<DependencyObject> GetDescendants(this DependencyObject start)
        {
            var queue = new Queue<DependencyObject>();
            foreach (var child in start.GetChildren())
                queue.Enqueue(child);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                yield return current;
                foreach (var child in current.GetChildren())
                    queue.Enqueue(child);
            }
        }

        public static IEnumerable<T> GetDescendantsOfType<T>(this DependencyObject start) where T : DependencyObject
        {
            return start.GetDescendants().OfType<T>();
        }

        public static T GetFirstDescendantOfType<T>(this DependencyObject start) where T : DependencyObject
        {
            return start.GetDescendantsOfType<T>().FirstOrDefault();
        }

        //UWP Community Toolkit name for CommunityToolkit.WinUI's FindDescendant(name)
        public static FrameworkElement FindDescendantByName(this DependencyObject start, string name)
        {
            return start.GetDescendantsOfType<FrameworkElement>().FirstOrDefault(element => element.Name == name);
        }

        public static IEnumerable<DependencyObject> GetAncestors(this DependencyObject start)
        {
            var parent = VisualTreeHelper.GetParent(start);
            while (parent != null)
            {
                yield return parent;
                parent = VisualTreeHelper.GetParent(parent);
            }
        }

        public static T GetFirstAncestorOfType<T>(this DependencyObject start) where T : DependencyObject
        {
            return start.GetAncestors().OfType<T>().FirstOrDefault();
        }
    }
}
