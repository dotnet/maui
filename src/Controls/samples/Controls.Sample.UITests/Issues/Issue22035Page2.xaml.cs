using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Issue22035Page2 : ContentPage
	{
		public Issue22035Page2()
		{
			InitializeComponent();

			BindingContext = this;
		}

		public ObservableCollection<Issue22035Model> Images { get; set; } = new();
		
		async void OnLoadButtonPressed(object sender, EventArgs e)
		{
			Images.Clear();

			await Task.Delay(1000);

			Images.Add(new Issue22035Model { ImagePath = "photo21314.jpg" });
			Images.Add(new Issue22035Model { ImagePath = "oasis.jpg" });
			Images.Add(new Issue22035Model { ImagePath = "photo21314.jpg" });
			Images.Add(new Issue22035Model { ImagePath = "oasis.jpg" });
		}
	}

	public class Issue22035Model
	{
		public string ImagePath { get; set; }
	}
}