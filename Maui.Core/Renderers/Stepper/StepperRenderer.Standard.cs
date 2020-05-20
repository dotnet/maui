namespace System.Maui.Platform
{
	public partial class StepperRenderer : AbstractViewRenderer<IStepper, object>
	{
		protected override object CreateView() => throw new NotImplementedException();

		public static void MapPropertyMinimum(IViewRenderer renderer, IStepper slider) { }
		public static void MapPropertyMaximum(IViewRenderer renderer, IStepper slider) { }
		public static void MapPropertyIncrement(IViewRenderer renderer, IStepper slider) { }
		public static void MapPropertyValue(IViewRenderer renderer, IStepper slider) { }
	}
}