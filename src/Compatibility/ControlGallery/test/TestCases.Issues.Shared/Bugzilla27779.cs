using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 27779, "Microsoft.Maui.Controls.ReadOnlyListAdapter.IndexOf throws NotImplementedExcpetion ")]
	public class Bugzilla27779 : TestContentPage // or TestFlyoutPage, etc ...
	{
		ListView _listview;
		IReadOnlyList<Person> _itemsSource;

		public class Source : IReadOnlyList<Person>
		{
			List<Person> _items;
			public Source()
			{
				_items = new List<Person>();

				for (int i = 0; i < 100; i++)
				{
					_items.Add(new Person("Person #" + i));
				}

			}
			#region IEnumerable implementation
			public IEnumerator<Person> GetEnumerator()
			{
				return _items.GetEnumerator();
			}
			#endregion
			#region IEnumerable implementation
			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return _items.GetEnumerator();
			}
			#endregion
			#region IReadOnlyList implementation
			public Person this[int index]
			{
				get
				{
					return _items[index];
				}
			}
			#endregion
			#region IReadOnlyCollection implementation
			public int Count
			{
				get
				{
					return _items.Count;
				}
			}
			#endregion

		}
		protected override void Init()
		{

			_itemsSource = new Source();

			_listview = new ListView
			{
				ItemsSource = _itemsSource
			};

			var btn = new Button { Text = "Set selected", AutomationId = "btnSelect" };
			btn.Clicked += (object sender, EventArgs e) =>
			{
				_listview.SelectedItem = _itemsSource[0];
			};

			Content = new StackLayout { Children = { btn, _listview } };
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
		}

	}
}
