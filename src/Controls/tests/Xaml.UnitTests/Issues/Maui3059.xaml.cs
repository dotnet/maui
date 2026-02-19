using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

// Note: This test file goes through source generation to verify MAUIX2015 warning
// The warning is tested in SourceGen.UnitTests/MultipleChildrenWarningTests.cs
// This file just verifies the XAML compiles successfully with warnings suppressed
public partial class Maui3059 : ContentPage
{
	public Maui3059()
	{
		InitializeComponent();
	}

	[TestFixture]
	class Tests
	{
		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
		}

		[TearDown]
		public void TearDown()
		{
			Application.SetCurrentApplication(null);
		}

		[Test]
		public void BorderWithMultipleChildren_CompileSucceeds()
		{
			// This test verifies that the XAML compiles successfully
			// The actual warning diagnostic is tested in SourceGen.UnitTests
			MockCompiler.Compile(typeof(Maui3059));
		}
	}
}
