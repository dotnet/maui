// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using NUnit.Framework;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class Gh7837VMBase //using a base class to test #2131
	{
		public string this[int index] => "";
		public string this[string index] => "";
	}

	public class Gh7837VM : Gh7837VMBase
	{
		public new string this[int index] => index == 42 ? "forty-two" : "dull number";
		public new string this[string index] => index.ToUpper();
	}

	public partial class Gh7837 : ContentPage
	{
		public Gh7837() => InitializeComponent();
		public Gh7837(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[SetUp] public void Setup() => Device.PlatformServices = new MockPlatformServices();
			[TearDown] public void TearDown() => Device.PlatformServices = null;

			[Test]
			public void BindingWithMultipleIndexers([Values(false, true)] bool useCompiledXaml)
			{
				if (useCompiledXaml)
					MockCompiler.Compile(typeof(Gh7837));
				var layout = new Gh7837(useCompiledXaml);
				Assert.That(layout.label0.Text, Is.EqualTo("forty-two"));
				Assert.That(layout.label1.Text, Is.EqualTo("FOO"));
				Assert.That(layout.label2.Text, Is.EqualTo("forty-two"));
				Assert.That(layout.label3.Text, Is.EqualTo("FOO"));
			}
		}
	}
}
