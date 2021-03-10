using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class ShellNavigationStateTests : ShellTestBase
	{
		[Test]
		public void LocationInitializedWithUri()
		{
			var uri = new Uri($"//test/IMPL_TEST/D_FAULT_TEST", UriKind.Relative);
			var uriState = new ShellNavigationState(uri);

			Assert.AreEqual("//test", uriState.Location.ToString());
			Assert.AreEqual("//test/IMPL_TEST/D_FAULT_TEST", uriState.FullLocation.ToString());
		}

		[Test]
		public void LocationInitializedWithString()
		{
			var uri = new Uri("//test/IMPL_TEST/D_FAULT_TEST", UriKind.Relative);
			var strState = new ShellNavigationState(uri.ToString());

			Assert.AreEqual("//test", strState.Location.ToString());
			Assert.AreEqual("//test/IMPL_TEST/D_FAULT_TEST", strState.FullLocation.ToString());
		}
	}
}
