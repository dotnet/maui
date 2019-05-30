
using Xamarin.Forms.Platform.WPF;

namespace Xamarin.Forms.Sandbox.WPF
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Xamarin.Forms.Platform.WPF.FormsApplicationPage
	{
		public MainWindow()
		{
			InitializeComponent();
			Xamarin.Forms.Forms.Init();
			FormsMaps.Init("");
			LoadApplication(new Sandbox.App());
		}
	}
}
