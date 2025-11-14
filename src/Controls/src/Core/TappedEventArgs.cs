using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/TappedEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.TappedEventArgs']/Docs/*" />
	public class TappedEventArgs : EventArgs
	{
		Func<IElement?, Point?>? _getPosition;

		/// <include file="../../docs/Microsoft.Maui.Controls/TappedEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public TappedEventArgs(object? parameter)
		{
			Parameter = parameter;
		}

		internal TappedEventArgs(object? parameter, Func<IElement?, Point?>? getPosition, ButtonsMask buttons) : this(parameter)
		{
			_getPosition = getPosition;
			Buttons = buttons;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/TappedEventArgs.xml" path="//Member[@MemberName='Parameter']/Docs/*" />
		public object? Parameter { get; private set; }

		public ButtonsMask Buttons { get; private set; }

		public virtual Point? GetPosition(Element? relativeTo) =>
			_getPosition?.Invoke(relativeTo);
	}
}