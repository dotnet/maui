#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/FocusEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.FocusEventArgs']/Docs/*" />
	public class FocusEventArgs : EventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/FocusEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public FocusEventArgs(VisualElement visualElement, bool isFocused)
		{
			if (visualElement == null)
				throw new ArgumentNullException(nameof(visualElement));

			VisualElement = visualElement;
			IsFocused = isFocused;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/FocusEventArgs.xml" path="//Member[@MemberName='IsFocused']/Docs/*" />
		public bool IsFocused { get; private set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/FocusEventArgs.xml" path="//Member[@MemberName='VisualElement']/Docs/*" />
		public VisualElement VisualElement { get; private set; }
	}
}