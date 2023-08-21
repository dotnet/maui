// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xamarin.UITest.Queries;

namespace Microsoft.Maui.AppiumTests
{
	public static class UITestHelper
	{
		public static string ReadText(this AppResult result) => result.Text ?? result.Description;
	}

}