using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;

#if WINDOWS
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using WinPoint = Windows.Foundation.Point;
using WinRect = Windows.Foundation.Rect;
#endif

#if (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID)
using IPlatformViewHandler = Microsoft.Maui.IViewHandler;
#endif
#if IOS || MACCATALYST
using PlatformView = UIKit.UIView;
using ParentView = UIKit.UIView;
#elif ANDROID
using PlatformView = Android.Views.View;
using ParentView = Android.Views.IViewParent;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
using ParentView = Microsoft.UI.Xaml.DependencyObject;
#elif TIZEN
using PlatformView = Tizen.NUI.BaseComponents.View;
using ParentView = Tizen.NUI.BaseComponents.View;
#else
using PlatformView = System.Object;
using ParentView = System.Object;
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
				var uniqueElements = findChildren(uiElement).ToHashSet();

				var descendants = visualElement.GetVisualTreeDescendants();

				// Add in reverse order
				for (int i = descendants.Count - 1; i >= 0; i--)
				{
					var descendant = descendants[i];

					if (descendant is not IView view || view.Handler is null)
					{
						continue;
					}
					
					if (uniqueElements.Contains(view.ToPlatform()))
					{
						visualElements.Add(descendant);
					}
				}
			}

			return visualElements;
		}
#endif

		/// <summary>
		/// Locates the <see cref="IVisualTreeElement"/> that's a best fit for the given platform view.		 
		/// </summary>
		/// <remarks>
		/// If an exact <see cref="IVisualTreeElement"/> counterpart isn't found, then the
		/// first <see cref="IVisualTreeElement"/> within the ancestors of the given platform view will 
		/// be returned. 
		/// </remarks>
		/// <param name="platformView">The platform view.</param>
		/// <returns>
		/// A visual tree element if found, <see langword="null"/> otherwise.
		/// </returns>
		internal static IVisualTreeElement? GetVisualTreeElement(
			this PlatformView platformView) =>
				platformView.GetVisualTreeElement(true);

		/// <summary>
		/// Locates the <see cref="IVisualTreeElement"/> that's a best fit for the given platform view.		 
		/// </summary>
		/// <remarks>
		/// If an exact <see cref="IVisualTreeElement"/> counterpart isn't found, then the
		/// first <see cref="IVisualTreeElement"/> within the ancestors of the given platform view will 
		/// be returned. 
		/// </remarks>
		/// <param name="platformView">The platform view.</param>
		/// <param name="searchAncestors">
		/// <see langword="true"/> to search within the ancestors of the given platform view;  
		/// otherwise, <see langword="false"/>.</param>
		/// <returns>
		/// A visual tree element if found, <see langword="null"/> otherwise.
		/// </returns>
		internal static IVisualTreeElement? GetVisualTreeElement(
			this PlatformView platformView, bool searchAncestors)
		{
			var platformParentPath = new List<PlatformView>();
			IVisualTreeElement? foundParent = null;

			// Locate the first Platform View we can find that can return us its Maui Element
			var nearestParentContainer =
				platformView
					.FindParent(x =>
					{
						if (x is PlatformView pv)
							platformParentPath.Add(pv);

						if (x is IVisualTreeElementProvidable backing)
						{
							foundParent = backing.GetElement();
							return foundParent is not null;
						}

						return false;
					});

			platformParentPath.Reverse();

			if (foundParent?.IsThisMyPlatformView(platformView) == true)
				return foundParent;

			if (nearestParentContainer is null || foundParent is null)
				return null;

			// Now that we have an xplat starting point
			// Let's search back down the xplat tree to figure out what IElement to return
			// This searches down the xplat tree to figure out what path going down the xplat tree
			// matches up against the path we took to go up the platform tree
			var returnValue = FindNextChild(foundParent, platformView, platformParentPath);

			// If we aren't searching ancestors, then we only want to return
			// IVTE if it matches the found platformView
			if (!searchAncestors &&
				returnValue != null &&
				!returnValue.IsThisMyPlatformView(platformView))
			{
				return null;
			}

			return returnValue;

			static IVisualTreeElement? FindNextChild(
				IVisualTreeElement parent,
				PlatformView platformView,
				List<PlatformView> platformParentPath)
			{
				var children = parent.GetVisualChildren();
				IVisualTreeElement? childMatch = null;
				foreach (var child in children)
				{
					if (child is not IVisualTreeElement childVTE)
					{
						return parent;
					}

					if (childVTE.IsThisMyPlatformView(platformView))
					{
						return childVTE;
					}

					// We only want to check children with platform components that have been realized
					if (childVTE is IElement element &&
						element.Handler is IPlatformViewHandler pvh &&
						pvh.PlatformView is not null)
					{
						var indexOfPlatformView = platformParentPath.IndexOf(pvh.PlatformView);

						if (indexOfPlatformView < 0)
							continue;

						childMatch = child;
						platformParentPath.RemoveRange(0, indexOfPlatformView + 1);
						break;
					}
				}

				// If I've ran out of children then we just return the parent 
				// as the furthest down element we've been able to match to
				if (childMatch is null)
					return parent;

				return FindNextChild(childMatch, platformView, platformParentPath);
			}
		}

		internal static bool IsThisMyPlatformView(this IVisualTreeElement? visualTreeElement, PlatformView platformView)
		{
			if (visualTreeElement is IElement element)
				return element.IsThisMyPlatformView(platformView);

			return false;
		}

		static List<IVisualTreeElement> GetVisualTreeElementsInternal(IVisualTreeElement visualElement, Predicate<Rect> intersectElementBounds)
		{
			var elements = new List<IVisualTreeElement>();

			Impl(visualElement, intersectElementBounds, elements);

			elements.Reverse();
			return elements;

			static void Impl(IVisualTreeElement visualElement, Predicate<Rect> intersectElementBounds, List<IVisualTreeElement> elements)
			{
				if (visualElement is IView view && view.Handler is not null)
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