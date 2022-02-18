using System;

namespace Microsoft.Maui.Handlers
{
	public partial class ProgressBarHandler : ViewHandler<IProgress, object>
	{
		protected override object CreatePlatformView() => throw new NotImplementedException();

		public static void MapProgress(ProgressBarHandler handler, IProgress progress) { }
		public static void MapProgressColor(ProgressBarHandler handler, IProgress progress) { }
	}
}