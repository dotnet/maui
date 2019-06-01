using Xamarin.Forms;
using Xamarin.Forms.Platform.Tizen;
using Xamarin.Forms.Platform.Tizen.Native;
using Xamarin.Forms.Material.Tizen;
using TForms = Xamarin.Forms.Platform.Tizen.Forms;
using XFEntry = Xamarin.Forms.Entry;

[assembly: ExportRenderer(typeof(XFEntry), typeof(MaterialEntryRenderer), new[] { typeof(VisualMarker.MaterialVisual) })]
namespace Xamarin.Forms.Material.Tizen
{
	public class MaterialEntryRenderer : EntryRenderer
	{
		protected override ElmSharp.Entry CreateNativeControl()
		{
			return new MaterialEntry(TForms.NativeParent)
			{
				IsSingleLine = true,
			};
		}

		protected override void UpdateTextColor()
		{
			if(Control is MaterialEntry me)
			{
				me.TextColor = Element.TextColor.ToNative();
				me.TextFocusedColor = Element.TextColor.ToNative();
				me.UnderlineColor = Element.TextColor.ToNative();
				me.UnderlineFocusedColor = Element.TextColor.ToNative();
				me.CursorColor = Element.TextColor.ToNative();
			}
		}
	}
}
