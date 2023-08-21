// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Controls.Xaml.UnitTests.Issues.Maui14158;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.Maui14158;

public partial class WithSuffix : ContentPage
{
	public WithSuffix()
	{
		InitializeComponent();
	}

	public WithSuffix(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	[TestFixture]
	class Tests
	{
		[TestCase(true)]
		[TestCase(false)]
		public void VerifyCorrectTypesUsed(bool useCompiledXaml)
		{
			if (useCompiledXaml)
				Assert.DoesNotThrow(() => MockCompiler.Compile(typeof(WithSuffix)));

			var page = new WithSuffix(useCompiledXaml);

			Assert.IsInstanceOf<PublicWithSuffix>(page.publicWithSuffix);
			Assert.IsInstanceOf<InternalWithSuffix>(page.internalWithSuffix);
		}
	}
}
