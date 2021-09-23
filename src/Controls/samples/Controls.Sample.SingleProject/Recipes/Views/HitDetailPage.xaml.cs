using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Recipes.ViewModels;
using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Essentials;

namespace Recipes.Views
{
    public partial class HitDetailPage : ContentPage
    {

        HitDetailViewModel _viewModel;
		Color primary = Color.FromArgb("#2BB0ED");
		string alertMessage = "This recipe has been added to your personal recipe collection! Go to the 'My Recipes' tab to view and modify this recipe from there";

		public HitDetailPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new HitDetailViewModel();
        }

		void AddItem_Clicked(System.Object sender, System.EventArgs e)
		{
			SemanticScreenReader.Announce(alertMessage);

			// TODO MAUI
			//var messageOptions = new MessageOptions
			//{
			//	Foreground = Color.White,
			//	Message = alertMessage,
			//	Font = Font.SystemFontOfSize(14),
			//	Padding = new Thickness(10)
			//};

			//var options = new SnackBarOptions
			//{
			//	MessageOptions = messageOptions,
			//	Duration = TimeSpan.FromMilliseconds(5000),
			//	BackgroundColor = primary,
			//	IsRtl = false
			//};

			//await this.DisplaySnackBarAsync(options);
		}
	}
}