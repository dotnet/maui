using System;
using AppKit;

namespace System.Maui.Platform.MacOS
{
	public class CheckBoxRenderer : ViewRenderer<CheckBox, NSButton>
	{
		bool _disposed;

		IElementController ElementController => Element;

		protected override void OnElementChanged(ElementChangedEventArgs<CheckBox> e)
		{
			if (e.OldElement != null)
				e.OldElement.CheckedChanged -= OnElementChecked;

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					SetNativeControl(new NSButton { AllowsMixedState = false, Title = string.Empty });

					Control.SetButtonType(NSButtonType.Switch);
					Control.Activated += OnControlActivated;
				}

				UpdateState();
				e.NewElement.CheckedChanged += OnElementChecked;
			}

			base.OnElementChanged(e);
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;
			if (disposing && Control != null)
			{
				Control.Activated -= OnControlActivated;
			}

			base.Dispose(disposing);
		}

		void OnControlActivated(object sender, EventArgs e)
		{
			ElementController?.SetValueFromRenderer(CheckBox.IsCheckedProperty, Control.State == NSCellStateValue.On);
		}

		void OnElementChecked(object sender, EventArgs e)
		{
			UpdateState();
		}

		void UpdateState()
		{
			Control.State = Element.IsChecked ? NSCellStateValue.On : NSCellStateValue.Off;
		}
	}
}