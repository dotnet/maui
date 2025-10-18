using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class ShellNavigationStateTests : ShellTestBase
	{
		[Fact]
		public void LocationInitializedWithUri()
		{
			var uri = new Uri($"//test/IMPL_TEST/D_FAULT_TEST", UriKind.Relative);
			var uriState = new ShellNavigationState(uri);

			Assert.Equal("//test", uriState.Location.ToString());
			Assert.Equal("//test/IMPL_TEST/D_FAULT_TEST", uriState.FullLocation.ToString());
		}

		[Fact]
		public void LocationInitializedWithString()
		{
			var uri = new Uri("//test/IMPL_TEST/D_FAULT_TEST", UriKind.Relative);
			var strState = new ShellNavigationState(uri.ToString());

			Assert.Equal("//test", strState.Location.ToString());
			Assert.Equal("//test/IMPL_TEST/D_FAULT_TEST", strState.FullLocation.ToString());
		}
	}
}
