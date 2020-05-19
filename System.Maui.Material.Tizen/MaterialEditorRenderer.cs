using System.Maui;
using System.Maui.Material.Tizen;
using System.Maui.Material.Tizen.Native;
using System.Maui.Platform.Tizen;
using System.Maui.Platform.Tizen.Native;

[assembly: ExportRenderer(typeof(Editor), typeof(MaterialEditorRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
namespace System.Maui.Material.Tizen
{
	public class MaterialEditorRenderer : EditorRenderer
	{
		protected override ElmSharp.Entry CreateNativeControl()
		{
			return new MEditor(System.Maui.Maui.NativeParent)
			{
				IsSingleLine = false,
				LineWrapType = ElmSharp.WrapType.Mixed
			};
		}

		protected override void UpdateTextColor()
		{
			if (Control is MEditor me)
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