using System.Maui.Platform.Tizen;

namespace System.Maui.Material.Tizen
{
	public class MaterialShellItemRenderer : ShellItemRenderer
	{
		public MaterialShellItemRenderer(IFlyoutController flyoutController, ShellItem item) : base(flyoutController, item)
		{
		}

		protected override IShellTabs CreateTabs()
		{
			return new MaterialShellTabs(System.Maui.Maui.NativeParent);
		}

		protected override ShellSectionNavigation CreateShellSectionNavigation(IFlyoutController flyoutController, ShellSection section)
		{
			return new MaterialShellSectionNavigation(flyoutController, section);
		}
	}
}
