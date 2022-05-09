using System;
using ElmSharp;

namespace Microsoft.Maui.Handlers
{
	public partial class MenuBarItemHandler : ElementHandler<IMenuBarItem, EvasObject>, IMenuBarItemHandler
	{
		// TODO : Need to implement
		protected override EvasObject CreatePlatformElement()
		{
			throw new NotImplementedException();
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
