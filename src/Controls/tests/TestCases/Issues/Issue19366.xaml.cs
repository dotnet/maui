using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 19366, "Items are enabled when ListView is not enabled", PlatformAffected.iOS)]
	public partial class Issue19366 : ContentPage
	{
		public Issue19366()
		{
			InitializeComponent();
			List<ItemViewModel> items = new List<ItemViewModel>();
			for (int i = 1; i <= 5; i++)
			{
				items.Add(new ItemViewModel()
				{
					Text = $"Not clicked",
					Index = $"{i}",
				});
			}

			listView.ItemsSource = items;
		}

		void Button_Clicked(System.Object sender, System.EventArgs e)
		{
			listView.IsEnabled = !listView.IsEnabled;
		}
	}

	public class ItemViewModel : INotifyPropertyChanged
	{
		private string _text;
		public string Text
		{
			get => _text;
			set
			{
				_text = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
			}
		}

		private string _isEnabled;
		public string IsEnabled
		{
			get => _isEnabled;
			set
			{
				_isEnabled = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEnabled)));
			}
		}

		public string Index { get; set; }

		public Command ClickedCommand { get; set; }

		public ItemViewModel()
		{
			ClickedCommand = new Command(() => Text = "Clicked");
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}