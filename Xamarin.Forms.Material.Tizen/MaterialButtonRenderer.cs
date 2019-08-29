using Xamarin.Forms;
using Xamarin.Forms.Platform.Tizen;
using Xamarin.Forms.Platform.Tizen.Native;
using Xamarin.Forms.Material.Tizen;
using XFButton = Xamarin.Forms.Button;
using EButton = ElmSharp.Button;

[assembly: ExportRenderer(typeof(XFButton), typeof(MaterialButtonRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
namespace Xamarin.Forms.Material.Tizen
{
	public class MaterialButtonRenderer : ButtonRenderer
	{
		protected override EButton CreateNativeControl()
		{
			return new MaterialButton(Forms.NativeParent);
		}
	}
}
