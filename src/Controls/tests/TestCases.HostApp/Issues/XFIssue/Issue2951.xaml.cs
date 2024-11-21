using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 2951, "On Android, button background is not updated when color changes ")]
public partial class Issue2951 : TestContentPage
{
	public Issue2951()
	{
		InitializeComponent();
	}

	async void ListView_ItemAppearing(object sender, ItemVisibilityEventArgs e)
	{
		if (e.ItemIndex == 2)
		{
			await Task.Delay(10);

			lblReady.Text = "Ready";
		}
	}

	protected override void Init()
	{
		BindingContext = new MyViewModel();
	}

	public class MyViewModel
	{
		public ObservableCollection<MyItemViewModel> Items { get; private set; }

		public Command<MyItemViewModel> ButtonTapped { get; private set; }

		public MyViewModel()
		{
			ButtonTapped = new Command<MyItemViewModel>(OnItemTapped);

			Items = new ObservableCollection<MyItemViewModel>();

			Items.Add(new MyItemViewModel { Name = "A", IsStarted = false });
			Items.Add(new MyItemViewModel { Name = "B", IsStarted = false });
			Items.Add(new MyItemViewModel { Name = "C", IsStarted = false });
		}

		void OnItemTapped(MyItemViewModel model)
		{
			if (model.IsStarted)
			{
				Items.Remove(model);
			}
			else
			{
				model.IsStarted = true;
			}
		}
	}

	public class MyItemViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		string _name;

		public string Name
		{
			get { return _name; }
			set
			{
				_name = value;
				OnPropertyChanged("Name");
			}
		}

		bool _isStarted;

		public bool IsStarted
		{
			get { return _isStarted; }
			set
			{
				_isStarted = value;
				OnPropertyChanged("IsStarted");
			}
		}

		void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}

public class ButtonExtensions
{
	public static readonly BindableProperty IsPrimaryProperty = BindableProperty.CreateAttached("IsPrimary", typeof(bool), typeof(ButtonExtensions), false, BindingMode.TwoWay, null, null, null, null);

	public static bool GetIsPrimary(BindableObject bo)
	{
		return (bool)bo.GetValue(IsPrimaryProperty);
	}

	public static void SetIsPrimary(BindableObject bo, bool value)
	{
		bo.SetValue(IsPrimaryProperty, value);
	}
}
