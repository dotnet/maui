using System;


namespace System.Maui.Platform {
	public partial class MauiRenderer
	{
		public static PropertyMapper<IView> ViewMapper = new PropertyMapper<IView> {
			[nameof(IView.IsEnabled)] = MapPropertyIsEnabled,
			[nameof(IView.BackgroundColor)] = MapBackgroundColor,
			//[nameof(IView.Frame)] = MapPropertyFrame,
			//[nameof(IClipShapeView.ClipShape)] = MapPropertyClipShape
		};

		//public static void MapPropertyFrame(IMauiRenderer renderer, IView view)
		//	=> renderer?.SetFrame(view.Frame);


		//public static void MapPropertyClipShape(IMauiRenderer renderer, IView view)
		//{
		//	if (!(view is IClipShapeView clipShape))
		//		return;
		//	//If we are ever going to set HasContainer = false,
		//	//we need to add a method to verify if anything else requires it
		//	if (clipShape.ClipShape != null)
		//		renderer.HasContainer = true;
		//	if (renderer.ContainerView != null)
		//		renderer.ContainerView.ClipShape = clipShape.ClipShape;
		//}
	}
}
