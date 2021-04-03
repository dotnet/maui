using System;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class ActivityIndicatorHandler : ViewHandler<IActivityIndicator, ProgressBar>
	{
		protected override ProgressBar CreateNativeView() => new ProgressBar();
	}
}