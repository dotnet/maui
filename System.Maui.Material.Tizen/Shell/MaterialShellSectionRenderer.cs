using System.Maui.Platform.Tizen;

namespace System.Maui.Material.Tizen
{
	public class MaterialShellSectionRenderer : ShellSectionRenderer
	{
		public MaterialShellSectionRenderer(ShellSection section) : base(section)
		{
		}

		protected override IShellTabs CreateToolbar()
		{
			return new MaterialShellTabs(System.Maui.Maui.NativeParent);
		}
	}
}
