using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1975, "[iOS] ListView throws NRE when grouping enabled and data changed",
		PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.ListView)]
#endif
	public class Issue1975 : TestNavigationPage
	{
		protected override void Init()
		{
			PushAsync(CreateRootPage());
		}

		const string Success = "If you can see this, the test has passed";
		const string Go = "Go";

		ContentPage CreateRootPage()
		{
			var button = new Button { Text = Go };

			button.Clicked += (sender, args) => Application.Current.MainPage = ModifyDataPage();

			var lv = new ListView();

			lv.SetBinding(ListView.ItemsSourceProperty, new Binding("Items"));
			lv.IsGroupingEnabled = true;
			lv.GroupDisplayBinding = new Binding("Description");
			lv.GroupShortNameBinding = new Binding("ShortName");

			lv.ItemTemplate = new DataTemplate(() =>
			{
				var textCell = new TextCell();
				textCell.SetBinding(TextCell.TextProperty, new Binding("Text"));
				return textCell;
			});

			var layout = new StackLayout();
			layout.Children.Add(button);
			layout.Children.Add(lv);

			return new ContentPage { Content = layout, BindingContext = DataSample.Instance };
		}

		ContentPage ModifyDataPage()
		{
			var contentPage = new ContentPage { Content = new Label { Text = Success, Margin = 100 } };

			contentPage.Appearing += (sender, args) =>
				DataSample.Instance.Items.Add(new Item("C") { new SubItem("Cherry"), new SubItem("Cranberry") });

			return contentPage;
		}

		[Preserve(AllMembers = true)]
		class DataSample
		{
			static readonly object _lockObject = new object();

			static volatile DataSample _instance;

			public static DataSample Instance
			{
				get
				{
					if (_instance != null)
					{
						return _instance;
					}

					lock (_lockObject)
					{
						if (_instance == null)
						{
							_instance = new DataSample();
						}
					}

					return _instance;
				}
			}

			DataSample()
			{
				Items = new ObservableCollection<Item>
				{
					new Item("A")
					{
						new SubItem("Apple"),
						new SubItem("Avocado")
					},
					new Item("B")
					{
						new SubItem("Banana"),
						new SubItem("Blackberry")
					}
				};
			}

			public ObservableCollection<Item> Items { get; }
		}

		[Preserve(AllMembers = true)]
		class Item : ObservableCollection<SubItem>
		{
			public string ShortName { get; set; }
			public string Description { get; set; }

			public Item(string shortName)
			{
				ShortName = shortName;
				Description = shortName;
			}
		}

		[Preserve(AllMembers = true)]
		class SubItem
		{
			public string Text { get; set; }

			public SubItem(string text)
			{
				Text = text;
			}
		}

#if UITEST
		[Test]
		public void UpdatingSourceOfDisposedListViewDoesNotCrash()
		{
			RunningApp.Tap(Go);
			RunningApp.WaitForElement(Success);
		}
#endif
	}
}
