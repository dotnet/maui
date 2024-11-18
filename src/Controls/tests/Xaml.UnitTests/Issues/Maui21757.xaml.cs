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
using NUnit.Framework;

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

	[TestFixture]
	class Test
	{
		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Test]
		public void TypeLiteralAndXTypeCanBeUsedInterchangeably([Values(false, true)] bool useCompiledXaml)
		{
			var resourceDictionary = new Maui21757(useCompiledXaml);

			var styleA = resourceDictionary["A"] as Style;
			Assert.NotNull(styleA);
			Assert.That(styleA.TargetType, Is.EqualTo(typeof(BoxView)));
			Assert.That(styleA.Setters[0].Property, Is.EqualTo(BoxView.ColorProperty));
			Assert.That(styleA.Setters[0].Value, Is.EqualTo(Color.FromArgb("#C8C8C8")));

			var styleB = resourceDictionary["B"] as Style;
			Assert.NotNull(styleB);
			Assert.That(styleB.TargetType, Is.EqualTo(typeof(BoxView)));
			Assert.That(styleB.Setters[0].Property, Is.EqualTo(BoxView.ColorProperty));
			Assert.That(styleB.Setters[0].Value, Is.EqualTo(Color.FromArgb("#C8C8C8")));
		}
	}
}
