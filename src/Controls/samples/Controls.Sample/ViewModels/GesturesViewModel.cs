using System.Collections.Generic;
using Maui.Controls.Sample.Models;
using Maui.Controls.Sample.Pages;
using Maui.Controls.Sample.ViewModels.Base;

namespace Maui.Controls.Sample.ViewModels
{
	public class GesturesViewModel : BaseGalleryViewModel
	{
		protected override IEnumerable<SectionModel> CreateItems() => new[]
		{
			new SectionModel(typeof(DragAndDropBetweenLayouts), "Drag And Drop",
				"Drag and Drop Views."),
			new SectionModel(typeof(DropFileToMauiApp), "Drag and Drop file from OS",
				"Drop File to App"),
			new SectionModel(typeof(PanGestureGallery), "Pan Gesture",
				"Pan Gesture."),
			new SectionModel(typeof(PinchGestureTestPage), "Pinch Gesture",
				"Pinch Gesture."),
			new SectionModel(typeof(PointerGestureGalleryPage), "Pointer Gesture",
				"Pointer Gesture."),
			new SectionModel(typeof(SwipeGestureGalleryPage), "Swipe Gesture",
				"Swipe Gesture."),
			new SectionModel(typeof(TapGestureGalleryPage), "Tap Gesture",
				"Tap Gesture."),
		};
	}
}