using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Caboodle.Samples.View;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace Caboodle.Samples
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new HomePage());
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
