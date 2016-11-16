using System;
using Xamarin.Forms.CustomAttributes;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 30651, "ListView jumps around while scrolling after items are added to its source")]
	public class Bugzilla30651: TestContentPage
	{
		ListViewModel _viewModel;
		protected override void Init ()
		{
			_viewModel = new ListViewModel();
			BindingContext = _viewModel;
			var lv = new ListView ();
			lv.SetBinding (ListView.ItemsSourceProperty, new Binding ("Items"));
			lv.SeparatorVisibility = SeparatorVisibility.None;
			lv.HasUnevenRows = true;
			lv.ItemAppearing+= (object sender, ItemVisibilityEventArgs e) => {
				_viewModel.OnItemAppearing(e.Item.ToString());
			};
			lv.ItemTemplate = new DataTemplate (typeof(TestCell));
			Content = lv;
		}

		public class TestCell : ViewCell {
			Label _myLabel;
			public TestCell ()
			{
				View = _myLabel = new Label();
			}

			protected override void OnBindingContextChanged()
			{
				if (BindingContext == null)
					return;

				var i = BindingContext as string;
				_myLabel.Text = i;
				Height = 100;

				base.OnBindingContextChanged();
			}
		}


		public class ListViewModel : ViewModelBase
		{
			ObservableCollection<string> _items;
			int _counter = 0;

			public ObservableCollection<string> Items
			{
				get { return _items; }
				set { _items = value; OnPropertyChanged ();}
			}

			public ListViewModel()
			{
				Items = new ObservableCollection<string>();
				AddMoreData();
			}

			public void OnItemAppearing(string s)
			{
				if (Items.Last() == s)
					AddMoreData();
			}

			void AddMoreData()
			{
				_counter++;
				Items.Add(_counter.ToString());
				_counter++;
				Items.Add(_counter.ToString());
				_counter++;
				Items.Add(_counter.ToString());
				_counter++;
				Items.Add(_counter.ToString());
				_counter++;
				Items.Add(_counter.ToString());
				_counter++;
				Items.Add(_counter.ToString());
				_counter++;
				Items.Add(_counter.ToString());
				_counter++;
				Items.Add(_counter.ToString());
				_counter++;
				Items.Add(_counter.ToString());
				_counter++;
				Items.Add(_counter.ToString());
				_counter++;
				Items.Add(_counter.ToString());
				_counter++;
				Items.Add(_counter.ToString());
			}
		}
	}
}
