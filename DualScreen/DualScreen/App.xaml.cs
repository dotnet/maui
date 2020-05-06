using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DualScreen
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new TwoPaneViewGallery());
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
