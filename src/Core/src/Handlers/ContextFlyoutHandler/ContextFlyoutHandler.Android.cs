using System;

namespace Microsoft.Maui.Handlers
{
	public partial class ContextFlyoutHandler : ElementHandler<IContextFlyout, object>, IContextFlyoutHandler
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
