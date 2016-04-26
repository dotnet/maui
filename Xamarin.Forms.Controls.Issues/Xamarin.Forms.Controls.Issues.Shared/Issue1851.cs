using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.TestCasesPages
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 1851, "ObservableCollection in ListView gets Index out of range when removing item", PlatformAffected.Android)]
	public class Issue1851 : ContentPage
	{
		public Issue1851 ()
		{
			var grouping = new Grouping<string, string>("number", new List<string> { "1", "2", "3", "4", "5", "6", "7", "8", "9" });
			var groupings = new ObservableCollection<Grouping<string, string>>
			{
				new Grouping<string, string>("letters", new List<string> {"a", "b", "c", "d", "e", "f", "g", "h", "i"}),
				new Grouping<string, string>("colours", new List<string> {"red", "green", "blue", "white", "orange", "purple", "grey", "mauve", "pink"}),
				grouping,
			};

			var listview = new ListView
			{
				HasUnevenRows = true,
				IsGroupingEnabled = true,
				ItemsSource = groupings,
				ItemTemplate = new DataTemplate(typeof(CellTemplate)),
				GroupDisplayBinding = new Binding("Key")
			};
			var groupbtn = new Button() { Text = "add/remove group" };
			bool group = true;
			groupbtn.Clicked += (sender, args) =>
			{
				listview.GroupShortNameBinding = new Binding ("Key");
				if (group)
				{
					group = false;

					// ***** Crashes here
					groupings.Remove(grouping);
				}
				else
				{
					group = true;
					groupings.Add(grouping);
				}
			};

			Content = new StackLayout
			{
				Children =
				{
					groupbtn,
					listview,
				}
			};
		}
	}

	public class CellTemplate : ViewCell
	{
		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();

			var text = BindingContext as string;
			if (text == null)
				return;

			View = new Label { Text = text };
		}
	}

	public class Grouping<TKey, TElement> : ObservableCollection<TElement>
	{
		public Grouping(TKey key, IEnumerable<TElement> items)
		{
			Key = key;
			foreach (var item in items)
				Items.Add(item);
		}

		public TKey Key { get; private set; }


	}
}
	