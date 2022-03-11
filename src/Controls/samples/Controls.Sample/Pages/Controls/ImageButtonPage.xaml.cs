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
		}

		void OnImageButtonClicked(object sender, EventArgs e)
		{
			_clickTotal += 1;
			InfoLabel.Text = $"{_clickTotal} ImageButton click{(_clickTotal == 1 ? "" : "s")}";
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