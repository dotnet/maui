using System;
using AppKit;

namespace Xamarin.Forms.Platform.MacOS
{
	public class SwitchRenderer : ViewRenderer<Switch, NSButton>
	{
		bool _disposed;

		IElementController ElementController => Element;

		protected override void OnElementChanged(ElementChangedEventArgs<Switch> e)
		{
			if (e.OldElement != null)
				e.OldElement.Toggled -= OnElementToggled;

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					SetNativeControl(new NSButton { AllowsMixedState = false, Title = string.Empty });

					Control.SetButtonType(NSButtonType.Switch);
					Control.Activated += OnControlActivated;
				}

				UpdateState();
				e.NewElement.Toggled += OnElementToggled;
			}

			base.OnElementChanged(e);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_disposed = true;
				if (Control != null)
					Control.Activated -= OnControlActivated;
			}

			base.Dispose(disposing);
		}

		void OnControlActivated(object sender, EventArgs e)
		{
			ElementController?.SetValueFromRenderer(Switch.IsToggledProperty, Control.State == NSCellStateValue.On);
		}

		void OnElementToggled(object sender, EventArgs e)
		{
			UpdateState();
		}

		void UpdateState()
		{
			Control.State = Element.IsToggled ? NSCellStateValue.On : NSCellStateValue.Off;
		}
	}
}