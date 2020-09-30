using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	internal static class ButtonElementManager
	{
		static readonly UIControlState[] s_controlStates = { UIControlState.Normal, UIControlState.Highlighted, UIControlState.Disabled };

		public static void Init(IVisualNativeElementRenderer renderer)
		{
			renderer.ElementPropertyChanged += OnElementPropertyChanged;
			renderer.ControlChanged += OnControlChanged;
		}

		static void OnControlChanged(object sender, EventArgs e)
		{
			var renderer = (IVisualNativeElementRenderer)sender;
			var control = (UIButton)renderer.Control;

			foreach (UIControlState uiControlState in s_controlStates)
			{
				control.SetTitleColor(UIButton.Appearance.TitleColor(uiControlState), uiControlState); // if new values are null, old values are preserved.
				control.SetTitleShadowColor(UIButton.Appearance.TitleShadowColor(uiControlState), uiControlState);
				control.SetBackgroundImage(UIButton.Appearance.BackgroundImageForState(uiControlState), uiControlState);
			}


			control.TouchUpInside -= TouchUpInside;
			control.TouchDown -= TouchDown;
			control.TouchUpInside += TouchUpInside;
			control.TouchDown += TouchDown;
		}

		static void TouchUpInside(object sender, EventArgs eventArgs)
		{
			var button = sender as UIButton;
			var renderer = button.Superview as IVisualNativeElementRenderer;
			OnButtonTouchUpInside(renderer.Element as IButtonController);
		}

		static void TouchDown(object sender, EventArgs eventArgs)
		{
			var button = sender as UIButton;
			var renderer = button.Superview as IVisualNativeElementRenderer;
			OnButtonTouchDown(renderer.Element as IButtonController);
		}

		public static void Dispose(IVisualNativeElementRenderer renderer)
		{
			var control = (UIButton)renderer.Control;
			renderer.ElementPropertyChanged -= OnElementPropertyChanged;
			control.TouchUpInside -= TouchUpInside;
			control.TouchDown -= TouchDown;
		}

		static void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
		}


		static void SetControlPropertiesFromProxy(UIButton control)
		{
			foreach (UIControlState uiControlState in s_controlStates)
			{
				control.SetTitleColor(UIButton.Appearance.TitleColor(uiControlState), uiControlState); // if new values are null, old values are preserved.
				control.SetTitleShadowColor(UIButton.Appearance.TitleShadowColor(uiControlState), uiControlState);
				control.SetBackgroundImage(UIButton.Appearance.BackgroundImageForState(uiControlState), uiControlState);
			}
		}

		internal static void OnButtonTouchDown(IButtonController element)
		{
			element?.SendPressed();
		}

		internal static void OnButtonTouchUpInside(IButtonController element)
		{
			element?.SendReleased();
			element?.SendClicked();
		}

		internal static void OnButtonTouchUpOutside(IButtonController element)
		{
			element?.SendReleased();
		}
	}
}