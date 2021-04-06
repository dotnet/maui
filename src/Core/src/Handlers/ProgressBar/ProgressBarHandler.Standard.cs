using System;

namespace Microsoft.Maui.Handlers
{
	public partial class ProgressBarHandler : ViewHandler<IProgress, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();

		public static void MapProgress(ProgressBarHandler handler, IProgress progress) { }
	}
}