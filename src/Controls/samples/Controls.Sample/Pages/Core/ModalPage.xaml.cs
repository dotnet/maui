using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public partial class ModalPage
	{
		static int s_instanceCount = 0;
		public ModalPage()
		{
			InitializeComponent();
			BackgroundColor = Colors.Purple;
			Title = $"Modal Page {s_instanceCount++}";
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			PushModal.IsVisible = Navigation.ModalStack.Count > 0;
		}

		async void PushNavigationModalClicked(object sender, EventArgs e)
		{
			var modalPage = new ModalPage();
			Page pushMe = new NavigationPage(modalPage)
			{
				BackgroundColor =
						(BackgroundColor == Colors.Purple) ? Colors.Pink : Colors.Purple,
				Title = $"Navigation Root: {modalPage.Title}"
			};

			await Navigation.PushModalAsync(pushMe);

		}

		async void PushModalClicked(object sender, EventArgs e)
		{
			Page pushMe = new ModalPage()
			{
				BackgroundColor =
						   (BackgroundColor == Colors.Purple) ? Colors.Pink : Colors.Purple
			};

			await Navigation.PushModalAsync(pushMe);
		}

		async void PushClicked(object sender, EventArgs e)
		{
			await Navigation.PushAsync(new ModalPage()
			{
				BackgroundColor =
					(BackgroundColor == Colors.Purple) ? Colors.Pink : Colors.Purple
			});
		}

		async void PopModalClicked(object sender, EventArgs e)
		{
			await Navigation.PopModalAsync();
		}
	}
}