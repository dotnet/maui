// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Build.Tasks;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class xKeyLiteral : ContentPage
	{
		public xKeyLiteral() => InitializeComponent();
		public xKeyLiteral(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[SetUp] public void Setup() => Device.PlatformServices = new MockPlatformServices();
			[TearDown] public void TearDown() => Device.PlatformServices = null;

			[Test]
			//this requirement might change, see https://github.com/xamarin/Xamarin.Forms/issues/12425
			public void xKeyRequireStringLiteral([Values(false, true)] bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(xKeyLiteral)));
				else
					Assert.Throws<XamlParseException>(() => new xKeyLiteral(useCompiledXaml));
			}
		}
	}
}
