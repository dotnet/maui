using System;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.WinUI;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
using Microsoft.Maui.Controls.Platform;
using Microsoft.UI.Xaml.Input;
using Windows.UI.Input;

[assembly: ExportRenderer(typeof(Bugzilla60122._60122Image), typeof(_60122ImageRenderer))]

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.WinUI
{
	public class _60122ImageRenderer : ImageRenderer
	{
		Bugzilla60122._60122Image _customControl;
		readonly global::Windows.UI.Input.GestureRecognizer _gestureRecognizer = new global::Windows.UI.Input.GestureRecognizer();

		protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				_customControl = e.NewElement as Bugzilla60122._60122Image;
				_gestureRecognizer.GestureSettings = GestureSettings.HoldWithMouse;
				Holding += OnHolding;
			}
			else
			{
				Holding -= OnHolding;
			}
		}

		void OnHolding(object sender, HoldingRoutedEventArgs holdingRoutedEventArgs)
		{
			if (holdingRoutedEventArgs.HoldingState == HoldingState.Completed)
			{
				_customControl?.HandleLongPress(_customControl, new EventArgs());
			}
		}
	}
}