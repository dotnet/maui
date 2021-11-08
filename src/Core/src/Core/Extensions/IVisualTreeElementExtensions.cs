using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Graphics;

#if WINDOWS
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using WinPoint = Windows.Foundation.Point;
#endif

namespace Microsoft.Maui
{
	public static class IVisualTreeElementExtensions
	{
		/// <summary>
		/// Gets the Window containing the Visual Tree Element, if the element is contained within one.
		/// </summary>
		/// <param name="element"><see cref="IVisualTreeElement"/>.</param>
		/// <returns><see cref="IWindow"/> if element is contained within a Window, else returns null.</returns>
		public static IWindow? GetVisualElementWindow(this IVisualTreeElement element)
		{
			if (element is IWindow window)
				return window;

			var parent = element.GetVisualParent();
			if (parent != null)
				return parent.GetVisualElementWindow();

			return null;
		}

		/// <summary>
		/// Gets the entire list of children for a given Visual Tree Element.
		/// </summary>
		/// <param name="visualElement"><see cref="IVisualTreeElement"/> to scan.</param>
		/// <returns>List of Children Elements.</returns>
		public static IList<IVisualTreeElement> GetEntireVisualTreeElementChildren(this IVisualTreeElement visualElement) => visualElement.GetEntireVisualTreeElementChildrenInternal();

		private static IList<IVisualTreeElement> GetEntireVisualTreeElementChildrenInternal(this IVisualTreeElement visualElement, IList<IVisualTreeElement>? elements = null)
		{
			if (elements == null)
				elements = new List<IVisualTreeElement>();

			elements.Add(visualElement);

			foreach (var children in visualElement.GetVisualChildren())
				children.GetEntireVisualTreeElementChildrenInternal(elements);

			return elements;
		}

		/// <summary>
		/// Gets list of a Visual Tree Elements children based off of a given x, y point.
		/// </summary>
		/// <param name="visualElement"><see cref="IVisualTreeElement"/> to scan.</param>
		/// <param name="x">The X point.</param>
		/// <param name="y">The Y point.</param>
		/// <param name="useNativeViewBounds">If else, use native view bounds for given elements. Else, use the Elements Frame.</param>
		/// <returns>List of Children Elements.</returns>
		public static IList<IVisualTreeElement> GetVisualTreeElements(this IVisualTreeElement visualElement, double x, double y, bool useNativeViewBounds = true) => GetVisualTreeElements(visualElement, new Point(x, y), useNativeViewBounds);
#if WINDOWS
		/// <summary>
		/// Gets list of a Visual Tree Elements children based off of a given Point.
		/// </summary>
		/// <param name="visualElement"><see cref="IVisualTreeElement"/> to scan.</param>
		/// <param name="point"><see cref="Point"/>.</param>
		/// <param name="useNativeViewBounds">If else, use native view bounds for given elements. Else, use the Elements Frame.</param>
		/// <returns>List of Children Elements.</returns>
		public static IList<IVisualTreeElement> GetVisualTreeElements(this IVisualTreeElement visualElement, Point point, bool useNativeViewBounds = true) => GetVisualTreeElementsWindowsInternal(visualElement, new List<Point>() { point }, useNativeViewBounds);
#else
		/// <summary>
		/// Gets list of a Visual Tree Elements children based off of a given Point.
		/// </summary>
		/// <param name="visualElement"><see cref="IVisualTreeElement"/> to scan.</param>
		/// <param name="point"><see cref="Point"/>.</param>
		/// <param name="useNativeViewBounds">If else, use native view bounds for given elements. Else, use the Elements Frame.</param>
		/// <returns>List of Children Elements.</returns>
		public static IList<IVisualTreeElement> GetVisualTreeElements(this IVisualTreeElement visualElement, Point point, bool useNativeViewBounds = true) => GetVisualTreeElementsInternal(visualElement, new List<Point>() { point }, useNativeViewBounds);
#endif
#if WINDOWS
		private static IList<IVisualTreeElement> GetVisualTreeElementsWindowsInternal(IVisualTreeElement visualElement, IList<Point> points, bool useNativeViewBounds = true)
		{
			if (!useNativeViewBounds)
			{
				return GetVisualTreeElementsInternal(visualElement, points, false);
			}

			UIElement? uiElement = null;
			var visualElements = new List<IVisualTreeElement>();
			if (visualElement is IWindow window)
			{
				// DIRTY HACK!!!
				// GetNative() returns a FrameworkElement
				// Window is a UI Window, not a FrameworkElement so it's always null.
				// For now, get the handler and cast it right.

				var testElement = window as IElement;
				if (testElement != null && testElement.Handler != null)
				{
					var testWindow = testElement.Handler.NativeView as Window;
					if (testWindow != null)
						uiElement = testWindow.Content;
				}
			}
			else if (visualElement is IView view)
			{
				uiElement = view.GetNative(true);
			}

			if (uiElement != null)
			{
				var uiElements = new List<UIElement>();
				foreach (var point in points)
				{
					uiElements.AddRange(VisualTreeHelper.FindElementsInHostCoordinates(new WinPoint(point.X, point.Y), uiElement));
				}

				var uniqueElements = uiElements.Distinct();
				var viewTree = visualElement.GetEntireVisualTreeElementChildren().Where(n => n is IView).Select(n => new Tuple<IView, object?>((IView)n, ((IView)n).GetNative(true)));
				var testList = viewTree.Where(n => uniqueElements.Contains(n.Item2)).Select(n => n.Item1);
				if (testList != null && testList.Any())
					visualElements.AddRange(testList.Select(n => (IVisualTreeElement)n));
			}

			visualElements.Reverse();
			return visualElements;
		}
#endif

		private static IList<IVisualTreeElement> GetVisualTreeElementsInternal(IVisualTreeElement visualElement, IList<Point> points, bool useNativeViewBounds = true, IList<IVisualTreeElement>? elements = null)
		{
			if (elements == null)
				elements = new List<IVisualTreeElement>();

			if (visualElement is IView view)
			{
				if (useNativeViewBounds && points.All(n => view.GetNativeViewBounds().Contains(n)))
				{
					elements.Add(visualElement);
				}
				else if (points.All(n => view.Frame.Contains(n)))
				{
					elements.Add(visualElement);
				}
			}

			var children = visualElement.GetVisualChildren();

			foreach(var child in children)
			{
				GetVisualTreeElementsInternal(child, points, useNativeViewBounds, elements);
			}

			
			return elements.Reverse().ToList();
		}
	}
}
