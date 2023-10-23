using System;
using Microsoft.Maui.Controls.Platform;
using Gtk;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Gtk
{

	public class DefaultRenderer : IVisualElementRenderer
	{

		public void Dispose()
		{
			throw new NotImplementedException();
		}

		public VisualElement Element { get; set; }

		public Widget NativeView { get; set; }

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		public void SetElement(VisualElement element)
		{
			ElementChanged?.Invoke(this, new VisualElementChangedEventArgs(Element, element));

			Element = element;
		}

		public SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			throw new NotImplementedException();
		}

		public void UpdateLayout()
		{
			throw new NotImplementedException();
		}

	}

#pragma warning disable CS0612 // Type or member is obsolete
	public class NativeViewWrapperRenderer : DefaultRenderer { }
#pragma warning restore CS0612 // Type or member is obsolete
}