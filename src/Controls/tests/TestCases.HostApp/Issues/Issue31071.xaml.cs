using System.Collections;
using System.Windows.Input;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 31071, "CollectionView2 (CollectionViewHandler2) much more slow than CV1 on updating columns count at runtime",
		PlatformAffected.iOS)]
	public partial class Issue31071 : ContentPage
	{
		public Issue31071()
		{
			SetGroupedCommand = new Command(() =>
			{
				var now = DateTime.Now;
				IsGrouped = true;
				CollectionItems = Enumerable.Range(0, 1000)
					.GroupBy(i => i / 20).ToList();
				SetActionTime(now);
			});
			SetUnGroupedCommand = new Command(() =>
			{
				var now = DateTime.Now;
				IsGrouped = false;
				CollectionItems = Enumerable.Range(0, 1000).ToList();
				SetActionTime(now);
			});
			SetColumnsCommand = new Command<int>(columns =>
			{
				var now = DateTime.Now;
				CVColumns = columns;
				SetActionTime(now);
			});

			void SetActionTime(DateTime now) =>
				ActionTime = (DateTime.Now - now).TotalSeconds;

			BindingContext = this;
			InitializeComponent();

			MessagingCenter.Subscribe<Issue31071Item, int>(
				this, string.Empty, (s, i) =>
					Dispatcher.Dispatch(() => CollViewItemsCount += i));
		}

		public ICommand SetGroupedCommand { get; }
		public ICommand SetUnGroupedCommand { get; }
		public ICommand SetColumnsCommand { get; }

		public bool IsGrouped
		{
			get => isGrouped;
			set
			{
				isGrouped = value;
				OnPropertyChanged();
			}
		}

		private bool isGrouped;

		public IEnumerable CollectionItems
		{
			get => collectionItems;
			set
			{
				collectionItems = value;
				OnPropertyChanged();
			}
		}

		IEnumerable collectionItems;

		public int CVColumns
		{
			get => cvColumns;
			set
			{
				cvColumns = value;
				OnPropertyChanged();
			}
		}

		int cvColumns = 4;

		public int CollViewItemsCount
		{
			get => collViewItemsCount;
			set
			{
				collViewItemsCount = value;
				OnPropertyChanged();
			}
		}

		int collViewItemsCount;

		public double ActionTime
		{
			get => actionTime;
			set
			{
				actionTime = value;
				OnPropertyChanged();
			}
		}

		double actionTime;

		public double TotalMemory =>
			GC.GetTotalMemory(false) / 1024.0 / 1024;

		void OnScrolled(object sender, ItemsViewScrolledEventArgs e) =>
			OnPropertyChanged(nameof(TotalMemory));
	}
}