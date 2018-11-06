using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 1590, "ListView.IsGroupingEnabled results ins ArguementOutOfRangeException", PlatformAffected.Android)]
	public class Issue1590 : ContentPage
	{
		ListView _listView;

		public Issue1590()
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

	[Preserve (AllMembers=true)]
	public class RootPageViewModel
	{
		public IEnumerable MediaSections
		{
			get
			{
				var titles = new[] {"First", "Second", "Third", "Forth", "Fifth"};

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

	[Preserve (AllMembers=true)]
	public class MediaListSection : ObservableCollection<FooViewModel>
	{
		public string SectionName { get; private set; }

		public MediaListSection(string sectionName)
		{
			SectionName = sectionName;
		}
	}

	[Preserve (AllMembers=true)]
	public class FooViewModel
	{
		public string Title { get; set; }
		public string Description { get; set; }
	}

	[Preserve (AllMembers=true)]
	public class ModuleMediaListItemTemplate : ViewCell
	{
		public ModuleMediaListItemTemplate()
		{
#pragma warning disable 618
			var title = new Label { YAlign = TextAlignment.Center };
#pragma warning restore 618
			title.SetBinding(Label.TextProperty, new Binding("Title", BindingMode.OneWay));

#pragma warning disable 618
			var description = new Label { YAlign = TextAlignment.Center };
#pragma warning restore 618
			description.SetBinding(Label.TextProperty, new Binding("Description", BindingMode.OneWay));

			View = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				Padding = new Thickness(8),
				Children = { title, description } 
			};
		}
	}

	[Preserve (AllMembers=true)]
	public class ModuleMediaListHeaderTemplate : ViewCell
	{
		public ModuleMediaListHeaderTemplate()
		{
#pragma warning disable 618
			var title = new Label { TextColor = Color.White, YAlign = TextAlignment.Center };
#pragma warning restore 618
			title.SetBinding(Label.TextProperty, new Binding("SectionName", BindingMode.OneWay));

			View = new StackLayout
			{
				Padding = new Thickness(8, 0),
				BackgroundColor = Color.FromHex("#999999"),
				Orientation = StackOrientation.Horizontal,
				Children = { title },
			};
		}
	}
}
