﻿#nullable disable
using System;
using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class ShellContentHandler : ElementHandler<ShellContent, FrameworkElement>
	{
		public static PropertyMapper<ShellContent, ShellContentHandler> Mapper =
				new PropertyMapper<ShellContent, ShellContentHandler>(ElementMapper) { [nameof(ShellContent.Title)] = MapTitle };

		public static CommandMapper<ShellContent, ShellContentHandler> CommandMapper =
				new CommandMapper<ShellContent, ShellContentHandler>(ElementCommandMapper);

		public ShellContentHandler() : base(Mapper, CommandMapper)
		{
		}

		internal static void MapTitle(ShellContentHandler handler, ShellContent item)
		{
			var shellSection = item.Parent as ShellSection;
			var shellItem = shellSection?.Parent as ShellItem;
			var shellItemHandler = shellItem?.Handler as ShellItemHandler;
			shellItemHandler?.UpdateTitle();
		}

		protected override FrameworkElement CreatePlatformElement()
		{
			return (VirtualView as IShellContentController).GetOrCreateContent().ToPlatform(MauiContext);
		}
	}
}
