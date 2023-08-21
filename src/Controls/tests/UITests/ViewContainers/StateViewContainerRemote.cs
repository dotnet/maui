// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using TestUtils.Appium.UITests;
using Xamarin.UITest.Queries;

namespace Microsoft.Maui.AppiumTests
{
	internal sealed class StateViewContainerRemote : BaseViewContainerRemote
	{
		public StateViewContainerRemote(IUITestContext? testContext, Enum formsType, string? platformViewType)
			: base(testContext, formsType, platformViewType)
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