using System;

namespace Microsoft.Maui.Handlers
{
	public partial class MenuFlyoutHandler : ElementHandler<IMenuFlyout, object>, IMenuFlyoutHandler
	{
		protected override object CreatePlatformElement()
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
