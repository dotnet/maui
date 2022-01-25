using System;
using System.Diagnostics;
using System.Windows.Input;
using Maui.Controls.Sample.Models;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages.Base
{
	public class BasePage : ContentPage
	{
		SectionModel _selectedItem;

		public BasePage()
		{
			NavigateCommand = new Command(async () =>
			{
				if (SelectedItem != null)
				{
					if (Application.Current.MainPage is FlyoutPage fp)
						await fp.Detail.Navigation.PushAsync(PreparePage(SelectedItem));
					else
						await Navigation.PushAsync(PreparePage(SelectedItem));

					SelectedItem = null;
				}
			});

			ToolbarItems.Add(new ToolbarItem()
			{
				Text = "Settings",
				IconImageSource = ImageSource.FromFile("settings.png"),
				Command = new Command(OnToolbarItemClicked)
			});
		}

		void OnToolbarItemClicked()
		{
			Navigation.PushModalAsync(new SettingsPage());
		}

		protected override void OnAppearing()
		{
			Debug.WriteLine($"OnAppearing: {this}");
		}

		protected override void OnDisappearing()
		{
			Debug.WriteLine($"OnDisappearing: {this}");
		}

		public ICommand NavigateCommand { get; }

		public SectionModel SelectedItem
		{
			get { return _selectedItem; }
			set
			{
				_selectedItem = value;
				OnPropertyChanged();
			}
		}

		Page PreparePage(SectionModel model)
		{
			var page = (Handler?.MauiContext?.Services?.GetService(model.Type) as Page) ?? (Page)Activator.CreateInstance(model.Type);
			page.Title = model.Title;

			return page;
		}
	}
}