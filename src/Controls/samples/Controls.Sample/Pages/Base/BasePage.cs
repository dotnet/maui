using System;
using System.Diagnostics;
using System.Windows.Input;
using Maui.Controls.Sample.Models;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages.Base
{
	public class BasePage : ContentPage, IPage
	{
		SectionModel _selectedItem;

		public BasePage()
		{
			NavigateCommand = new Command(async () =>
			{
				if (SelectedItem != null)
				{
					await Navigation.PushAsync(PreparePage(SelectedItem));

					SelectedItem = null;
				}
			});
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
			var page = (Page)Activator.CreateInstance(model.Type);
			page.Title = model.Title;

			return page;
		}
	}
}