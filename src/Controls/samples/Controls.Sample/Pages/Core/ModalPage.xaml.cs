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
		public ModalPage()
		{
			InitializeComponent();
			BackgroundColor = Colors.Purple;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			PushModal.IsVisible = Navigation.ModalStack.Count > 0;
		}

		async void PushModalClicked(object sender, EventArgs e)
		{
			await Navigation.PushModalAsync(new ModalPage()
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