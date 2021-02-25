using System.ComponentModel;

namespace Xamarin.Forms.Platform.Tizen.TV
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
