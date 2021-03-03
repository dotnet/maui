using System.ComponentModel;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen.TV
{
	public class TVShellSectionStack : ShellSectionStack
	{

		public TVShellSectionStack(ShellSection section) : base(section)
		{
		}

		public override bool NavBarIsVisible => false;

		protected override IShellSectionRenderer CreateShellSectionRenderer(ShellSection section)
		{
			return new TVShellSectionRenderer(section);
		}
	}
}
