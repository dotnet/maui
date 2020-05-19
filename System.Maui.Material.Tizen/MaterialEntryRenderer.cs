using System.Maui;
using System.Maui.Platform.Tizen;
using System.Maui.Platform.Tizen.Native;
using System.Maui.Material.Tizen;
using XFEntry = System.Maui.Entry;

[assembly: ExportRenderer(typeof(XFEntry), typeof(MaterialEntryRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
namespace System.Maui.Material.Tizen
{
	public class MaterialEntryRenderer : EntryRenderer
	{
		protected override ElmSharp.Entry CreateNativeControl()
		{
			return new MaterialEntry(System.Maui.Maui.NativeParent)
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
				me.UnderlineColor = Element.PlaceholderColor.ToNative();
				me.UnderlineFocusedColor = Element.PlaceholderColor.ToNative();
				me.CursorColor = Element.TextColor.ToNative();
			}
		}
	}
}
