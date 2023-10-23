using System;
using Container = Microsoft.Maui.Controls.Compatibility.Platform.GTK.GtkFormsContainer;

namespace Microsoft.Maui.Controls.Compatibility.Platform.GTK
{
	public interface IVisualElementRenderer : IDisposable, IRegisterable
	{
		event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		VisualElement Element { get; }

		Container Container { get; }

		bool Disposed { get; }

		void SetElement(VisualElement element);

		SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint);

		void SetElementSize(Size size);
	}
}
