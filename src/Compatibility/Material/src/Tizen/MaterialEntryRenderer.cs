using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.Material.Tizen;
using Microsoft.Maui.Controls.Platform.Tizen;
using Microsoft.Maui.Controls.Platform.Tizen.Native;
using XFEntry = Microsoft.Maui.Controls.Entry;

[assembly: ExportRenderer(typeof(XFEntry), typeof(MaterialEntryRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
namespace Microsoft.Maui.Controls.Compatibility.Material.Tizen
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
				me.TextColor = Element.TextColor.ToPlatform();
				me.TextFocusedColor = Element.TextColor.ToPlatform();
				me.UnderlineColor = Element.PlaceholderColor.ToPlatform();
				me.UnderlineFocusedColor = Element.PlaceholderColor.ToPlatform();
				me.CursorColor = Element.TextColor.ToPlatform();
			}
		}
	}
}
