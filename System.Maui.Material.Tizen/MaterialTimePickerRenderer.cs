using System;
using System.Globalization;
using System.Maui;
using System.Maui.Material.Tizen;
using System.Maui.Material.Tizen.Native;
using System.Maui.Platform.Tizen;

[assembly: ExportRenderer(typeof(TimePicker), typeof(MaterialTimePickerRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
namespace System.Maui.Material.Tizen
{
	public class MaterialTimePickerRenderer : TimePickerRenderer
	{
		Color _defaultTitleColor = Color.Black;
		static readonly string _defaultFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;

		protected override ElmSharp.Entry CreateNativeControl()
		{
			return new MPicker(System.Maui.Maui.NativeParent);
		}

		protected override void UpdateTimeAndFormat()
		{
			if (Control is MPicker mp)
			{
				// Xamarin using DateTime formatting (https://developer.xamarin.com/api/property/System.Maui.TimePicker.Format/)
				mp.Placeholder = new DateTime(Time.Ticks).ToString(Element.Format ?? _defaultFormat);
			}
		}

		protected override void UpdateTextColor()
		{
			if (Control is MPicker mp)
			{
				if (Element.TextColor.IsDefault)
				{
					mp.PlaceholderColor = _defaultTitleColor.ToNative();
				}
				else
				{
					mp.PlaceholderColor = Element.TextColor.ToNative();
				}
			}
		}
	}
}