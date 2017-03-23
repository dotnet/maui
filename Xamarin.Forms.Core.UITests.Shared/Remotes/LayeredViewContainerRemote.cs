using System;
using System.Linq;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace Xamarin.Forms.Core.UITests
{
	internal sealed class LayeredViewContainerRemote : BaseViewContainerRemote
	{
		public LayeredViewContainerRemote(IApp app, Enum formsType, string platformViewType)
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