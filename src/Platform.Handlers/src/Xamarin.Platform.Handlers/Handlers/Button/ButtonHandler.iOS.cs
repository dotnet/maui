using System;
using UIKit;

namespace Xamarin.Platform.Handlers
{
	public partial class ButtonHandler : AbstractViewHandler<IButton, UIButton>
	{
		protected override UIButton CreateView() => new UIButton();

		public static void MapText(IViewHandler handler, IButton view) { }
	}
}