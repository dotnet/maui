using Xamarin.Forms;
using Xamarin.Forms.Material.Tizen;
using Xamarin.Forms.Platform.Tizen;

[assembly: ExportRenderer(typeof(Shell), typeof(MaterialShellRenderer), new[] { typeof(VisualMarker.MaterialVisual) })]
namespace Xamarin.Forms.Material.Tizen
{
	public class MaterialShellRenderer : ShellRenderer
	{
		protected override INavigationDrawer CreateNavigationDrawer()
		{
			return new MaterialNavigationDrawer(Forms.NativeParent);
		}

		protected override INavigationView CreateNavigationView()
		{
			return new MaterialNavigationView(Forms.NativeParent);
		}

		protected override ShellItemRenderer CreateShellItem(ShellItem item)
		{
			return new MaterialShellItemRenderer(this, item);
		}
	}
}
