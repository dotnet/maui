namespace Samples.UWP
{
    public sealed partial class MainPage : Xamarin.Forms.Platform.UWP.WindowsPage
    {
        public MainPage()
        {
            InitializeComponent();

            LoadApplication(new Samples.App());
        }
    }
}
