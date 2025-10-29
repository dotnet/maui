using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;

using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui21757
{
	public Maui21757()
	{
		InitializeComponent();
	}

	public Maui21757(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}
	public class Test
	{
		// NOTE: xUnit uses constructor for setup. This may need manual conversion.
		// [SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void Method(bool useCompiledXaml)
		{
			var resourceDictionary = new Maui21757(useCompiledXaml);

			var styleA = resourceDictionary["A"] as Style;
			Assert.NotNull(styleA);
			Assert.Equal(typeof(BoxView), styleA.TargetType);
			Assert.Equal(BoxView.ColorProperty, styleA.Setters[0].Property);
			Assert.Equal(Color.FromArgb("#C8C8C8", styleA.Setters[0].Value));

			var styleB = resourceDictionary["B"] as Style;
			Assert.NotNull(styleB);
			Assert.Equal(typeof(BoxView), styleB.TargetType);
			Assert.Equal(BoxView.ColorProperty, styleB.Setters[0].Property);
			Assert.Equal(Color.FromArgb("#C8C8C8", styleB.Setters[0].Value));
		}
	}
}
