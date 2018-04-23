using System;
using ElmSharp;
using Xamarin.Forms.PlatformConfiguration.TizenSpecific;
using Specific = Xamarin.Forms.PlatformConfiguration.TizenSpecific.VisualElement;
using EColor = ElmSharp.Color;

namespace Xamarin.Forms.Platform.Tizen
{
	public class SwitchRenderer : ViewRenderer<Switch, Check>
	{
		readonly string _onColorPart;
		readonly bool _isTV;
		string _onColorEdjePart;

		public SwitchRenderer()
		{
			_isTV = Device.Idiom == TargetIdiom.TV;
			_onColorPart = _isTV ? "slider_on" : Device.Idiom == TargetIdiom.Watch ? "outer_bg_on" : "bg_on";
			RegisterPropertyHandler(Switch.IsToggledProperty, HandleToggled);
			RegisterPropertyHandler(Switch.OnColorProperty, UpdateOnColor);
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
				_onColorEdjePart = Control.ClassName.ToLower().Replace("elm_", "") + "/" + _onColorPart;
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

		void UpdateOnColor(bool initialize)
		{
			if (initialize && Element.OnColor.IsDefault)
				return;

			if (Element.OnColor.IsDefault)
			{
				Control.EdjeObject.DeleteColorClass(_onColorEdjePart);
				if (_isTV)
					Control.EdjeObject.DeleteColorClass(_onColorEdjePart.Replace(_onColorPart, "slider_focused_on"));
			}
			else
			{
				EColor color = Element.OnColor.ToNative();
				Control.SetPartColor(_onColorPart, color);
				if (_isTV)
					Control.SetPartColor("slider_focused_on", color);
			}
		}
	}
}
