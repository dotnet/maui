using System.Collections.Generic;
using Maui.Controls.Sample.Models;
using Maui.Controls.Sample.Pages;
using Maui.Controls.Sample.ViewModels.Base;

namespace Maui.Controls.Sample.ViewModels
{
	public class UserInterfaceViewModel : BaseGalleryViewModel
	{
		protected override IEnumerable<SectionModel> CreateItems() => new[]
		{
			new SectionModel(typeof(AnimationsPage), "Animations",
				"Animate your UI!"),

			new SectionModel(typeof(BehaviorsPage), "Behaviors",
				"Behaviors lets you add functionality to user interface controls without having to subclass them."),

			new SectionModel(typeof(FontsPage), "Fonts",
				"Customize the font type with different sizes, attributes, etc..."),

			new SectionModel(typeof(StylesPage), "Styles",
				"Define the visual elements appearance."),

			new SectionModel(typeof(StateTriggersPage), "State Triggers",
				"Use state triggers to automatically change visual states based on device properties like orientation, rotation, size, and platform."),

			new SectionModel(typeof(TriggersPage), "Triggers",
				"Triggers allow you to express actions declaratively in XAML that change the appearance of controls based on events or property changes. "),

			new SectionModel(typeof(VisualStatesPage), "VisualStates",
				"Use the Visual State Manager to make changes to XAML elements based on visual states set from code."),
		};
	}
}