using System.Collections;
using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 1590, "ListView.IsGroupingEnabled results ins ArguementOutOfRangeException",
		PlatformAffected.Android)]
	public class Issue1590 : TestContentPage
	{
		ListView _listView;

		protected override void Init()
		{
			var vm = new RootPageViewModel();
			Content = BuildListView(vm);
		}

		StackLayout BuildListView(RootPageViewModel viewModel)
		{
			var headerTemplate = new DataTemplate(typeof(ModuleMediaListHeaderTemplate));
			headerTemplate.CreateContent();

			var itemTemplate = new DataTemplate(typeof(ModuleMediaListItemTemplate));
			itemTemplate.CreateContent();

			_listView = new ListView
			{
				ItemsSource = viewModel.MediaSections,
				IsGroupingEnabled = true,
				GroupDisplayBinding = new Binding("SectionName"),
				HasUnevenRows = false,
				GroupHeaderTemplate = headerTemplate,
				ItemTemplate = itemTemplate
			};

			return new StackLayout { Children = { _listView } };
		}
	}


	public class RootPageViewModel
	{
		public IEnumerable MediaSections
		{
			get
			{
				var titles = new[] { "First", "Second", "Third", "Fourth", "Fifth" };

				return titles.Select(section => new MediaListSection(section)
				{
					new FooViewModel {Title = "Foo", Description = "description for foo"},
					new FooViewModel {Title = "Bar", Description = "description for bar"},
					new FooViewModel {Title = "Baz", Description = "description for baz"},
					new FooViewModel {Title = "Fiz", Description = "description for fiz"},
					new FooViewModel {Title = "Buz", Description = "description for buz"},
				}).ToList();
			}
		}
	}


	public class MediaListSection : ObservableCollection<FooViewModel>
	{
		public string SectionName { get; private set; }

		public MediaListSection(string sectionName)
		{
			SectionName = sectionName;
		}
	}


	public class FooViewModel
	{
		public string Title { get; set; }
		public string Description { get; set; }
	}


	public class ModuleMediaListItemTemplate : ViewCell
	{
		public ModuleMediaListItemTemplate()
		{
			var title = new Label { VerticalTextAlignment = TextAlignment.Center };
			title.SetBinding(Label.TextProperty, new Binding("Title", BindingMode.OneWay));

			var description = new Label { VerticalTextAlignment = TextAlignment.Center };
			description.SetBinding(Label.TextProperty, new Binding("Description", BindingMode.OneWay));

			View = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				Padding = new Thickness(8),
				Children = { title, description }
			};
		}
	}


	public class ModuleMediaListHeaderTemplate : ViewCell
	{
		public ModuleMediaListHeaderTemplate()
		{
			var title = new Label { TextColor = Colors.White, VerticalTextAlignment = TextAlignment.Center };
			title.SetBinding(Label.TextProperty, new Binding("SectionName", BindingMode.OneWay));

			View = new StackLayout
			{
				Padding = new Thickness(8, 0),
				BackgroundColor = Color.FromArgb("#999999"),
				Orientation = StackOrientation.Horizontal,
				Children = { title },
			};
		}
	}
}
