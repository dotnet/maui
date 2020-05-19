using Xamarin.Forms;
using Xamarin.Forms.Material.Tizen;
using Xamarin.Forms.Material.Tizen.Native;
using Xamarin.Forms.Platform.Tizen;
using Xamarin.Forms.Platform.Tizen.Native;

[assembly: ExportRenderer(typeof(Editor), typeof(MaterialEditorRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
namespace Xamarin.Forms.Material.Tizen
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
				me.TextColor = Element.TextColor.ToNative();
				me.TextFocusedColor = Element.TextColor.ToNative();
				me.UnderlineColor = Element.TextColor.ToNative();
				me.UnderlineFocusedColor = Element.TextColor.ToNative();
				me.CursorColor = Element.TextColor.ToNative();
			}
		}
	}
}