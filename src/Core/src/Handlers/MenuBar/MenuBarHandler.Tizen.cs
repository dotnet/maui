using System;
using ElmSharp;

namespace Microsoft.Maui.Handlers
{
	public partial class MenuBarHandler : ElementHandler<IMenuBar, EvasObject>, IMenuBarHandler
	{
		// TODO : Need to implement
		protected override EvasObject CreatePlatformElement()
		{
			throw new NotImplementedException();
		}

		public void Add(IMenuBarItem view)
		{
		}

		public void Remove(IMenuBarItem view)
		{
		}

		public void Clear()
		{
		}

		public void Insert(int index, IMenuBarItem view)
		{
		}
	}
}
