using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ActivityIndicatorHandler : AbstractViewHandler<IActivityIndicator, NativeActivityIndicator>
	{
		protected override NativeActivityIndicator CreateNativeView() => new NativeActivityIndicator(CGRect.Empty, VirtualView)
		{
			ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.Gray
		};
	}
}