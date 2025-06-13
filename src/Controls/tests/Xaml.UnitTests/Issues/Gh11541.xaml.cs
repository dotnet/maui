using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh11541 : ContentPage
	{
		public Gh11541() => InitializeComponent();
		public Gh11541(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		// [TestFixture] - removed for xUnit
		class Tests
		{
			[Fact]
			public void RectangleGeometryDoesntThrow([Values(false, true)] bool useCompiledXaml)
			{
				() => new Gh11541(useCompiledXaml)
			}
		}
	}
}
