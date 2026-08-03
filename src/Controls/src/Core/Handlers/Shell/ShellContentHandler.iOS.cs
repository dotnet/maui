#nullable enable
using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers
{
	/// <summary>
	/// Provides a mapper entry point for <see cref="ShellContent.Content"/> changes on iOS.
	/// <see cref="ShellContent"/> has no platform view of its own — its content is rendered
	/// by the owning <see cref="ShellSectionHandler"/>. This handler exists solely to
	/// route <see cref="ShellContent.Content"/> property changes to the section handler.
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
