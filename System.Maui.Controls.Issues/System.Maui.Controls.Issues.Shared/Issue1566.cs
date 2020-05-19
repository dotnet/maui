using System;
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
	[Issue (IssueTracker.Github, 1566, "ListView reuse issue", PlatformAffected.Android)]
	public class Issue1566
		: ContentPage
	{
		bool _flag = true;

		ObservableCollection<Tuple<string, string>> _collection = new ObservableCollection<Tuple<string, string>>();

		void FillTheList()
		{
			_collection.Clear();

			for (int i = 0; i < 100; i++)
			{
				var item = new Tuple<string, string>(
					string.Format("{0} {0} {0} {0} {0} {0}", _flag ? i : 100-i),
					string.Format("---- i ----{0} {0} {0} {0} {0} {0}", _flag ? i : 100-i)
				);


				_collection.Add(item);
			}

			//flag = !flag;
		}

		public Issue1566()
		{
			SearchBar search = new SearchBar();
			search.SearchButtonPressed += (sender, e) => FillTheList();

			ListView list = new ListView() {
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
				HasUnevenRows = true, 
				ItemsSource = _collection,
				ItemTemplate = new DataTemplate(typeof(CellTemplate)) 
			};

			Label info = new Label() {
				Text = "Type something into searchbox and press search. Then swipe the list. Rows are mixed. It's important to have keyboard visible!!!"
			};

			StackLayout root = new StackLayout() {
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
				Children =
				{
					search,
					list,
					info,
				}
			};

			Content = root;
		}

		class CellTemplate : ViewCell
		{
			public CellTemplate()
			{
				Label cellLabel = new Label() {
					HorizontalOptions = LayoutOptions.FillAndExpand,
				};

				cellLabel.SetBinding(Label.TextProperty, new Binding("Item1", BindingMode.OneWay));

				StackLayout root = new StackLayout() {
					Children = 
					{
						cellLabel
					}
				};

				View = root;
			}
		}
	}
}
