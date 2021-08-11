using Microsoft.AspNetCore.Components.WebView.WebView2;
using Windows.Foundation;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	internal class DeferralWrapper : IDeferralWrapper
	{
		private Deferral _deferral;

		public DeferralWrapper(Deferral deferral)
		{
			_deferral = deferral;
		}

		public void Complete()
		{
			_deferral.Complete();
		}
	}
}