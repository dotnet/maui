using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using WPoint = Windows.Foundation.Point;
namespace Microsoft.Maui.Platform
{
	internal static class RoutedEventArgsExtensions
	{
		public static void SetHandled(this RoutedEventArgs e, bool value)
		{
			if (e is RightTappedRoutedEventArgs rt)
				rt.Handled = value;
			else if (e is TappedRoutedEventArgs t)
				t.Handled = value;
			else if (e is DoubleTappedRoutedEventArgs dt)
				dt.Handled = value;
		}

		public static WPoint? GetPositionRelativeToElement(this RoutedEventArgs e, IElement? relativeTo)
		{
			if (relativeTo == null)
				return GetPositionRelativeToPlatformElement(e, null);

			if (relativeTo?.Handler?.PlatformView is UIElement element)
				return GetPositionRelativeToPlatformElement(e, element);

			return null;
		}

		public static WPoint? GetPositionRelativeToPlatformElement(this RoutedEventArgs e, UIElement? relativeTo)
		{
			if (e is RightTappedRoutedEventArgs rt)
				return rt.GetPosition(relativeTo);
			else if (e is TappedRoutedEventArgs t)
				return t.GetPosition(relativeTo);
			else if (e is DoubleTappedRoutedEventArgs dt)
				return dt.GetPosition(relativeTo);
			else if (e is PointerRoutedEventArgs p)
			{
				var point = p.GetCurrentPoint(relativeTo);
				return new WPoint(point.Position.X, point.Position.Y);
			}

			return null;
		}
	}
}
