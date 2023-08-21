// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class CarouselPageTests : MultiPageTests<ContentPage>
	{
		protected override MultiPage<ContentPage> CreateMultiPage()
		{
			return new CarouselPage();
		}

		protected override ContentPage CreateContainedPage()
		{
			return new ContentPage { Content = new View() };
		}

		protected override int GetIndex(ContentPage page)
		{
			return CarouselPage.GetIndex(page);
		}

		[Fact]
		public void TestConstructor()
		{
			var page = new CarouselPage();
			Assert.Empty(page.Children);
		}
	}
}
