#if UITEST
using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.UITest.Queries;

namespace Xamarin.Forms.Controls.Issues
{
	public static class UITestHelper
	{
		public static string ReadText(this AppResult result) =>
			result.Text ?? result.Description;


		public static void AssertHasText(this AppResult result, string text)
		{
			if(String.Equals(result.Description, text, StringComparison.OrdinalIgnoreCase) ||
				String.Equals(result.Text, text, StringComparison.OrdinalIgnoreCase) ||
				String.Equals(result.Label, text, StringComparison.OrdinalIgnoreCase))
			{
				return;
			}

			Assert.Fail();
		}
	}
}

#endif