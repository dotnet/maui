using Xamarin.Essentials;

namespace Samples.UWP
{
    public sealed partial class MainPage : Xamarin.Forms.Platform.UWP.WindowsPage
    {
        static Samples.App formsApp;

        public MainPage()
        {
            InitializeComponent();

            Platform.MapServiceToken = "RJHqIE53Onrqons5CNOx~FrDr3XhjDTyEXEjng-CRoA~Aj69MhNManYUKxo6QcwZ0wmXBtyva0zwuHB04rFYAPf7qqGJ5cHb03RCDw1jIW8l";

            LoadApplication(formsApp ??= new Samples.App());
        }
    }
}
