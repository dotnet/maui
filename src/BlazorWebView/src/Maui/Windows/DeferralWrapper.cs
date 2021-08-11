using Microsoft.AspNetCore.Components.WebView.WebView2;
using Windows.Foundation;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	internal class DeferralWrapper : IDeferralWrapper
	{
		private readonly Deferral _deferral;

		public DeferralWrapper(Deferral deferral)
		{
			_deferral = deferral ?? throw new System.ArgumentNullException(nameof(deferral));
		}

		public void Complete()
		{
			_deferral.Complete();
		}

		public void Dispose()
		{
			_deferral.Dispose();
		}
	}
}