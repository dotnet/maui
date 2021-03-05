using static Android.Resource;
using AndroidProgressBar = Android.Widget.ProgressBar;

namespace Microsoft.Maui.Handlers
{
	public partial class ProgressBarHandler : AbstractViewHandler<IProgress, AndroidProgressBar>
	{
		protected override AndroidProgressBar CreateNativeView()
		{
			return new AndroidProgressBar(Context, null, Attribute.ProgressBarStyleHorizontal)
			{
				Indeterminate = false,
				Max = ProgressBarExtensions.Maximum
			};
		}
	}
}