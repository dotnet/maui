using Xamarin.Forms;
using Xamarin.Forms.Material.Tizen;
using Xamarin.Forms.Platform.Tizen;
using Xamarin.Forms.Platform.Tizen.Native;
using XFEntry = Xamarin.Forms.Entry;

[assembly: ExportRenderer(typeof(XFEntry), typeof(MaterialEntryRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
namespace Xamarin.Forms.Material.Tizen
{
	public class MaterialEntryRenderer : EntryRenderer
	{
		protected override ElmSharp.Entry CreateNativeControl()
		{
			return new MaterialEntry(Forms.NativeParent)
			{
				IsSingleLine = true,
			};
		}

		protected override void UpdateTextColor()
		{
			if (Control is MaterialEntry me)
			{
				me.TextColor = Element.TextColor.ToNative();
				me.TextFocusedColor = Element.TextColor.ToNative();
				me.UnderlineColor = Element.PlaceholderColor.ToNative();
				me.UnderlineFocusedColor = Element.PlaceholderColor.ToNative();
				me.CursorColor = Element.TextColor.ToNative();
			}
		}
	}
}
