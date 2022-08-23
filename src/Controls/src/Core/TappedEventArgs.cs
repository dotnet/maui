#nullable enable

using System;
using Microsoft.Maui.Graphics;
#if WINDOWS
using PlatformArgs = Microsoft.UI.Xaml.RoutedEventArgs;
#else
using PlatformArgs = System.Object;
#endif

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/TappedEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.TappedEventArgs']/Docs" />
	public class TappedEventArgs : EventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/TappedEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public TappedEventArgs(object? parameter)
		{
			Parameter = parameter;
		}

		internal TappedEventArgs(object? parameter, object platformArgs) : this(parameter)
		{
			if (platformArgs is PlatformArgs args)
				PlatformArgs = args;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/TappedEventArgs.xml" path="//Member[@MemberName='Parameter']/Docs" />
		public object? Parameter { get; private set; }

		public ButtonsMask Buttons { get; private set; }

		internal PlatformArgs? PlatformArgs { get; set; }

		public Point? GetPosition(Element? relativeTo)
		{
#if WINDOWS
			if (PlatformArgs == null)
				return null;

			if (relativeTo == null)
			{
				var position = PlatformArgs.GetPosition(null);
				return new Point(position.X, position.Y);
			}

			if (relativeTo.ToPlatform() is UI.Xaml.UIElement uiElement)
			{
				var position = PlatformArgs.GetPosition(uiElement);
				return new Point(position.X, position.Y);
			}
#endif

			return null;
		}
	}
}