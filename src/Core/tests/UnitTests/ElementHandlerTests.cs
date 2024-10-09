using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.UnitTests.TestClasses;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Core.UnitTests
{

	[Category(TestCategory.Core)]
	public class ElementHandlerTests
	{
		[Fact]
		public void CreatePlatformElementUsingPlatformElementFactory()
		{
			ElementHandlerStub.PlatformElementFactory = handler => new CustomPlatformCell();

			var elementStub = new ElementHandlerStub();
			elementStub.SetVirtualView(new Microsoft.Maui.Controls.TextCell());

			Assert.NotNull(elementStub.PlatformView);
			Assert.True(elementStub.PlatformView is CustomPlatformCell);
		}

		[Fact]
		public void FactoryCanPuntAndUseOriginalType()
		{
			ElementHandlerStub.PlatformElementFactory = (h) => { return null; };

			var handlerStub = new ElementHandlerStub();
			handlerStub.SetVirtualView(new Microsoft.Maui.Controls.TextCell());

			Assert.NotNull(handlerStub.PlatformView);
			Assert.False(handlerStub.PlatformView is CustomPlatformCell);
			Assert.True(handlerStub.PlatformView is object);
		}
	}



	class CustomPlatformCell : object
	{

	}

}
