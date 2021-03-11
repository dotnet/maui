using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ProgressBarHandler : AbstractViewHandler<IProgress, UIProgressView>
	{
		protected override UIProgressView CreateNativeView()
		{
			return new UIProgressView(UIProgressViewStyle.Default);
		}
	}
}