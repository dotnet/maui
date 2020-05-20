namespace System.Maui.Platform
{
	public partial class ActivityIndicatorRenderer : AbstractViewRenderer<IActivityIndicator, object>
	{
		protected override object CreateView() => throw new NotImplementedException();

		public static void MapPropertyIsRunning(IViewRenderer renderer, IActivityIndicator activityIndicator) { }
		public static void MapPropertyColor(IViewRenderer renderer, IActivityIndicator activityIndicator) { }
	}
}