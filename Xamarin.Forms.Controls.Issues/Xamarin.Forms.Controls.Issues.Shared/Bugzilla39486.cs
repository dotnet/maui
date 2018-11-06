using System;
using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 39486, "HasUnevenRows=true ignored in Forms iOS project in TableView with CustomCell; row heights are not auto-sized")]
	public class Bugzilla39486 : TestContentPage // or TestMasterDetailPage, etc ...
	{
		protected override void Init()
		{
			TextCell cell1 = new TextCell
			{
				Text = "ListView: TextCell"
			};
			cell1.Tapped += async delegate (object sender, EventArgs e)
			{
				await Navigation.PushAsync(new ListViewTextCellPage());
			};
			TextCell cell2 = new TextCell
			{
				Text = "ListView: CustomCell"
			};
			cell2.Tapped += async delegate (object sender, EventArgs e)
			{
				await Navigation.PushAsync(new ListViewCustomCellPage(ListViewCachingStrategy.RetainElement));
			};
			TextCell cell3 = new TextCell
			{
				Text = "TableView: TextCell"
			};
			cell3.Tapped += async delegate (object sender, EventArgs e)
			{
				await Navigation.PushAsync(new TableViewTextCellPage());
			};
			TextCell cell4 = new TextCell
			{
				Text = "TableView: CustomCell"
			};
			cell4.Tapped += async delegate (object sender, EventArgs e)
			{
				await Navigation.PushAsync(new TableViewCustomCellPage());
			};

			TextCell cell5 = new TextCell
			{
				Text = "ListView: CustomCell RecycleElement"
			};
			cell5.Tapped += async delegate (object sender, EventArgs e)
			{
				await Navigation.PushAsync(new ListViewCustomCellPage(ListViewCachingStrategy.RecycleElement));
			};
			TableView tableV = new TableView
			{
				Root = new TableRoot {
					new TableSection {
						cell1,
						cell2,
						cell3,
						cell4,
						cell5
					}
				}
			};
			Content = tableV;
		}

		class TableViewTextCellPage : ContentPage
		{
			public TableViewTextCellPage()
			{
				TableSection ts = new TableSection();
				TableView tableV = new TableView
				{
					HasUnevenRows = true,
					Root = new TableRoot {
					ts
				}
				};
				foreach (CustomData data in GetData())
				{
					ts.Add(new TextCell
					{
						Text = data.Title,
						Detail = data.SubTitle
					});
				}
				Content = tableV;
			}
		}

		class TableViewCustomCellPage : ContentPage
		{
			public TableViewCustomCellPage()
			{
				TableSection ts = new TableSection();
				TableView tableV = new TableView
				{
					HasUnevenRows = true,
					Root = new TableRoot {
					ts
				}
				};
				foreach (CustomData data in GetData())
				{
					ts.Add(new CustomCell
					{
						Text = data.Title,
						Detail = data.SubTitle
					});
				}
				Content = tableV;
			}
		}

		class ListViewTextCellPage : ContentPage
		{
			public ListViewTextCellPage()
			{
				DataTemplate it = new DataTemplate(typeof(TextCell));
				it.SetBinding(TextCell.TextProperty, "Title");
				it.SetBinding(TextCell.DetailProperty, "SubTitle");
				ListView listV = new ListView
				{
					HasUnevenRows = true,
					ItemTemplate = it,
					ItemsSource = GetData()
				};
				Content = listV;
			}
		}

		class ListViewCustomCellPage : ContentPage
		{
			public ListViewCustomCellPage(ListViewCachingStrategy strategy)
			{
				DataTemplate it = new DataTemplate(typeof(CustomCell));
				it.SetBinding(CustomCell.TextProperty, "Title");
				it.SetBinding(CustomCell.DetailProperty, "SubTitle");
				ListView listV = new ListView(strategy)
				{
					HasUnevenRows = true,
					ItemTemplate = it,
					ItemsSource = GetData()
				};
				Content = listV;
			}
		}

		class CustomCell : ViewCell
		{
			public static BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(CustomCell), default(string), propertyChanged:
			(bindable, oldValue, newValue) =>
			{
				var view = (CustomCell)bindable;
				view.Text = newValue.ToString();
			});
			public static BindableProperty DetailProperty = BindableProperty.Create(nameof(Detail), typeof(string), typeof(CustomCell), default(string), propertyChanged:
			(bindable, oldValue, newValue) =>
			{
				var view = (CustomCell)bindable;
				view.Detail = newValue.ToString();
			});

			public string Text
			{
				get { return (string)GetValue(TextProperty); }
				set
				{
					SetValue(TextProperty, value);
					_textL.Text = value;
				}
			}
			public string Detail
			{
				get { return (string)GetValue(DetailProperty); }
				set
				{
					SetValue(DetailProperty, value);
					_detailL.Text = value;
				}
			}

			private Label _textL = null;
			private Label _detailL = null;

			public CustomCell()
			{
				_textL = new Label();
				_detailL = new Label
				{
					FontSize = _textL.FontSize * 0.75f
				};
				StackLayout mainSL = new StackLayout
				{
					Orientation = StackOrientation.Vertical,
					Padding = new Thickness(15, 10),
					Spacing = 5,
					Children = {
					_textL, _detailL
				}
				};
				View = mainSL;
			}
		}

		static List<CustomData> GetData()
		{
			List<CustomData> retVal = new List<CustomData>(new CustomData[] {
				new CustomData {
					Title = "One",
					SubTitle = "Short"
				},
				new CustomData {
					Title = "Two",
					SubTitle = "Now is the time for all good men to come to the aid of their country.  Now is the time for all good men to come to the aid of their country.  Now is the time for all good men to come to the aid of their country.  Now is the time for all good men to come to the aid of their country."
				},
				new CustomData {
					Title = "Three",
					SubTitle = "Short"
				},
				new CustomData {
					Title = "Four",
					SubTitle = "Now is the time for all good men to come to the aid of their country.  Now is the time for all good men to come to the aid of their country.  Now is the time for all good men to come to the aid of their country.  Now is the time for all good men to come to the aid of their country."
				}
			});
			return (retVal);
		}

		class CustomData
		{
			public string Title { get; set; }
			public string SubTitle { get; set; }
		}
	}
}
