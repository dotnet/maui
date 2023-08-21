// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ElementTests : CoreHandlerTestBase
	{
		[Fact]
		public void ElementToHandlerReturnsIElementHandler()
		{
			var handler = new ElementStub().ToHandler(MauiContext);
			Assert.NotNull(handler);
		}
	}
}
