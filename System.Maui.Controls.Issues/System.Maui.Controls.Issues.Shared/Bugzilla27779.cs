using System;

using Xamarin.Forms.CustomAttributes;
using System.Collections.Generic;
using Xamarin.Forms.Controls.Issues;
using Xamarin.Forms.Internals;
#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 27779, "Xamarin.Forms.ReadOnlyListAdapter.IndexOf throws NotImplementedExcpetion ")]
	public class Bugzilla27779 : TestContentPage // or TestMasterDetailPage, etc ...
	{
		ListView _listview;
		IReadOnlyList<Person> _itemsSource;

		public class Source : IReadOnlyList<Person>
		{
			List<Person> _items;
			public Source ()
			{
				_items = new List<Person> ();

				for (int i = 0; i < 100; i++) {
					_items.Add (new Person ("Person #" + i));
				}

			}
			#region IEnumerable implementation
			public IEnumerator<Person> GetEnumerator ()
			{
				return _items.GetEnumerator ();
			}
			#endregion
			#region IEnumerable implementation
			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
			{
				return _items.GetEnumerator ();
			}
			#endregion
			#region IReadOnlyList implementation
			public Person this [int index] {
				get {
					return _items [index];
				}
			}
			#endregion
			#region IReadOnlyCollection implementation
			public int Count {
				get {
					return _items.Count;
				}
			}
			#endregion
			
		}
		protected override void Init ()
		{
			
			_itemsSource = new Source();

			_listview = new ListView {
				ItemsSource = _itemsSource
			};

			var btn = new Button { Text = "Set selected", AutomationId="btnSelect" };
			btn.Clicked+= (object sender, EventArgs e) => {
				_listview.SelectedItem = _itemsSource [0];
			};

			Content = new StackLayout { Children = { btn, _listview } };
		}

		protected override void OnAppearing ()
		{
			base.OnAppearing ();
		}

	}
}
