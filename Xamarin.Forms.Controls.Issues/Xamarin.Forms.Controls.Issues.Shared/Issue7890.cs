using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using System.Threading.Tasks;
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(UITestCategories.ListView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7890, "TemplatedItemsList incorrect grouped collection range removal", PlatformAffected.All)]
	public class Issue7890 : TestContentPage
	{
		const int Count = 10;
		const int RemoveFrom = 1;
		const int RemoveCount = 5;
		protected override void Init()
		{
			var items = Enumerable.Range(0, Count).Select(x => new DataGroup(x));
			var source = new ObservableCollectionFast<DataGroup>(items);

			var listView = new ListView()
			{
				IsGroupingEnabled = true,
				ItemsSource = source,
				GroupDisplayBinding = new Binding("Text"),
			};

			Content = new ScrollView()
			{
				Content = new StackLayout()
				{
					Children =
					{
						new Label() { Text = "Button click should remove items from 1 to 5"},
						new Button()
						{
							AutomationId = "RemoveBtn",
							Text = "remove",
							Command = new Command(() =>
							{
								source.RemoveRange(RemoveFrom, RemoveCount);
							})
						},
						listView
					}
				}
			};
		}

#if UITEST
		[Test]
		[NUnit.Framework.Category(Core.UITests.UITestCategories.UwpIgnore)]
		public void TestCorrectListItemsRemoved()
		{
			RunningApp.WaitForElement(q => q.Button("RemoveBtn"));
			RunningApp.Tap(q => q.Button("RemoveBtn"));
			var toRemove = Enumerable.Range(RemoveFrom, RemoveCount).ToList();
			foreach (var c in Enumerable.Range(0, Count))
			{
				if (toRemove.Contains(c))
					Assert.IsNull(RunningApp.Query(q=>q.Marked(c.ToString())).FirstOrDefault());
				else
					Assert.IsNotNull(RunningApp.Query(q => q.Marked(c.ToString())).FirstOrDefault());
			}
		}
#endif

		public class DataGroup : List<Data>
		{
			public DataGroup(int num)
			{
				Text = $"Group {num}";
				Add(new Data() { Text = num });
			}
			public string Text { get; set; }
		}
		public class Data
		{
			public int Text { get; set; }

			public override string ToString()
			{
				return Text.ToString();
			}
		}
	}

	public class ObservableCollectionFast<T> : ObservableCollection<T>
	{
		public ObservableCollectionFast(IEnumerable<T> collection) : base(collection) { }

		public void RemoveRange(int index, int count)
		{
			var removed = new List<T>(count);
			for (var i = index + count - 1; i >= index; i--)
			{
				removed.Add(Items[i]);
				Items.Remove(Items[i]);
			}
			this.OnPropertyChanged(new PropertyChangedEventArgs("Count"));
			this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
			this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed, index));
		}
	}
}
