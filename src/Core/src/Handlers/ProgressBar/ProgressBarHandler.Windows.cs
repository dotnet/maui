using System;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class ProgressBarHandler : ViewHandler<IProgress, ProgressBar>
	{
		protected override ProgressBar CreateNativeView() => new ProgressBar();

		[MissingMapper]
		public static void MapProgress(ProgressBarHandler handler, IProgress progress) { }
	}
}