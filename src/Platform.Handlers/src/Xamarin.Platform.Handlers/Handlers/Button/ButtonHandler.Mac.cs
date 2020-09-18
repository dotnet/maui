using System;
using AppKit;

namespace Xamarin.Platform.Handlers
{
	public partial class ButtonHandler : AbstractViewHandler<IButton, NSButton>
	{
		protected override NSButton CreateView() => throw new NotImplementedException();

		public static void MapText(IViewHandler handler, IButton view) { }
	}
}