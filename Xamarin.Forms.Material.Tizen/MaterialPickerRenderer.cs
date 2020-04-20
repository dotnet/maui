using System;
using Xamarin.Forms;
using Xamarin.Forms.Material.Tizen;
using Xamarin.Forms.Material.Tizen.Native;
using Xamarin.Forms.Platform.Tizen;

[assembly: ExportRenderer(typeof(Picker), typeof(MaterialPickerRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
namespace Xamarin.Forms.Material.Tizen
{
	public class MaterialPickerRenderer : PickerRenderer
	{
		Color _defaultTitleColor = Color.Black;

		protected override ElmSharp.Entry CreateNativeControl()
		{
			return new MPicker(Forms.NativeParent);
		}

		protected override void UpdateSelectedIndex()
		{
			if (Control is MPicker mp)
			{
				mp.Placeholder = (Element.SelectedIndex == -1 || Element.Items == null ?
				"" : Element.Items[Element.SelectedIndex]);
			}
		}

		protected override void UpdateTitleColor()
		{
			if (Control is MPicker mp)
			{
				if (Element.TitleColor.IsDefault)
				{
					mp.PlaceholderColor = _defaultTitleColor.ToNative();
				}
				else
				{
					mp.PlaceholderColor = Element.TitleColor.ToNative();
				}
			}
		}

		protected override void UpdateTextColor()
		{
			if (Control is MPicker mp)
			{
				if (Element.TextColor.IsDefault)
				{
					mp.TextColor = _defaultTitleColor.ToNative();
				}
				else
				{
					mp.TextColor = Element.TextColor.ToNative();
				}
			}
		}
	}
}