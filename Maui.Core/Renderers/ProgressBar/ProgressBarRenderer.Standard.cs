namespace System.Maui.Platform
{
	public partial class ProgressBarRenderer : AbstractViewRenderer<IProgress, object>
	{
		protected override object CreateView() => throw new NotImplementedException();

		public static void MapPropertyProgress(IViewRenderer renderer, IProgress progressBar) { }
		public static void MapPropertyProgressColor(IViewRenderer renderer, IProgress progressBar) { }
	}
}