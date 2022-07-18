using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Graphics;

#if WINDOWS
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using WinPoint = Windows.Foundation.Point;
using WinRect = Windows.Foundation.Rect;
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
		public static IReadOnlyList<IVisualTreeElement> GetVisualTreeDescendants(this IVisualTreeElement visualElement) =>
			visualElement.GetVisualTreeDescendantsInternal();

		static List<IVisualTreeElement> GetVisualTreeDescendantsInternal(this IVisualTreeElement visualElement, List<IVisualTreeElement>? elements = null)
		{
			if (elements == null)
				elements = new List<IVisualTreeElement>();

			elements.Add(visualElement);

			foreach (var children in visualElement.GetVisualChildren())
				children.GetVisualTreeDescendantsInternal(elements);

			return elements;
		}

		/// <summary>
		/// Gets list of a Visual Tree Elements children based off of a rectangle defined by its coordinates which are specified in platform units, not pixels.
		/// </summary>
		/// <param name="visualElement"><see cref="IVisualTreeElement"/> to scan.</param>
		/// <param name="x1">The X coordinate of the top left point.</param>
		/// <param name="y1">The Y coordinate of the top left point.</param>
		/// <param name="x2">The X coordinate of the bottom right point.</param>
		/// <param name="y2">The Y coordinate of the bottom right point.</param>
		/// <returns>List of Children Elements.</returns>
		public static IReadOnlyList<IVisualTreeElement> GetVisualTreeElements(this IVisualTreeElement visualElement, double x1, double y1, double x2, double y2) =>
			GetVisualTreeElements(visualElement, new Rect(x1, y1, x2 - x1, y2 - y1));

		/// <summary>
		/// Gets list of a Visual Tree Elements children based off of a rectangle.
		/// </summary>
		/// <param name="visualElement"><see cref="IVisualTreeElement"/> to scan.</param>
		/// <param name="rectangle">The rectangle.</param>
		/// <returns>List of Children Elements.</returns>
		public static IReadOnlyList<IVisualTreeElement> GetVisualTreeElements(this IVisualTreeElement visualElement, Rect rectangle)
		{
#if WINDOWS
			return GetVisualTreeElementsWindowsInternal(visualElement,
				uiElement => VisualTreeHelper.FindElementsInHostCoordinates(new WinRect(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height), uiElement));
#else
			return GetVisualTreeElementsInternal(
				visualElement,
				bounds => bounds.IntersectsWith(rectangle));
#endif
		}

		/// <summary>
		/// Gets list of a Visual Tree Elements children based off of a given x, y point.
		/// </summary>
		/// <param name="visualElement"><see cref="IVisualTreeElement"/> to scan.</param>
		/// <param name="x">The X point.</param>
		/// <param name="y">The Y point.</param>
		/// <returns>List of Children Elements.</returns>
		public static IReadOnlyList<IVisualTreeElement> GetVisualTreeElements(this IVisualTreeElement visualElement, double x, double y) =>
			GetVisualTreeElements(visualElement, new Point(x, y));

		/// <summary>
		/// Gets list of a Visual Tree Element's children based off of a given Point.
		/// </summary>
		/// <param name="visualElement"><see cref="IVisualTreeElement"/> to scan.</param>
		/// <param name="point"><see cref="Point"/>.</param>
		/// <returns>List of Children Elements.</returns>
		public static IReadOnlyList<IVisualTreeElement> GetVisualTreeElements(this IVisualTreeElement visualElement, Point point)
		{
#if WINDOWS
			return GetVisualTreeElementsWindowsInternal(visualElement,
				uiElement => VisualTreeHelper.FindElementsInHostCoordinates(new WinPoint(point.X, point.Y), uiElement));
#else
			return GetVisualTreeElementsInternal(visualElement, bounds => bounds.Contains(point));
#endif
		}

#if WINDOWS
		static List<IVisualTreeElement> GetVisualTreeElementsWindowsInternal(IVisualTreeElement visualElement, Func<UIElement, IEnumerable<UIElement>> findChildren)
		{
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
				var uniqueElements = findChildren(uiElement).Distinct();
				var viewTree = visualElement.GetVisualTreeDescendants().Where(n => n is IView).Select(n => new Tuple<IView, object?>((IView)n, ((IView)n).ToPlatform()));
				var testList = viewTree.Where(n => uniqueElements.Contains(n.Item2)).Select(n => n.Item1);
				if (testList != null && testList.Any())
					visualElements.AddRange(testList.Select(n => (IVisualTreeElement)n));
			}

			visualElements.Reverse();
			return visualElements;
		}
#endif

		static List<IVisualTreeElement> GetVisualTreeElementsInternal(IVisualTreeElement visualElement, Predicate<Rect> intersectElementBounds)
		{
			var elements = new List<IVisualTreeElement>();

			Impl(visualElement, intersectElementBounds, elements);

			elements.Reverse();
			return elements;

			static void Impl(IVisualTreeElement visualElement, Predicate<Rect> intersectElementBounds, List<IVisualTreeElement> elements)
			{
				if (visualElement is IView view)
				{
					Rect bounds = view.GetBoundingBox();
					if (intersectElementBounds(bounds))
						elements.Add(visualElement);
				}
				var children = visualElement.GetVisualChildren();

				foreach (var child in children)
				{
					Impl(child, intersectElementBounds, elements);
				}
			}
		}
	}
}