using System;
using System.Diagnostics;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class ImageButtonPage
	{
		int _clickTotal;

		public ImageButtonPage()
		{
			InitializeComponent();

			BindingContext = new ImageButtonPageViewModel();

			ImageButton01.PropertyChanged += (s, e) =>
			{
				if (e.PropertyName == ImageButton.IsLoadingProperty.PropertyName)
					Debug.WriteLine($"{e.PropertyName}: {ImageButton01.IsLoading}");
			};
		}

		void OnImageButtonClicked(object sender, EventArgs e)
		{
			_clickTotal += 1;
			InfoLabel.Text = $"{_clickTotal} ImageButton click{(_clickTotal == 1 ? "" : "s")}";
		}

		void OnResizeImageButtonClicked(object sender, EventArgs e)
		{
			ResizeImageButton.HeightRequest = 100;
			ResizeImageButton.WidthRequest = 100;
		}
	}

	public class ImageButtonPageViewModel : BindableObject
	{
		public ICommand ImageButtonCommand => new Command(OnExecuteImageButtonCommand);

		void OnExecuteImageButtonCommand()
		{
			Debug.WriteLine("Command");
		}
	}
}