using System;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class ProgressBarHandler : AbstractViewHandler<IProgress, ProgressBar>
	{
		protected override ProgressBar CreateNativeView() => throw new NotImplementedException();
	}
}