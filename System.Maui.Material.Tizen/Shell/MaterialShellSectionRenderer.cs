using Xamarin.Forms.Platform.Tizen;

namespace Xamarin.Forms.Material.Tizen
{
	public class MaterialShellSectionRenderer : ShellSectionRenderer
	{
		public MaterialShellSectionRenderer(ShellSection section) : base(section)
		{
		}

		protected override IShellTabs CreateToolbar()
		{
			return new MaterialShellTabs(Forms.NativeParent);
		}
	}
}
