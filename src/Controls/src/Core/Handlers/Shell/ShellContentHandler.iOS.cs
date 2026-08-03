#nullable enable
using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers
{
	/// <summary>
	/// Routes <see cref="ShellContent.Content"/> changes to the owning <see cref="ShellSectionHandler"/> on iOS.
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

		// Placeholder view; ShellSectionHandler renders the actual content.
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
