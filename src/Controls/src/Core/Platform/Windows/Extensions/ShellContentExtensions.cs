using Microsoft.Maui.Controls.Handlers;

namespace Microsoft.Maui.Controls.Platform
{
	internal static class ShellContentExtensions
	{
		internal static void UpdateParentShellItemTitle(this ShellContent shellContent)
		{
			if (shellContent.Parent is ShellSection shellSection && shellSection.Parent is ShellItem shellItem && shellItem.Handler is ShellItemHandler shellItemHandler)
			{
				shellItemHandler.UpdateTitle();
			}
		}
	}
}
