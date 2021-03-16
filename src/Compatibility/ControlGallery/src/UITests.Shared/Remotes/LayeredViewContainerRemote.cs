using System;
using System.Linq;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace Microsoft.Maui.Controls.Compatibility.UITests
{
	using IApp = Xamarin.UITest.IApp;
	internal sealed class LayeredViewContainerRemote : BaseViewContainerRemote
	{
		public LayeredViewContainerRemote(Xamarin.UITest.IApp app, Enum formsType, string platformViewType)
			: base(app, formsType, platformViewType)
		{
		}

		public AppResult GetLayeredLabel()
		{
			return App.Query(q => q.Raw(LayeredLabelQuery)).First();
		}

		public void TapHiddenButton()
		{
			App.Tap(q => q.Raw(LayeredHiddenButtonQuery));
		}
	}
}