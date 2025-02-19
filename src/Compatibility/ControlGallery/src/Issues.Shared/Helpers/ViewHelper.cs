using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Maui.Controls.ControlGallery.Issues.Helpers
{
	public static class ViewHelper
	{
		public static List<View> GetAllViews()
		{
			var controls = new List<View>
			{
				new ActivityIndicator { },
				new BoxView { },
				new Button { },
				new DatePicker { },
				new Editor { },
				new Entry { },
				new Image { },
				new Label { },
				new ListView { ItemsSource = Enumerable.Range(0,10), ItemTemplate = new DataTemplate(() => new ViewCell{ View = new View() }) },
				new ListView { ItemsSource = Enumerable.Range(0,10), ItemTemplate = new DataTemplate(typeof(TextCell)) },
				new ListView { ItemsSource = Enumerable.Range(0,10), ItemTemplate = new DataTemplate(typeof(ImageCell)) },
				new ListView { ItemsSource = Enumerable.Range(0,10), ItemTemplate = new DataTemplate(typeof(EntryCell)) },
				new ListView { ItemsSource = Enumerable.Range(0,10), ItemTemplate = new DataTemplate(typeof(SwitchCell)) },
				new Picker { },
				new ProgressBar { },
				new SearchBar { },
				new Slider { },
				new Stepper { },
				new Switch { },
				new TableView { },
				new TimePicker { },
				GetNativeView()
			};

			return controls;
		}

		public static List<Page> GetAllPages()
		{
			var controls = new List<Page>
			{
				// TODO MAUI: These can come back when we have nested navigation
				//new FlyoutPage { Flyout = new ContentPage { Title = "Flyout" }, Detail = new ContentPage() },
				//new NavigationPage(new ContentPage()),
				// TODO MAUI: These can come back with we get a defaultrenderer
				//new FlyoutPage { Flyout = new Page { Title = "Flyout" }, Detail = new Page() },
				//new Page(),
				//new TemplatedPage(),
				new ContentPage(),
				new TabbedPage(),
			};

			return controls;
		}

		public static View GetNativeView()
		{
			View view = null;
			view = DependencyService.Get<ISampleNativeControl>().View;
			return view;
		}
	}
}
