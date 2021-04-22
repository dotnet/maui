using System;

namespace Microsoft.Maui.Handlers
{
	public partial class PageHandler : ViewHandler<IPage, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();
	}
}
