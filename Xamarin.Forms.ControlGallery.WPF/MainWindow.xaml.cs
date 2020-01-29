using Xamarin.Forms.Platform.WPF;

namespace Xamarin.Forms.ControlGallery.WPF
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FormsApplicationPage
	{
		public MainWindow()
		{
			InitializeComponent();
			Forms.SetFlags("CarouselView_Experimental", "MediaElement_Experimental");
			Xamarin.Forms.Forms.Init();
			FormsMaps.Init("");
			LoadApplication(new Controls.App());
        }
	}
}
