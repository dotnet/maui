using System.Collections.Generic;
using Maui.Controls.Sample.Models;
using Maui.Controls.Sample.Pages;
using Maui.Controls.Sample.ViewModels.Base;

namespace Maui.Controls.Sample.ViewModels
{
	public class CoreViewModel : BaseGalleryViewModel
	{
		protected override IEnumerable<SectionModel> CreateItems() => new[]
		{
			new SectionModel(typeof(AlertsPage), "Alerts",
				"Displaying an alert, asking a user to make a choice, or displaying a prompt."),

			new SectionModel(typeof(BrushesPage), "Brushes",
				"A brush enables you to paint an area, such as the background of a control, using different approaches."),

			new SectionModel(typeof(ClipPage), "Clip",
				"Defines the outline of the contents of an element."),

			new SectionModel(typeof(GesturesPage), "Gestures",
				"Use tap, pinch, pan, swipe, and drag and drop gestures on View instances."),

			new SectionModel(typeof(SemanticsPage), "Semantics",
				".NET MAUI allows accessibility values to be set on user interface elements by using Semantics values."),

			new SectionModel(typeof(TransformationsPage), "Transformations",
				"Apply scale transformations, rotation, etc. to a View."),
		};
	}
}