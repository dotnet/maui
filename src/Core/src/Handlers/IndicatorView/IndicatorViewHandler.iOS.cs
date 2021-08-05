using System;
using System.Linq;
using UIKit;
using NativeView = UIKit.UIView;

namespace Microsoft.Maui.Handlers
{
	public partial class IndicatorViewHandler : ViewHandler<IIndicatorView, NativeView>
	{
		protected override NativeView CreateNativeView()
		{
			return new UIView();
		}
	}
}
