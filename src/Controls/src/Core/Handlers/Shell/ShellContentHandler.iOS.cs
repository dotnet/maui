#nullable enable
using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers
{
	/// <summary>
	/// Handler-era replacement for the previous manual <c>ShellContent.PropertyChanged</c>
	/// subscription. <see cref="ShellContent"/> has no visible platform view of its own on iOS —
	/// its <see cref="ShellContent.Content"/> is rendered by the owning
	/// <see cref="ShellSectionHandler"/> — so this handler exists purely to give
	/// <see cref="ShellContent"/> a genuine <see cref="Mapper"/> entry point for
	/// <see cref="ShellContent.Content"/> changes, mirroring the existing Windows
	/// <c>ShellContentHandler</c>.
	/// </summary>
	public partial class ShellContentHandler : ElementHandler<ShellContent, UIView>
	{
		public static PropertyMapper<ShellContent, ShellContentHandler> Mapper =
			new PropertyMapper<ShellContent, ShellContentHandler>(ElementMapper)
			{
				[nameof(ShellContent.Content)] = MapContent,
			};

		public static CommandMapper<ShellContent, ShellContentHandler> CommandMapper =
			new CommandMapper<ShellContent, ShellContentHandler>(ElementCommandMapper);

		public ShellContentHandler() : base(Mapper, CommandMapper)
		{
		}

		// No native visual - the actual page renderer lives on ShellSectionHandler.
		protected override UIView CreatePlatformElement() => new UIView();

		internal static void MapContent(ShellContentHandler handler, ShellContent shellContent)
		{
			if (shellContent.Parent is ShellSection shellSection &&
				shellSection.Handler is ShellSectionHandler sectionHandler)
			{
				sectionHandler.OnShellContentContentChanged(shellContent);
			}
		}
	}
}
