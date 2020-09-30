using Xamarin.Forms;
using Xamarin.Forms.Material.Tizen;
using Xamarin.Forms.Material.Tizen.Native;
using Xamarin.Forms.Platform.Tizen;

[assembly: ExportRenderer(typeof(DatePicker), typeof(MaterialDatePickerRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
namespace Xamarin.Forms.Material.Tizen
{
	public class MaterialDatePickerRenderer : DatePickerRenderer
	{
		Color _defaultTitleColor = Color.Black;

		protected override ElmSharp.Entry CreateNativeControl()
		{
			return new MPicker(Forms.NativeParent);
		}

		protected override void OnDateTimeChanged(object sender, Platform.Tizen.Native.DateChangedEventArgs dcea)
		{
			Element.Date = dcea.NewDate;
			if (Control is MPicker mp)
			{
				mp.Placeholder = dcea.NewDate.ToString(Element.Format);
			}
		}

		protected override void UpdateDate()
		{
			if (Control is MPicker mp)
			{
				mp.Placeholder = Element.Date.ToString(Element.Format);
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