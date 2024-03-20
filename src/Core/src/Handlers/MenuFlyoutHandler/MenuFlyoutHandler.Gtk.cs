using System;
using PlatformView = Microsoft.Maui.Platform.MauiMenu;

namespace Microsoft.Maui.Handlers
{
	public partial class MenuFlyoutHandler : ElementHandler<IMenuFlyout, PlatformView>, IMenuFlyoutHandler
	{
		protected override PlatformView CreatePlatformElement()
		{
			return new();
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
