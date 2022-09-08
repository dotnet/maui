using System;

namespace Microsoft.Maui.Handlers
{
	public partial class FlyoutViewHandler : ViewHandler<IFlyoutView, NotImplementedView>
	{
		protected override NotImplementedView CreatePlatformView() => new();
	}
}