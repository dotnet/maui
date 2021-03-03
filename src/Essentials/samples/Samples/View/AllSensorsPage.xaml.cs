using System.Threading.Tasks;
using Samples.ViewModel;
using Xamarin.Forms;

namespace Samples.View
{
	public partial class AllSensorsPage : BasePage
	{
		public AllSensorsPage()
		{
			InitializeComponent();
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			SetupBinding(GridAccelerometer.BindingContext);
			SetupBinding(GridCompass.BindingContext);
			SetupBinding(GridGyro.BindingContext);
			SetupBinding(GridMagnetometer.BindingContext);
			SetupBinding(GridOrientation.BindingContext);
			SetupBinding(GridBarometer.BindingContext);
		}

		protected override void OnDisappearing()
		{
			TearDownBinding(GridAccelerometer.BindingContext);
			TearDownBinding(GridCompass.BindingContext);
			TearDownBinding(GridGyro.BindingContext);
			TearDownBinding(GridMagnetometer.BindingContext);
			TearDownBinding(GridOrientation.BindingContext);
			TearDownBinding(GridBarometer.BindingContext);
			base.OnDisappearing();
		}
	}
}
