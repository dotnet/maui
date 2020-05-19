using System.Maui.Platform.WPF;

namespace System.Maui.ControlGallery.WPF
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FormsApplicationPage
	{
		public MainWindow()
		{
			InitializeComponent();
			System.Maui.Maui.SetFlags("CarouselView_Experimental", "MediaElement_Experimental");
			System.Maui.Maui.Init();
			FormsMaps.Init("");
			LoadApplication(new Controls.App());
        }
	}
}
