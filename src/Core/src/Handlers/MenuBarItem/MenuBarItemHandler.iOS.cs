using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Platform;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class MenuBarItemHandler : ElementHandler<IMenuBarItem, UIView>, IMenuBarItemHandler
	{
		protected override UIView CreateNativeElement()
		{
			throw new NotImplementedException();
		}

		public void Add(IMenuFlyoutItemBase view)
		{
		}

		public void Remove(IMenuFlyoutItemBase view)
		{
		}

		public void Clear()
		{
		}

		public void Insert(int index, IMenuFlyoutItemBase view)
		{
		}
	}
}
