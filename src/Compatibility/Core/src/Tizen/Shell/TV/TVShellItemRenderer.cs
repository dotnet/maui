using System;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen.TV
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class TVShellItemRenderer : ShellItemRenderer
	{
		public TVShellItemRenderer(ShellItem item) : base(item)
		{
		}

		protected override ShellSectionStack CreateShellSectionStack(ShellSection section)
		{
			return new TVShellSectionStack(section);
		}

		protected override void UpdateTabsItems()
		{
		}
	}
}
