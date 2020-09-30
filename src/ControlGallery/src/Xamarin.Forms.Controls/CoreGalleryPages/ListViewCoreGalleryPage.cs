using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

namespace Xamarin.Forms.Controls
{
	[Preserve(AllMembers = true)]
	internal class ListViewCoreGalleryPage : CoreGalleryPage<ListView>
	{
		internal class Employee : INotifyPropertyChanged
		{
			string _name;
			public string Name
			{
				get { return _name; }
				set
				{
					if (value != null && value != _name)
					{
						_name = value;
						OnPropertyChanged();
					}
				}
			}

			TimeSpan _daysWorked;
			public TimeSpan DaysWorked
			{
				get { return _daysWorked; }
				set
				{
					if (value != null && value != _daysWorked)
					{
						_daysWorked = value;
						OnPropertyChanged();
					}
				}
			}

			int _rowHeight;
			public int RowHeight
			{
				get { return _rowHeight; }
				set
				{
					if (value != _rowHeight)
					{
						_rowHeight = value;
						OnPropertyChanged();
					}
				}
			}

			public Employee(string name, TimeSpan daysWorked, int rowHeight)
			{
				_name = name;
				_daysWorked = daysWorked;
				_rowHeight = rowHeight;
			}

			public event PropertyChangedEventHandler PropertyChanged;

			protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
			{
				PropertyChangedEventHandler handler = PropertyChanged;
				if (handler != null)
					handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		[Preserve(AllMembers = true)]
		internal class Grouping<K, T> : ObservableCollection<T>
		{
			public K Key { get; private set; }

			public Grouping(K key, IEnumerable<T> items)
			{
				Key = key;
				foreach (T item in items)
				{
					Items.Add(item);
				}
			}
		}

		[Preserve(AllMembers = true)]
		public class HeaderCell : ViewCell
		{
			public HeaderCell()
			{
				Height = 30;
				var title = new Label
				{
					HeightRequest = 30,
					BackgroundColor = Color.Navy,
					TextColor = Color.White
				};

				title.SetBinding(Label.TextProperty, new Binding("Key"));

				View = new StackLayout
				{
					BackgroundColor = Color.Pink,
					Children = { title }
				};
			}
		}

		[Preserve(AllMembers = true)]
		class UnevenCell : ViewCell
		{
			public UnevenCell()
			{

				SetBinding(HeightProperty, new Binding("RowHeight"));

				var label = new Label();
				label.SetBinding(Label.TextProperty, new Binding("Name"));

				var layout = new StackLayout
				{
					BackgroundColor = Color.Red,
					Children = {
						label
					}
				};

				View = layout;
			}
		}

		[Preserve(AllMembers = true)]
		internal class ListViewViewModel
		{
			public ObservableCollection<Grouping<string, Employee>> CategorizedEmployees { get; private set; }
			public ObservableCollection<Employee> Employees { get; private set; }

			public ListViewViewModel()
			{
				CategorizedEmployees = new ObservableCollection<Grouping<string, Employee>> {
					new Grouping<string, Employee> (
						"Engineer",
						new [] {
							new Employee ("Seth", TimeSpan.FromDays (10), 60),
							new Employee ("Jason", TimeSpan.FromDays (100), 100),
							new Employee ("Eric", TimeSpan.FromDays (1000), 160),
						}
					),
					new Grouping<string, Employee> (
						"Sales",
						new [] {
							new Employee ("Andrew 1", TimeSpan.FromDays (10), 160),
							new Employee ("Andrew 2", TimeSpan.FromDays (100), 100),
							new Employee ("Andrew 3", TimeSpan.FromDays (1000), 60),
						}
					),
				};

				Employees = new ObservableCollection<Employee> {
					new Employee ("Seth", TimeSpan.FromDays (10), 60),
					new Employee ("Jason", TimeSpan.FromDays (100), 100),
					new Employee ("Eric", TimeSpan.FromDays (1000), 160),
					new Employee ("Andrew 1", TimeSpan.FromDays (10), 160),
					new Employee ("Andrew 2", TimeSpan.FromDays (100), 100),
					new Employee ("Andrew 3", TimeSpan.FromDays (1000), 60),
				};

				Enumerable.Range(0, 9000).Select(e => new Employee(e.ToString(), TimeSpan.FromDays(1), 60)).ForEach(e => Employees.Add(e));
			}
		}

		protected override bool SupportsFocus
		{
			get { return false; }
		}

		protected override void InitializeElement(ListView element)
		{
			InitializeElementListView(element, 60);
		}

		private void InitializeElementListView(ListView element, int rowHeight)
		{
			element.HeightRequest = 350;
			element.RowHeight = rowHeight;

			var viewModel = new ListViewViewModel();
			element.BindingContext = viewModel;

			element.ItemsSource = viewModel.Employees;

			var template = new DataTemplate(typeof(TextCell));
			template.SetBinding(TextCell.TextProperty, "Name");
			template.SetBinding(TextCell.DetailProperty, new Binding("DaysWorked", converter: new GenericValueConverter(time => time.ToString())));

			element.ItemTemplate = template;
		}

		protected override void Build(StackLayout stackLayout)
		{
			base.Build(stackLayout);

			var viewModel = new ListViewViewModel();

			var groupDisplayBindingContainer = new ViewContainer<ListView>(Test.ListView.GroupDisplayBinding, new ListView());
			InitializeElementListView(groupDisplayBindingContainer.View, 0);
			groupDisplayBindingContainer.View.ItemsSource = viewModel.CategorizedEmployees;
			groupDisplayBindingContainer.View.IsGroupingEnabled = true;
			groupDisplayBindingContainer.View.GroupDisplayBinding = new Binding("Key");


			var groupHeaderTemplateContainer = new ViewContainer<ListView>(Test.ListView.GroupHeaderTemplate, new ListView());
			InitializeElementListView(groupHeaderTemplateContainer.View, 0);
			groupHeaderTemplateContainer.View.ItemsSource = viewModel.CategorizedEmployees;
			groupHeaderTemplateContainer.View.IsGroupingEnabled = true;
			groupHeaderTemplateContainer.View.GroupHeaderTemplate = new DataTemplate(typeof(HeaderCell));

			var groupShortNameContainer = new ViewContainer<ListView>(Test.ListView.GroupShortNameBinding, new ListView());
			InitializeElementListView(groupShortNameContainer.View, 0);
			groupShortNameContainer.View.ItemsSource = viewModel.CategorizedEmployees;
			groupShortNameContainer.View.IsGroupingEnabled = true;
			groupShortNameContainer.View.GroupShortNameBinding = new Binding("Key");

			// TODO - not sure how to do this
			var hasUnevenRowsContainer = new ViewContainer<ListView>(Test.ListView.HasUnevenRows, new ListView());
			InitializeElement(hasUnevenRowsContainer.View);
			hasUnevenRowsContainer.View.HasUnevenRows = true;
			hasUnevenRowsContainer.View.ItemTemplate = new DataTemplate(typeof(UnevenCell));

			var isGroupingEnabledContainer = new StateViewContainer<ListView>(Test.ListView.IsGroupingEnabled, new ListView());
			InitializeElement(isGroupingEnabledContainer.View);
			isGroupingEnabledContainer.View.ItemsSource = viewModel.CategorizedEmployees;
			isGroupingEnabledContainer.View.IsGroupingEnabled = true;
			isGroupingEnabledContainer.StateChangeButton.Clicked += (sender, args) => isGroupingEnabledContainer.View.IsGroupingEnabled = !isGroupingEnabledContainer.View.IsGroupingEnabled;


			var itemAppearingContainer = new EventViewContainer<ListView>(Test.ListView.ItemAppearing, new ListView());
			InitializeElement(itemAppearingContainer.View);
			itemAppearingContainer.View.ItemAppearing += (sender, args) => itemAppearingContainer.EventFired();

			var itemDisappearingContainer = new EventViewContainer<ListView>(Test.ListView.ItemDisappearing, new ListView());
			InitializeElement(itemDisappearingContainer.View);
			itemDisappearingContainer.View.ItemDisappearing += (sender, args) => itemDisappearingContainer.EventFired();

			var itemSelectedContainer = new EventViewContainer<ListView>(Test.ListView.ItemSelected, new ListView());
			InitializeElement(itemSelectedContainer.View);
			itemSelectedContainer.View.ItemSelected += (sender, args) => itemSelectedContainer.EventFired();

			var itemTappedContainer = new EventViewContainer<ListView>(Test.ListView.ItemTapped, new ListView());
			InitializeElement(itemTappedContainer.View);
			itemTappedContainer.View.ItemTapped += (sender, args) => itemTappedContainer.EventFired();

			// TODO
			var rowHeightContainer = new ViewContainer<ListView>(Test.ListView.RowHeight, new ListView());
			InitializeElement(rowHeightContainer.View);

			var selectedItemContainer = new ViewContainer<ListView>(Test.ListView.SelectedItem, new ListView());
			InitializeElement(selectedItemContainer.View);
			selectedItemContainer.View.SelectedItem = viewModel.Employees[2];

			var fastScrollItemContainer = new ViewContainer<ListView>(Test.ListView.FastScroll, new ListView());
			InitializeElement(fastScrollItemContainer.View);
			fastScrollItemContainer.View.On<Android>().SetIsFastScrollEnabled(true);
			fastScrollItemContainer.View.ItemsSource = viewModel.CategorizedEmployees;

			var scrolledItemContainer = new ViewContainer<ListView>(Test.ListView.Scrolled, new ListView());
			InitializeElement(scrolledItemContainer.View);
			scrolledItemContainer.View.ItemsSource = viewModel.Employees;
			var scrollTitle = scrolledItemContainer.TitleLabel.Text;
			scrolledItemContainer.View.Scrolled += (sender, args) =>
			{
				scrolledItemContainer.TitleLabel.Text = $"{scrollTitle}; X={args.ScrollX};Y={args.ScrollY}";
			};

			var refreshControlColorContainer = new ViewContainer<ListView>(Test.ListView.RefreshControlColor, new ListView());
			InitializeElement(refreshControlColorContainer.View);
			refreshControlColorContainer.View.RefreshControlColor = Color.Red;
			refreshControlColorContainer.View.IsPullToRefreshEnabled = true;
			refreshControlColorContainer.View.Refreshing += async (object sender, EventArgs e) =>
			{
				await Task.Delay(2000);
				refreshControlColorContainer.View.IsRefreshing = false;
			};
			refreshControlColorContainer.View.ItemsSource = viewModel.Employees;

			var scrollbarVisibilityContainer = new ViewContainer<ListView>(Test.ListView.ScrollBarVisibility, new ListView());
			InitializeElement(scrollbarVisibilityContainer.View);
			scrollbarVisibilityContainer.View.HorizontalScrollBarVisibility = ScrollBarVisibility.Never;
			scrollbarVisibilityContainer.View.VerticalScrollBarVisibility = ScrollBarVisibility.Never;
			scrollbarVisibilityContainer.View.ItemsSource = viewModel.CategorizedEmployees;
			scrollbarVisibilityContainer.View.IsGroupingEnabled = true;
			scrollbarVisibilityContainer.View.GroupDisplayBinding = new Binding("Key");

			Add(groupDisplayBindingContainer);
			Add(groupHeaderTemplateContainer);
			Add(groupShortNameContainer);
			Add(hasUnevenRowsContainer);
			Add(isGroupingEnabledContainer);
			Add(itemAppearingContainer);
			Add(itemDisappearingContainer);
			Add(itemSelectedContainer);
			Add(itemTappedContainer);
			Add(rowHeightContainer);
			Add(selectedItemContainer);
			Add(fastScrollItemContainer);
			Add(scrolledItemContainer);
			Add(refreshControlColorContainer);
			Add(scrollbarVisibilityContainer);
		}
	}
}