using System;

namespace Microsoft.Maui.Handlers
{
	public partial class SwipeViewHandler : ViewHandler<ISwipeView, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();
	}
}