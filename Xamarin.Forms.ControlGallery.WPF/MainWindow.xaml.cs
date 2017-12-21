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
			Xamarin.Forms.Forms.Init();
			FormsMaps.Init("");
			LoadApplication(new Controls.App());
            //LoadApplication(new OpenGLViewApp());
        }
	}
}
