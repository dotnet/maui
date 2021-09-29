using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Recipes.Services;
using Recipes.Views;
using System.Collections.Generic;

namespace Recipes
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();
            Microsoft.Maui.Controls.Device.SetFlags(new List<string> { "Accessibility_Experimental" });
            DependencyService.Register<MockDataStore>();
            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
