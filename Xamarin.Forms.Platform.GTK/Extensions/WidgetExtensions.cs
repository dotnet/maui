using Gtk;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Xamarin.Forms.Platform.GTK.Extensions
{
    public static class WidgetExtensions
    {
        public static SizeRequest GetDesiredSize(
            this Widget self,
            double widthConstraint,
            double heightConstraint)
        {
            Gdk.Size desiredSize;

            if (self is IDesiredSizeProvider)
            {
                desiredSize = ((IDesiredSizeProvider)self).GetDesiredSize();
            }
            else
            {
                var req = self.SizeRequest();
                desiredSize = new Gdk.Size(
                    req.Width > 0 ? req.Width : 0, 
                    req.Height > 0 ? req.Height : 0);
            }

            var widthFits = widthConstraint >= desiredSize.Width;
            var heightFits = heightConstraint >= desiredSize.Height;

            if (widthFits && heightFits) // Enough space with given constraints
            {
                return new SizeRequest(new Size(desiredSize.Width, desiredSize.Height));
            }

            if (!widthFits)
            {
                self.SetSize((int)widthConstraint, -1);

                var req = self.SizeRequest();
                desiredSize = new Gdk.Size(
                    req.Width > 0 ? req.Width : 0, 
                    req.Height > 0 ? req.Height : 0);
                heightFits = heightConstraint >= desiredSize.Height;
            }

            var size = new Size(desiredSize.Width, heightFits ? desiredSize.Height : (int)heightConstraint);

            return new SizeRequest(size);
        }

        public static void MoveTo(this Widget self, double x, double y)
        {
            if (self.Parent is Fixed)
            {
                var container = self.Parent as Fixed;
                var calcX = (int)Math.Round(x);
                var calcY = (int)Math.Round(y);

                int containerChildX, containerChildY;
                GetContainerChildXY(container, self, out containerChildX, out containerChildY);

                if (containerChildX != calcX || containerChildY != calcY)
                {
                    container.Move(self, calcX, calcY);
                }
            }
        }

        static void GetContainerChildXY (Fixed parent, Widget child, out int x, out int y)
        {
            using (GLib.Value val = parent.ChildGetProperty(child, "x"))
            {
                x = (int)val;
            }

            using (GLib.Value val = parent.ChildGetProperty(child, "y"))
            {
                y = (int)val;
            }
        }

        public static void SetSize(this Widget self, double width, double height)
        {
            int calcWidth = (int)Math.Round(width);
            int calcHeight = (int)Math.Round(height);

            // Avoid negative values
            if (calcWidth < -1)
            {
                calcWidth = -1;
            }

            if (calcHeight < -1)
            {
                calcHeight = -1;
            }

            if (calcWidth != self.WidthRequest || calcHeight != self.HeightRequest)
            {
                self.SetSizeRequest(calcWidth, calcHeight);
            }
        }

        public static void RemoveFromContainer(this Widget self, Widget child)
        {
            var container = self as Container;

            if (child != null && child.Parent != null)
            {
                if (container != null && container.HasChild(child))
                {
                    container.Remove(child);
                }
            }
        }

        public static bool HasChild(this Container self, Widget child)
        {
            return self.Children.Contains(child);
        }

        public static Size GetMaxChildDesiredSize(this Widget self, double widthConstraint, double heightConstraint)
        {
            var container = self as Container;
            var childReq = Size.Zero;

            if (container != null)
            {
                foreach (var child in container.Children)
                {
                    var currentChildReq = child.GetMaxChildDesiredSize(widthConstraint, heightConstraint);

                    if (currentChildReq.Height > childReq.Height)
                    {
                        childReq = currentChildReq;
                    }
                }
            }

            self.SetSize((int)widthConstraint - 1, -1);
            var desiredSize = self.GetDesiredSize(widthConstraint, heightConstraint);

            return childReq.Height > desiredSize.Request.Height
                ? childReq
                : desiredSize.Request;
        }

        public static IEnumerable<Widget> GetDescendants(this Widget self)
        {
            var descendants = new List<Widget>();
            var container = self as Container;

            if (container != null)
            {
                foreach (var child in container.Children)
                {
                    descendants.Add(child);
                    descendants.AddRange(child.GetDescendants());
                }
            }

            return descendants;
        }

        public static void PrintTree(this Widget widget)
        {
            const char indent = '-';
            int level = CalculateDepthLevel(widget);

            Console.WriteLine(
                string.Format(
                    "({0}) {1} Name: {2} ({3})",
                    level,
                    new String(indent, level * 2),
                    widget.Name,
                    widget.GetType()));

            Console.WriteLine(string.Format("{0} Size: {1}", new String('\t', level), widget.Allocation.Size));
            Console.WriteLine(string.Format("{0} Location: {1}", new String('\t', level), widget.Allocation.Location));

            if (widget is Container)
            {
                var container = widget as Container;

                foreach (Widget child in container.Children)
                {
                    PrintTree(child);
                }
            }
        }

        private static int CalculateDepthLevel(Widget widget)
        {
            int level = 0;
            Widget current = widget;

            while ((current = current.Parent) != null)
            {
                level++;
            }

            return level;
        }
    }
}