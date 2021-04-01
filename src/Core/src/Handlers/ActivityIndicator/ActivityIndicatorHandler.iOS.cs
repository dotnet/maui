using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ActivityIndicatorHandler : ViewHandler<IActivityIndicator, MauiActivityIndicator>
	{
		protected override MauiActivityIndicator CreateNativeView() => new MauiActivityIndicator(CGRect.Empty, VirtualView)
		{
			ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.Gray
		};
	}
}