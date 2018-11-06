using System;
using System.Collections.ObjectModel;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 1680, "Disposed object updating ListView ItemSource", PlatformAffected.Android)]
	public class Issue1680 : ContentPage
	{
		XamarinListViewBug _page1 = new XamarinListViewBug();

		public Issue1680()
		{
			var button1 = new Button { Text = "PAGE1" };

			button1.Clicked += (sender, e) => Navigation.PushAsync (_page1);

			var root = new StackLayout {
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
				Children = {
					button1,
				}
			};

			Content = root;
		}

		public class XamarinListViewBug : MasterDetailPage
		{
			bool _flag;

			readonly ObservableCollection<Tuple<string, string>> _collection = new ObservableCollection<Tuple<string, string>>();

			void FillTheList()
			{
				_collection.Clear();

				for (int i = 0; i < 100; i++) {
					var item = new Tuple<string, string> (
						string.Format ("{0} {0} {0} {0} {0} {0}", _flag ? i : 100 - i),
						string.Format ("---- i ----{0} {0} {0} {0} {0} {0}", _flag ? i : 100 - i)
						);


					_collection.Add (item);
				}

				_flag = !_flag;
			}

			public XamarinListViewBug()
			{
				Title = "XamarinListViewBug";

				SearchBar search = new SearchBar();
				search.SearchButtonPressed += (sender, e) => FillTheList();

				ListView list = new ListView {
					HorizontalOptions = LayoutOptions.FillAndExpand,
					VerticalOptions = LayoutOptions.FillAndExpand,
					HasUnevenRows = true,
					ItemsSource = _collection,
					ItemTemplate = new DataTemplate (typeof (CellTemplate))
				};

				StackLayout root = new StackLayout {
					HorizontalOptions = LayoutOptions.FillAndExpand,
					VerticalOptions = LayoutOptions.FillAndExpand,
					Children = {
						search,
						list,
					}
				};

				Master = new ContentPage { Title = "Master" };
				Detail = new ContentPage {
					Title = "Detail",
					Content = root
				};
			}

			class CellTemplate : ViewCell
			{
				public CellTemplate()
				{
					Label cellLabel = new Label{
						HorizontalOptions = LayoutOptions.FillAndExpand,
					};

					cellLabel.SetBinding (Label.TextProperty, new Binding ("Item1", BindingMode.OneWay));

					StackLayout root = new StackLayout {
						Children = {
							cellLabel
						}
					};

					View = root;
				}
			}
		}
	}
}
