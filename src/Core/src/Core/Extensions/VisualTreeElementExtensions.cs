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
	public static class VisualTreeElementExtensions
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
		/// Gets the entire hierarchy of descendants as a list of children for a given Visual Tree Element.
		/// </summary>
		/// <param name="visualElement"><see cref="IVisualTreeElement"/> to scan.</param>
		/// <returns>List of Children Elements.</returns>
		public static IList<IVisualTreeElement> GetVisualTreeDescendants(this IVisualTreeElement visualElement) =>
			visualElement.GetVisualTreeDescendantsInternal();

		static IList<IVisualTreeElement> GetVisualTreeDescendantsInternal(this IVisualTreeElement visualElement, IList<IVisualTreeElement>? elements = null)
		{
			if (elements == null)
				elements = new List<IVisualTreeElement>();

			elements.Add(visualElement);

			foreach (var children in visualElement.GetVisualChildren())
				children.GetVisualTreeDescendantsInternal(elements);

			return elements;
		}

		/// <summary>
		/// Gets list of a Visual Tree Elements children based off of a given x, y point.
		/// </summary>
		/// <param name="visualElement"><see cref="IVisualTreeElement"/> to scan.</param>
		/// <param name="x1">The X point.</param>
		/// <param name="y1">The Y point.</param>
		/// <param name="x2">The X point.</param>
		/// <param name="y2">The Y point.</param>
		/// <param name="usePlatformViewBounds">If true, use platform view bounds for given elements. Else, use the Elements Frame.</param>
		/// <returns>List of Children Elements.</returns>
		public static IList<IVisualTreeElement> GetVisualTreeElements(this IVisualTreeElement visualElement, double x1, double y1, double x2, double y2, bool usePlatformViewBounds = true) =>
			GetVisualTreeElements(visualElement, new Rect(x1, y1, x2 - x1, y2 - y1), usePlatformViewBounds);

		/// <summary>
		/// Gets list of a Visual Tree Elements children based off of a rectangle.
		/// </summary>
		/// <param name="visualElement"><see cref="IVisualTreeElement"/> to scan.</param>
		/// <param name="rectangle">The rectangle.</param>
		/// <param name="usePlatformViewBounds">If true, use platform view bounds for given elements. Else, use the Elements Frame.</param>
		/// <returns>List of Children Elements.</returns>
		public static IList<IVisualTreeElement> GetVisualTreeElements(this IVisualTreeElement visualElement, Rect rectangle, bool usePlatformViewBounds = true) =>
			GetVisualTreeElementsInternal(
				visualElement,
				new List<Point>
				{
					new Point(rectangle.X, rectangle.Y),
					new Point(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height),
					new Point(rectangle.X + rectangle.Width, rectangle.Y),
					new Point(rectangle.X, rectangle.Y + rectangle.Height)
				},
				usePlatformViewBounds);

		/// <summary>
		/// Gets list of a Visual Tree Elements children based off of a given x, y point.
		/// </summary>
		/// <param name="visualElement"><see cref="IVisualTreeElement"/> to scan.</param>
		/// <param name="x">The X point.</param>
		/// <param name="y">The Y point.</param>
		/// <param name="usePlatformViewBounds">If true, use platform view bounds for given elements. Else, use the Elements Frame.</param>
		/// <returns>List of Children Elements.</returns>
		public static IList<IVisualTreeElement> GetVisualTreeElements(this IVisualTreeElement visualElement, double x, double y, bool usePlatformViewBounds = true) =>
			GetVisualTreeElements(visualElement, new Point(x, y), usePlatformViewBounds);

		/// <summary>
		/// Gets list of a Visual Tree Element's children based off of a given Point.
		/// </summary>
		/// <param name="visualElement"><see cref="IVisualTreeElement"/> to scan.</param>
		/// <param name="point"><see cref="Point"/>.</param>
		/// <param name="usePlatformViewBounds">If true, use platform view bounds for given elements. Else, use the Element's Frame.</param>
		/// <returns>List of Children Elements.</returns>
		public static IList<IVisualTreeElement> GetVisualTreeElements(this IVisualTreeElement visualElement, Point point, bool usePlatformViewBounds = true) =>
#if WINDOWS
			GetVisualTreeElementsWindowsInternal(visualElement, new List<Point>() { point }, usePlatformViewBounds);
#else
			GetVisualTreeElementsInternal(visualElement, new List<Point>() { point }, usePlatformViewBounds);
#endif

#if WINDOWS
		static IList<IVisualTreeElement> GetVisualTreeElementsWindowsInternal(IVisualTreeElement visualElement, IList<Point> points, bool usePlatformViewBounds = true)
		{
			if (!usePlatformViewBounds)
			{
				return GetVisualTreeElementsInternal(visualElement, points, false);
			}

			UIElement? uiElement = null;
			var visualElements = new List<IVisualTreeElement>();
			if (visualElement is IWindow window)
			{
				uiElement = window.Content.ToPlatform();
			}
			else if (visualElement is IView view)
			{
				uiElement = view.ToPlatform();
			}

			if (uiElement != null)
			{
				var uiElements = new List<UIElement>();
				foreach (var point in points)
				{
					uiElements.AddRange(VisualTreeHelper.FindElementsInHostCoordinates(new WinPoint(point.X, point.Y), uiElement));
				}

				var uniqueElements = uiElements.Distinct();
				var viewTree = visualElement.GetVisualTreeDescendants().Where(n => n is IView).Select(n => new Tuple<IView, object?>((IView)n, ((IView)n).ToPlatform()));
				var testList = viewTree.Where(n => uniqueElements.Contains(n.Item2)).Select(n => n.Item1);
				if (testList != null && testList.Any())
					visualElements.AddRange(testList.Select(n => (IVisualTreeElement)n));
			}

			visualElements.Reverse();
			return visualElements;
		}
#endif

		static IList<IVisualTreeElement> GetVisualTreeElementsInternal(IVisualTreeElement visualElement, IList<Point> points, bool usePlatformViewBounds = true, IList<IVisualTreeElement>? elements = null)
		{
			if (elements == null)
				elements = new List<IVisualTreeElement>();

			if (visualElement is IView view)
			{
				if (usePlatformViewBounds)
				{
					var bounds = view.GetPlatformViewBounds();
					if (points.All(n => bounds.Contains(n)))
						elements.Add(visualElement);
				}
				else if (points.All(n => view.Frame.Contains(n)))
				{
					elements.Add(visualElement);
				}
			}

			var children = visualElement.GetVisualChildren();

			foreach (var child in children)
			{
				GetVisualTreeElementsInternal(child, points, usePlatformViewBounds, elements);
			}

			return elements.Reverse().ToList();
		}
	}
}