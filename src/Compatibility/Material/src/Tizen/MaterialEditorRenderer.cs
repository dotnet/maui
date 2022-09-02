using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.Material.Tizen;
using Microsoft.Maui.Controls.Compatibility.Material.Tizen.Native;
using Microsoft.Maui.Controls.Platform.Tizen;
using Microsoft.Maui.Controls.Platform.Tizen.Native;

[assembly: ExportRenderer(typeof(Editor), typeof(MaterialEditorRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
namespace Microsoft.Maui.Controls.Compatibility.Material.Tizen
{
	public class MaterialEditorRenderer : EditorRenderer
	{
		protected override ElmSharp.Entry CreateNativeControl()
		{
			return new MEditor(Forms.NativeParent)
			{
				IsSingleLine = false,
				LineWrapType = ElmSharp.WrapType.Mixed
			};
		}

		protected override void UpdateTextColor()
		{
			if (Control is MEditor me)
			{
				me.TextColor = Element.TextColor.ToPlatform();
				me.TextFocusedColor = Element.TextColor.ToPlatform();
				me.UnderlineColor = Element.TextColor.ToPlatform();
				me.UnderlineFocusedColor = Element.TextColor.ToPlatform();
				me.CursorColor = Element.TextColor.ToPlatform();
			}
		}
	}
}