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
		public BasePage()
		{
			NavigateCommand = new Command<SectionModel>(sectionModel =>
			{
				if (sectionModel != null)
					Navigation.PushAsync(PreparePage(sectionModel));
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

		Page PreparePage(SectionModel model)
		{
			var page = (Page)Activator.CreateInstance(model.Type);
			page.Title = model.Title;

			return page;
		}
	}
}