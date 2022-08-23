using System;

namespace Microsoft.Maui.Handlers
{
	public partial class MenuBarItemHandler : ElementHandler<IMenuBarItem, Gtk.Widget>, IMenuBarItemHandler
	{
		protected override Gtk.Widget CreatePlatformElement()
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
