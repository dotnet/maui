using System;
using Android.App;

namespace Microsoft.Maui.Handlers
{
	public partial class WindowHandler : ElementHandler<IWindow, Activity>, INativeWindowHandler
	{
		Activity? _activity;

		protected override Activity CreateNativeElement() =>
			_activity ?? throw new InvalidOperationException("Android does now support creating new activities directly.");

		public void SetWindow(Activity activity) =>
			_activity = activity;

		public static void MapTitle(WindowHandler handler, IWindow window) { }
	}
}