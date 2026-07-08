using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.Material.Tizen;
using Microsoft.Maui.Controls.Platform.Tizen;
using Microsoft.Maui.Controls.Platform.Tizen.Native;
using EButton = ElmSharp.Button;
using XFButton = Microsoft.Maui.Controls.Button;

[assembly: ExportRenderer(typeof(XFButton), typeof(MaterialButtonRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
namespace Microsoft.Maui.Controls.Compatibility.Material.Tizen
{
	public class MaterialButtonRenderer : ButtonRenderer
	{
		protected override EButton CreateNativeControl()
		{
			return new MaterialButton(Forms.NativeParent);
		}
	}
}
