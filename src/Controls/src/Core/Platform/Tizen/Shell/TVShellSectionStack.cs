namespace Microsoft.Maui.Controls.Platform
{
	public class TVShellSectionStack : ShellSectionStack
	{
		public TVShellSectionStack(ShellSection section, IMauiContext context) : base(section, context)
		{
		}

		public override bool NavBarIsVisible => false;

		protected override IShellSectionHandler CreateShellSectionView(ShellSection section)
		{
			return new TVShellSectionHandler(section, MauiContext);
		}
	}
}
