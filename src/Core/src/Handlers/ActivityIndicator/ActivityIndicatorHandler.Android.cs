using Android.Widget;

namespace Microsoft.Maui.Handlers
{
	public partial class ActivityIndicatorHandler : AbstractViewHandler<IActivityIndicator, ProgressBar>
	{
		protected override ProgressBar CreateNativeView() => new ProgressBar(Context) { Indeterminate = true };
	}
}