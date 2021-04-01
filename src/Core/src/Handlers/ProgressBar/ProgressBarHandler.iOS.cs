using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ProgressBarHandler : ViewHandler<IProgress, UIProgressView>
	{
		protected override UIProgressView CreateNativeView()
		{
			return new UIProgressView(UIProgressViewStyle.Default);
		}
	}
}