using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public partial class NavigationGallery
	{
		static int pageCount = 0;
		public NavigationGallery()
		{
			InitializeComponent();
			pageCount++;
			lblPageCount.Text = $"{pageCount}";
			this.Title = $"PAGE NUMBER {pageCount}";
		}

		void InsertPage(object sender, EventArgs e)
		{
			Navigation.InsertPageBefore(new NavigationGallery(), Navigation.NavigationStack.Last());
		}

		async void PopPage(object sender, EventArgs e)
		{
			await Navigation.PopAsync(true);
		}

		async void PushPage(object sender, EventArgs e)
		{
			await Navigation.PushAsync(new NavigationGallery(), true);
		}

		async void PopToRoot(object sender, EventArgs e)
		{
			await Navigation.PopToRootAsync(true);
		}

		void RemovePage(object sender, EventArgs e)
		{
			if (Navigation.NavigationStack.Count >= 2)
				Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2]);
		}

		void ToggleNavigationBar(object sender, EventArgs e)
		{
			NavigationPage.SetHasNavigationBar(this, !NavigationPage.GetHasNavigationBar(this));
		}

		void ToggleBackButton(object sender, EventArgs e)
		{
			NavigationPage.SetHasBackButton(this, !NavigationPage.GetHasBackButton(this));
		}


		void SwapRoot(object sender, EventArgs e)
		{
			(Parent as INavigationView).RequestNavigation(
				new MauiNavigationRequestedEventArgs(
					new List<NavigationGallery>
					{
						new NavigationGallery()
					}, false));
		}
	}
}