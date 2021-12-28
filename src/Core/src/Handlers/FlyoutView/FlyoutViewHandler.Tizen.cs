using System;
using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Handlers
{
	public partial class FlyoutViewHandler : ViewHandler<IFlyoutView, NView>
	{
		protected override NView CreatePlatformView()
		{
			//TODO : Need to impl
			throw new NotImplementedException();
		}
	}
}
