using System.Maui;
using System.Maui.Platform.Tizen;
using System.Maui.Platform.Tizen.Native;
using System.Maui.Material.Tizen;
using XFButton = System.Maui.Button;
using EButton = ElmSharp.Button;

[assembly: ExportRenderer(typeof(XFButton), typeof(MaterialButtonRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
namespace System.Maui.Material.Tizen
{
	public class MaterialButtonRenderer : ButtonRenderer
	{
		protected override EButton CreateNativeControl()
		{
			return new MaterialButton(System.Maui.Maui.NativeParent);
		}
	}
}
