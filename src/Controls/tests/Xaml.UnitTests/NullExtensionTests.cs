// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[TestFixture]
	public class NullExtensionTests : BaseTestFixture
	{
		[Test]
		public void TestxNull()
		{
			var markupString = "{x:Null}";
			var serviceProvider = new Internals.XamlServiceProvider(null, null);
			var result = (new MarkupExtensionParser()).ParseExpression(ref markupString, serviceProvider);

			Assert.IsNull(result);
		}
	}
}
