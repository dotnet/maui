using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 23148, "Incorrect height of CollectionView when ItemsSource is empty", PlatformAffected.Android)]

	public partial class Issue23148 : ContentPage
	{
		public ObservableCollection<string> Items { get; set; } = [];

		public ICommand AddCommand => new Command(() => Items.Add("Item"));

		public Issue23148()
		{
			InitializeComponent();
			BindingContext = this;
		}
	}
}
