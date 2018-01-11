using System;
using System.ComponentModel;
using ElmSharp;
using Xamarin.Forms.PlatformConfiguration.TizenSpecific;
using Specific = Xamarin.Forms.PlatformConfiguration.TizenSpecific.VisualElement;

namespace Xamarin.Forms.Platform.Tizen
{
	public class SwitchRenderer : ViewRenderer<Switch, Check>
	{
		public SwitchRenderer()
		{
			RegisterPropertyHandler(Switch.IsToggledProperty, HandleToggled);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Switch> e)
		{
			if (Control == null)
			{
				SetNativeControl(new Check(Forms.NativeParent)
				{
					PropagateEvents = false,
					Style = SwitchStyle.Toggle
				});
				Control.StateChanged += OnStateChanged;
			}
			base.OnElementChanged(e);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Control != null)
				{
					Control.StateChanged -= OnStateChanged;
				}
			}
			base.Dispose(disposing);
		}

		protected override void UpdateThemeStyle()
		{
			var style = Specific.GetStyle(Element);
			if (string.IsNullOrEmpty(style))
			{
				return;
			}
			switch (style)
			{
				case SwitchStyle.Toggle:
				case SwitchStyle.Favorite:
				case SwitchStyle.CheckBox:
					Control.Style = style;
					break;
				default:
					Control.Style = SwitchStyle.Toggle;
					break;
			}
			((IVisualElementController)Element).NativeSizeChanged();
		}

		void OnStateChanged(object sender, EventArgs e)
		{
			Element.SetValue(Switch.IsToggledProperty, Control.IsChecked);
		}

		void HandleToggled()
		{
			Control.IsChecked = Element.IsToggled;
		}
	}
}
