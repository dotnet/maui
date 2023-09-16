using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(UITestCategories.CollectionView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 6889, "Labels disappearing in CollectionView", PlatformAffected.Android)]
	public class Issue6889 : TestNavigationPage
	{
		protected override void Init()
		{
#if APP
			PushAsync(CreateRoot());
#endif
		}

		public ContentPage CreateRoot()
		{
			var page = new ContentPage { Title = "Issue6889" };

			var layout = new StackLayout();

			var instructions = new Label
			{
				Text = "Scroll the CollectionView below up and down quickly several times."
				+ " If any rows of labels disappear, this test has failed"
			};

			layout.Children.Add(instructions);

			var cv = new CollectionView();

			var template = new DataTemplate(() =>
			{
				var grid = new Grid
				{
					ColumnDefinitions = new ColumnDefinitionCollection
					{
						new ColumnDefinition { Width = GridLength.Auto },
						new ColumnDefinition { Width = GridLength.Auto }
					}
				};

				var label1 = new Label { HorizontalOptions = LayoutOptions.Start };
				label1.SetBinding(Label.TextProperty, new Binding("Text1"));
				grid.Children.Add(label1);
				Grid.SetColumn(label1, 0);

				var label2 = new Label { HorizontalOptions = LayoutOptions.StartAndExpand };
				label2.SetBinding(Label.TextProperty, new Binding("Text2"));
				grid.Children.Add(label2);
				Grid.SetColumn(label2, 1);

				return grid;
			});

			cv.ItemTemplate = template;
			cv.SetBinding(ItemsView.ItemsSourceProperty, new Binding("SampleList"));

			layout.Children.Add(cv);

			page.Content = layout;

			page.BindingContext = new _6889MainViewModel();

			return page;
		}
	}

	[Preserve(AllMembers = true)]
	public class _6889TextModel
	{
		public string Text1 { get; set; }
		public string Text2 { get; set; }
	}

	[Preserve(AllMembers = true)]
	public class _6889MainViewModel
	{
		public _6889MainViewModel()
		{
			SampleList = new ObservableCollection<_6889TextModel>
			{
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
				new _6889TextModel { Text1="Text1",Text2="Text2"},
			};
		}

		public ObservableCollection<_6889TextModel> SampleList { get; set; }
	}
}
