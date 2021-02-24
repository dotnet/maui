using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.Material.Tizen;
using Microsoft.Maui.Controls.Compatibility.Material.Tizen.Native;
using Microsoft.Maui.Controls.Platform.Tizen;

[assembly: ExportRenderer(typeof(TimePicker), typeof(MaterialTimePickerRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
namespace Microsoft.Maui.Controls.Compatibility.Material.Tizen
{
	public class MaterialTimePickerRenderer : TimePickerRenderer
	{
		Color _defaultTitleColor = Color.Black;
		static readonly string _defaultFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;

		protected override ElmSharp.Entry CreateNativeControl()
		{
			return new MPicker(Forms.NativeParent);
		}

		protected override void UpdateTimeAndFormat()
		{
			if (Control is MPicker mp)
			{
				// Microsoft.Maui.Controls using DateTime formatting (https://developer.xamarin.com/api/property/Microsoft.Maui.Controls.TimePicker.Format/)
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