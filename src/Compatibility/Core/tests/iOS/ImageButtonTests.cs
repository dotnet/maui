using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foundation;
using Xunit;
using ObjCRuntime;
using UIKit;
using CategoryAttribute = NUnit.Framework.CategoryAttribute;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS.UnitTests
{
	// [TestFixture] - removed for xUnit
	public class ImageButtonTests : PlatformTestFixture
	{
		[Test, Category("ImageButton")]
		public async Task CreatedWithCorrectButtonType()
		{
			var imageButton = new ImageButton();
			UIButtonType buttonType = await GetControlProperty(imageButton, uiButton => uiButton.ButtonType);
			Assert.NotEqual(UIButtonType.Custom, buttonType);
		}
	}
}