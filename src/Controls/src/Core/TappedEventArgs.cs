#nullable enable

using System;
using Microsoft.Maui.Graphics;
#if WINDOWS
using PlatformArgs = Microsoft.UI.Xaml.Input.TappedRoutedEventArgs;
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

		/// <include file="../../docs/Microsoft.Maui.Controls/TappedEventArgs.xml" path="//Member[@MemberName='Parameter']/Docs" />
		public object? Parameter { get; private set; }

		public ButtonsMask Buttons { get; private set; }

		PlatformArgs? _platformArgs;

		internal void SetPlatformArgs(PlatformArgs platformArgs) =>
			_platformArgs = platformArgs;

		public Point? GetPosition(Element? relativeTo)
		{
#if WINDOWS
			if (_platformArgs == null)
				return null;

			if (relativeTo == null)
			{
				var position = _platformArgs.GetPosition(null);
				return new Point(position.X, position.Y);
			}

			if (relativeTo.ToPlatform() is UI.Xaml.UIElement uiElement)
			{
				var position = _platformArgs.GetPosition(uiElement);
				return new Point(position.X, position.Y);
			}
#endif

			return null;
		}
	}
}