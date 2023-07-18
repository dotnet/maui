using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ElementTests : CoreHandlerTestBase
	{
		protected override MauiAppBuilder ConfigureBuilder(MauiAppBuilder mauiAppBuilder) =>
			base.ConfigureBuilder(mauiAppBuilder)
				.ConfigureMauiHandlers(handlers =>
					handlers.AddHandler(typeof(ElementStub), typeof(ElementHandlerStub)));

		[Fact]
		public void ElementToHandlerReturnsIElementHandler()
		{
			var handler = new ElementStub().ToHandler(MauiContext);
			Assert.NotNull(handler);
		}
	}
}
