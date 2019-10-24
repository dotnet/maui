using System;
using Windows.UI.Xaml;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public interface IVisualElementRenderer : IRegisterable, IDisposable
	{
		FrameworkElement ContainerElement { get; }

		VisualElement Element { get; }

		event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint);

		void SetElement(VisualElement element);

		UIElement GetNativeElement();
	}
}