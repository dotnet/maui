using System.Maui.Platform.Tizen;

namespace System.Maui.Material.Tizen
{
	public class MaterialShellSectionNavigation : ShellSectionNavigation
	{
		public MaterialShellSectionNavigation(IFlyoutController flyoutController, ShellSection section) : base(flyoutController, section)
		{
		}

		protected override ShellSectionRenderer CreateShellSection(ShellSection section)
		{
			return new MaterialShellSectionRenderer(section);
		}
	}
}
