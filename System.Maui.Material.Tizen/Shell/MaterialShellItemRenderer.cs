using Xamarin.Forms.Platform.Tizen;

namespace Xamarin.Forms.Material.Tizen
{
	public class MaterialShellItemRenderer : ShellItemRenderer
	{
		public MaterialShellItemRenderer(IFlyoutController flyoutController, ShellItem item) : base(flyoutController, item)
		{
		}

		protected override IShellTabs CreateTabs()
		{
			return new MaterialShellTabs(Forms.NativeParent);
		}

		protected override ShellSectionNavigation CreateShellSectionNavigation(IFlyoutController flyoutController, ShellSection section)
		{
			return new MaterialShellSectionNavigation(flyoutController, section);
		}
	}
}
