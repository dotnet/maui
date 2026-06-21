using System;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
	public partial class MenuFlyoutHandler : ElementHandler<IMenuFlyout, UIMenu>, IMenuFlyoutHandler
	{
		protected override UIMenu CreatePlatformElement()
		{
			var platformMenu =
				VirtualView
					.ToPlatformMenu(MauiContext!);

			return platformMenu;
		}

		public void Add(IMenuElement view)
		{
		}

		public void Remove(IMenuElement view)
		{
		}

		public void Clear()
		{
		}

		public void Insert(int index, IMenuElement view)
		{
		}
	}
}
