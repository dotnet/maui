using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 60699, "ListView Bindings fire multiple times on Android SDK 24+", PlatformAffected.Android)]
	public class Bugzilla60699 : TestContentPage // or TestMasterDetailPage, etc ...
	{
		[Preserve(AllMembers = true)]
		public class DebugConverter : IValueConverter
		{
			public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
			{
				System.Diagnostics.Debug.WriteLine($"{DateTime.Now} [BindingFired] {value}");
				return value;
			}

			public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
				=> throw new NotImplementedException("Method not implemented");
		}

		[Preserve(AllMembers = true)]
		public class TimestampConverter : IValueConverter
		{
			public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
			{
				System.Diagnostics.Debug.WriteLine($"{DateTime.Now} [BindingFired] {value}");
				System.Diagnostics.Debug.WriteLine($"**DEBUG** {value}?t={DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond}");

				return value + $"?t={DateTime.Now.Ticks}";
				//return value;
			}

			public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
				=> throw new NotImplementedException("Method not implemented");
		}

		[Preserve(AllMembers = true)]
		public class MyTemplate : Grid
		{
			static IValueConverter TimestampConverter = new TimestampConverter();
			static IValueConverter DebugConverter = new DebugConverter();

			public MyTemplate()
			{
				var cstar = new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) };
				ColumnDefinitions.Add(cstar);
				ColumnDefinitions.Add(cstar);

				var rstar = new RowDefinition { Height = new GridLength(2, GridUnitType.Star) };
				RowDefinitions.Add(rstar);
				RowDefinitions.Add(rstar);

				var image = new Image();
				image.Aspect = Aspect.AspectFill;
				image.SetBinding(Image.SourceProperty, new Binding("Image", converter: TimestampConverter));

				var labelTop = new Label();
				labelTop.LineBreakMode = LineBreakMode.NoWrap;
				labelTop.FontSize = 16;
				labelTop.SetBinding(Label.TextProperty, new Binding("Text", converter: DebugConverter));

				var labelBottom = new Label();
				labelBottom.LineBreakMode = LineBreakMode.NoWrap;
				labelBottom.FontSize = 13;
				labelBottom.SetBinding(Label.TextProperty, new Binding("Description", converter: DebugConverter));

				Children.Add(image, 0, 1, 0, 2);
				Children.Add(labelTop, 1, 2, 0, 1);
				Children.Add(labelBottom, 1, 2, 1, 2);
			}
		}

		[Preserve(AllMembers = true)]
		public class MyViewCell : ViewCell
		{
			public MyViewCell()
			{
				View = new MyTemplate();
			}
		}

		[Preserve(AllMembers = true)]
		public class Item
		{
			public string Id { get; set; }
			public string Text { get; set; }
			public string Description { get; set; }
			public string Image { get; set; }
		}

		[Preserve(AllMembers = true)]
		public class MockDataStore
		{
			List<Item> items;

			public MockDataStore()
			{
				items = Enumerable.Range(0, 60)
					.Select(o => new Item
					{
						Id = Guid.NewGuid().ToString(),
						Text = $"{o} item",
						Description = "This is an item description.",
						Image = "https://dummyimage.com/100x100/000/ffffff"
					}).ToList();
			}

			public async Task<IEnumerable<Item>> GetItemsAsync(bool forceRefresh = false)
			{
				return await Task.FromResult(items);
			}
		}

		[Preserve(AllMembers = true)]
		public partial class ItemsViewModel : INotifyPropertyChanged
		{
			public MockDataStore DataStore => new MockDataStore();

			bool isBusy = false;
			public bool IsBusy
			{
				get { return isBusy; }
				set { SetProperty(ref isBusy, value); }
			}

			string title = string.Empty;
			public string Title
			{
				get { return title; }
				set { SetProperty(ref title, value); }
			}

			protected bool SetProperty<T>(ref T backingStore, T value,
				[CallerMemberName]string propertyName = "",
				Action onChanged = null)
			{
				if (EqualityComparer<T>.Default.Equals(backingStore, value))
					return false;

				backingStore = value;
				onChanged?.Invoke();
				OnPropertyChanged(propertyName);
				return true;
			}

			public event PropertyChangedEventHandler PropertyChanged;
			protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
			{
				var changed = PropertyChanged;
				if (changed == null)
					return;

				changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public partial class ItemsViewModel
		{
			public ObservableCollection<Item> Items { get; set; }
			public Command LoadItemsCommand { get; set; }

			public ItemsViewModel()
			{
				Title = "Browse";
				Items = new ObservableCollection<Item>();
				LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
			}

			async Task ExecuteLoadItemsCommand()
			{
				if (IsBusy)
					return;

				IsBusy = true;

				try
				{
					Items.Clear();
					var items = await DataStore.GetItemsAsync(true);
					foreach (var item in items)
					{
						Items.Add(item);
					}
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex);
				}
				finally
				{
					IsBusy = false;
				}
			}
		}

		ItemsViewModel viewModel;

		protected override void Init()
		{
			SetBinding(
				TitleProperty,
				new Binding("Title")
			 );

			BindingContext = viewModel = new ItemsViewModel();

			var listView = new ListView(ListViewCachingStrategy.RecycleElement)
			{
				VerticalOptions = LayoutOptions.FillAndExpand,
				HasUnevenRows = true,
				IsPullToRefreshEnabled = true,
				RowHeight = 200
			};

			listView.SetBinding(
				ListView.ItemsSourceProperty,
				new Binding("Items")
			);

			listView.SetBinding(
				ListView.RefreshCommandProperty,
				new Binding("LoadItemsCommand")
			);

			listView.SetBinding(
				ListView.IsRefreshingProperty,
				new Binding("IsBusy", mode: BindingMode.OneWay)
			);

			listView.ItemTemplate = new DataTemplate(typeof(MyViewCell));

			Content = listView;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			if (viewModel.Items.Count == 0)
				viewModel.LoadItemsCommand.Execute(null);
		}
	}
}