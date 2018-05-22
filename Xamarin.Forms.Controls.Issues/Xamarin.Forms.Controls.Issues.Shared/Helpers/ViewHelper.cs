using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xamarin.Forms.Controls.Issues.Helpers
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
				new OpenGLView { },
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
				new MasterDetailPage { Master = new Page { Title = "Master" }, Detail = new Page() },
				new NavigationPage(new Page()),
				new Page(),
				new ContentPage(),
				new CarouselPage(),
				new TabbedPage(),
				new TemplatedPage(),
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
