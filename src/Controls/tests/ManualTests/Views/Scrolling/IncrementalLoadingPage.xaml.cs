using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Views
{
	public partial class IncrementalLoadingPage : ContentPage
	{
		public IncrementalLoadingPage()
		{
			InitializeComponent();
			BindingContext = new AnimalsViewModel();
		}

		void OnCollectionViewRemainingItemsThresholdReached(object sender, EventArgs e)
		{
			// Retrieve more data here, or via the RemainingItemsThresholdReachedCommand.
			// This sample retrieves more data using the RemainingItemsThresholdReachedCommand.
		}
	}
}
