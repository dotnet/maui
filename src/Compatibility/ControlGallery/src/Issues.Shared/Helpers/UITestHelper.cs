#if UITEST
using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.UITest.Queries;
using System.Linq;
using Microsoft.Maui.Controls.Compatibility.UITests;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	using IApp = Xamarin.UITest.IApp;
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

		public static string[] GetTestCategories(Type testType)
		{
			var testClassName = TestContext.CurrentContext.Test.ClassName;

			// TestContext.CurrentContext.Test.Properties["Category"]
			// Only gives you the categories on the test itself
			// There isn't a property I could find that gives you the Categories
			// on the Test Class
			return testType
						.Assembly
						.GetType(testClassName)
						.GetCustomAttributes(typeof(NUnit.Framework.CategoryAttribute), true)
						.OfType<NUnit.Framework.CategoryAttribute>()
						.Select(x => x.Name)
						.Union(TestContext.CurrentContext.Test.Properties["Category"].OfType<string>())
						.ToArray();
		}

		public static void MarkTestInconclusiveIfNoInternetConnectionIsPresent(Type testType, IApp app)
		{
			if (GetTestCategories(testType).Contains(UITestCategories.RequiresInternetConnection))
			{
				var hasInternetAccess = $"{app.Invoke("hasInternetAccess")}";
				bool checkInternet;

				if (bool.TryParse(hasInternetAccess, out checkInternet))
				{
					if (!checkInternet)
						Assert.Inconclusive("Device Has No Internet Connection");
				}
			}
		}
	}
}

#endif