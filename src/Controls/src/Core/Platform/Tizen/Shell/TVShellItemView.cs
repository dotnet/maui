namespace Microsoft.Maui.Controls.Platform
{
	public class TVShellItemView : ShellItemView
	{
		public TVShellItemView(ShellItem item, IMauiContext context) : base(item, context)
		{
		}

		protected override ShellSectionStack CreateShellSectionStack(ShellSection section)
		{
			return new TVShellSectionStack(section, MauiContext);
		}

		protected override void UpdateTabsItems()
		{
		}
	}
}
