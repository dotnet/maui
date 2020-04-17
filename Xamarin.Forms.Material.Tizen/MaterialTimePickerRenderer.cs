using System;
using System.Globalization;
using Xamarin.Forms;
using Xamarin.Forms.Material.Tizen;
using Xamarin.Forms.Material.Tizen.Native;
using Xamarin.Forms.Platform.Tizen;

[assembly: ExportRenderer(typeof(TimePicker), typeof(MaterialTimePickerRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
namespace Xamarin.Forms.Material.Tizen
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
				// Xamarin using DateTime formatting (https://developer.xamarin.com/api/property/Xamarin.Forms.TimePicker.Format/)
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