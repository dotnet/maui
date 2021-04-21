using System;

namespace Microsoft.Maui.Handlers
{
	public partial class WindowHandler : ViewHandler<IWindow, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();
	}
}
