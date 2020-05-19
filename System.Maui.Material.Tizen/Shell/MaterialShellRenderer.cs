using System.Maui;
using System.Maui.Material.Tizen;
using System.Maui.Platform.Tizen;

[assembly: ExportRenderer(typeof(Shell), typeof(MaterialShellRenderer), new[] { typeof(VisualMarker.MaterialVisual) })]
namespace System.Maui.Material.Tizen
{
	public class MaterialShellRenderer : ShellRenderer
	{
		protected override INavigationDrawer CreateNavigationDrawer()
		{
			return new MaterialNavigationDrawer(System.Maui.Maui.NativeParent);
		}

		protected override INavigationView CreateNavigationView()
		{
			return new MaterialNavigationView(System.Maui.Maui.NativeParent);
		}

		protected override ShellItemRenderer CreateShellItem(ShellItem item)
		{
			return new MaterialShellItemRenderer(this, item);
		}
	}
}
