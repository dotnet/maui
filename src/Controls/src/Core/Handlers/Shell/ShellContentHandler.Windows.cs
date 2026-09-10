#nullable disable
using System;
using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class ShellContentHandler : ElementHandler<ShellContent, FrameworkElement>
	{
		public static PropertyMapper<ShellContent, ShellContentHandler> Mapper =
				new PropertyMapper<ShellContent, ShellContentHandler>(ElementMapper);

		public static CommandMapper<ShellContent, ShellContentHandler> CommandMapper =
				new CommandMapper<ShellContent, ShellContentHandler>(ElementCommandMapper);

		public ShellContentHandler() : base(Mapper, CommandMapper)
		{
		}

		protected override FrameworkElement CreatePlatformElement()
		{
			return (VirtualView as IShellContentController).GetOrCreateContent().ToPlatform(MauiContext);
		}
	}
}
