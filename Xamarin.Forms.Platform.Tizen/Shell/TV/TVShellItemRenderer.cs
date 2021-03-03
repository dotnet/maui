using System;

namespace Xamarin.Forms.Platform.Tizen.TV
{
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
