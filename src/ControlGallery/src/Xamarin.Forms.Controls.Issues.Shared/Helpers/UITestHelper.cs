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
		public static bool IsWindowClosedException(this Exception exc)
		{
#if __WINDOWS__
			return exc.Message?.Contains("Currently selected window has been closed") ?? false;
#else
			return false;
#endif
		}

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