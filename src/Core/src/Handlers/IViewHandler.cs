using System;
using Microsoft.Maui;

namespace Microsoft.Maui
{
	public interface IViewHandler
	{
		void SetMauiContext(IMauiContext mauiContext);
		void SetVirtualView(IView view);
		void UpdateValue(string property);
		void DisconnectHandler();
		object? View { get; }
		IView? VirtualView { get; }
		bool HasContainer { get; set; }
		Size GetDesiredSize(double widthConstraint, double heightConstraint);
		void SetFrame(Rectangle frame);
	}
}