using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 11853, "[Bug][iOS] Concurrent issue leading to crash in SemaphoreSlim.Release in ObservableItemsSource",
		PlatformAffected.iOS)]
	public partial class Issue11853 : TestContentPage
	{
		const string Run = "Run";

		protected override void Init() { }

		public Issue11853()
		{
			InitializeComponent();

			BindingContext = new _11853ViewModel();
		}

		public class _11853Item
		{
			public string Text { get; set; }
		}

		public class _11853ViewModel : INotifyPropertyChanged
		{
			private bool isListVisible;
			public event PropertyChangedEventHandler PropertyChanged;

			public bool IsListVisible
			{
				get => isListVisible;
				set
				{
					isListVisible = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsListVisible)));
				}
			}

			public ObservableCollection<_11853Item> Items { get; }
			public ICommand TestCommand { get; }

			public _11853ViewModel()
			{
				Items = new ObservableCollection<_11853Item>();
				TestCommand = new Command(async () =>
				{
					var items = CreateItems(10, 0).ToList().First();
					var items2 = CreateItems(10, 0).ToList().Skip(1).First();

					int iterations = 1000; //10000;

					for (var i = 0; i < iterations; i++)
					{
						await Task.Delay(1);
						Items.Add(items);

						await Task.Delay(2);
						Items.Add(items2);
						await Task.Delay(2);

						Items.Clear();
						await Task.Delay(2);

						Items.Add(items);
						Items.Add(items2);
						await Task.Delay(2);
					}

				});
			}

			IEnumerable<_11853Item> CreateItems(int count, int batch)
			{
				var i = 0;
				while (count-- > 0)
					yield return new _11853Item { Text = $"Item {i++} Batch {batch}" };
			}
		}
	}
}