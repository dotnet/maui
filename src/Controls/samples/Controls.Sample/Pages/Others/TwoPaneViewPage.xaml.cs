namespace Maui.Controls.Sample.Pages
{
	using Microsoft.Maui.Controls.DualScreen;

	public partial class TwoPaneViewPage
	{
		public TwoPaneViewPage()
		{
			InitializeComponent();

			System.Diagnostics.Debug.Write("TwoPaneViewPage.ctor", "JWM");
		}
		protected override async void OnAppearing()
		{
			base.OnAppearing();
			System.Diagnostics.Debug.Write("TwoPaneViewPage.OnAppearing - hinge angle prepped", "JWM");
			DualScreenInfo.Current.HingeAngleChanged += Current_HingeAngleChanged;
			hingeLabel.Text = "Hinge prepped " + await DualScreenInfo.Current.GetHingeAngleAsync();
			hingeLabel.Text += " spanmode:" + DualScreenInfo.Current.SpanMode;
		}
		protected override void OnDisappearing()
		{
			DualScreenInfo.Current.HingeAngleChanged -= Current_HingeAngleChanged;
		}
		private void Current_HingeAngleChanged(object sender, HingeAngleChangedEventArgs e)
		{
			System.Diagnostics.Debug.Write("TwoPaneViewPage.Current_HingeAngleChanged - " + e.HingeAngleInDegrees, "JWM");

			hingeLabel.Text = e.HingeAngleInDegrees + " degrees";
		}
	}
}