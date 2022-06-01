using System;
using Microsoft.Maui;

namespace Maui.Controls.Sample.Pages
{
	public partial class TemplatePage
	{
		public TemplatePage()
		{
			InitializeComponent();

			TheScrollView.PropertyChanged += (sender, args) => {
				switch (args.PropertyName)
				{
					case "ContentSize":
						System.Diagnostics.Debug.WriteLine($">>>>>> ContentSize Changed to {(sender as IScrollView).ContentSize}");
						break;
				}
			};

			UpdateButton.Clicked += (sender, args) => 
			{
				if (TheLabel.HeightRequest == 50)
				{
					TheLabel.HeightRequest = 75;
				}
				else
				{
					TheLabel.HeightRequest = 50;
				}
			};
		}

	}
}