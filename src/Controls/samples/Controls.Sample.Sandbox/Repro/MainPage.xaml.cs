using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using System;


namespace LabelTextWrapTest
{
    public partial class MainPage : ContentPage
    {
       

        public MainPage()
        {
            InitializeComponent();
        }


        private async void Button_OnClicked(object? sender, EventArgs e)
        {
            await this.ShowPopupAsync(new ClonePopup());
        }
    }

}
