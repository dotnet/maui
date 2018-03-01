namespace Caboodle.Samples.UWP
{
    public sealed partial class MainPage : Xamarin.Forms.Platform.UWP.WindowsPage
    {
        public MainPage()
        {
            InitializeComponent();

            LoadApplication(new Caboodle.Samples.App());
        }
    }
}
