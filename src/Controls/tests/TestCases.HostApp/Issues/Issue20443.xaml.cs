using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using System.Runtime.CompilerServices;

namespace Controls.TestCases.HostApp.Issues
{
	[Issue(IssueTracker.Github, 20443, "CollectionView item gets wrong size after refresh", PlatformAffected.iOS)]

	public partial class Issue20443 : ContentPage
	{
		public Issue20443()
		{
			InitializeComponent();
		}
	}
	public class Issue20443ViewModel : INotifyPropertyChanged
	{
		public IList<object> Items { get; set; }

		private bool _isRefreshing;

		public bool IsRefreshing
		{
			get => _isRefreshing;
			set
			{
				_isRefreshing = value;
				OnPropertyChanged();
			}
		}

		public Command RefreshCommand { get; set; }

		public Issue20443ViewModel()
		{
			RefreshCommand = new Command(
				async () =>
				{
					await Task.Delay(2000);
					IsRefreshing = false;
				});

			Items = new List<object>();
			for (int i = 0; i < 100; i++)
			{
				Items.Add(new Issue20443ItemA());
				Items.Add(new Issue20443ItemB());
				Items.Add(new Issue20443ItemB());
				Items.Add(new Issue20443ItemB());
				Items.Add(new Issue20443ItemB());
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	public class Issue20443ItemA
	{

	}

	public class Issue20443ItemB
	{

	}

	public class Issue20443TemplateSelector : DataTemplateSelector
	{
		public DataTemplate ItemATemplate { get; set; }
		public DataTemplate ItemBTemplate { get; set; }

		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			if (item is Issue20443ItemA)
			{
				return ItemATemplate;
			}

			return ItemBTemplate;
		}
	}
}
