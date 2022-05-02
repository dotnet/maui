using System;
using System.Collections.Generic;
using System.Text;
using Android.Views;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Handlers
{
	public partial class MenuBarHandler : ElementHandler<IMenuBar, View>
	{
		protected override View CreatePlatformElement()
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
