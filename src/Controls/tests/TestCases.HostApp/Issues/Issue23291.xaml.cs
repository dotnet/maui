using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, "23291", "Changing Position no longer works after navigation away then coming back", PlatformAffected.iOS)]
	public class Issue23291NavPage : NavigationPage
	{
		public Issue23291NavPage() : base(new Issue23291()) { }
	}

	public partial class Issue23291 : ContentPage
	{
		public Issue23291()
		{
			InitializeComponent();
			BindingContext = new Issue23291ViewModel();
		}

		public async void OpenDetailPage_Clicked(object obj, EventArgs eventArgs)
		{
			await Navigation.PushAsync(new ContentPage() { Title = "Detail page" });
			await Navigation.PopAsync();
		}

		public class DashboardItem
		{
			public int FragmentNumber { get; set; }
		}
	}

	public class Issue23291DataTemplateSelector : DataTemplateSelector
	{
		public DataTemplate FragmentOneItemTemplate { get; set; }
		public DataTemplate FragmentTwoItemTemplate { get; set; }

		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			if (item is Issue23291.DashboardItem fragmentInfo)
			{
				if (fragmentInfo.FragmentNumber == 1)
				{
					return FragmentOneItemTemplate;
				}

				return FragmentTwoItemTemplate;
			}

			return FragmentOneItemTemplate;
		}
	}

	public class Issue23291ViewModel : INotifyPropertyChanged
	{
		private int _pagePosition;
		public int PagePosition
		{
			get => _pagePosition;
			set
			{
				_pagePosition = value;
				OnPropertyChanged();
			}
		}

		public Command OpenFragmentOneCommand => new Command(() => PagePosition = 0);
		public Command OpenFragmentTwoCommand => new Command(() => PagePosition = 1);

		public ObservableCollection<Issue23291.DashboardItem> DashboardItems { get; set; }

		public Issue23291ViewModel()
		{
			DashboardItems = new()
			{
				new Issue23291.DashboardItem { FragmentNumber = 1 },
				new Issue23291.DashboardItem { FragmentNumber = 2 }
			};
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}