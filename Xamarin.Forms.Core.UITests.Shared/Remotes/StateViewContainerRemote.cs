using System;
using System.Linq;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace Xamarin.Forms.Core.UITests
{
	internal sealed class StateViewContainerRemote : BaseViewContainerRemote
	{
		public StateViewContainerRemote(IApp app, Enum formsType, string platformViewType)
			: base(app, formsType, platformViewType)
		{
		}

		public void TapStateButton()
		{
			App.Screenshot("Before state change");
			App.Tap(q => q.Raw(StateButtonQuery));
			App.Screenshot("After state change");
		}

		public AppResult GetStateLabel()
		{
			return App.Query(q => q.Raw(StateLabelQuery)).First();
		}
	}
}