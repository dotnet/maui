// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class DefinitionCollectionTests : ContentPage
	{
		public DefinitionCollectionTests() => InitializeComponent();
		public DefinitionCollectionTests(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[SetUp] public void Setup() => Device.PlatformServices = new MockPlatformServices();
			[TearDown] public void TearDown() => Device.PlatformServices = null;

			[Test]
			public void DefinitionCollectionsParsedFromMarkup([Values(false, true)] bool useCompiledXaml)
			{
				var layout = new DefinitionCollectionTests(useCompiledXaml);
				var coldef = layout.grid.ColumnDefinitions;
				var rowdef = layout.grid.RowDefinitions;

				Assert.That(coldef.Count, Is.EqualTo(5));

				Assert.That(coldef[0].Width, Is.EqualTo(new GridLength(1, GridUnitType.Star)));
				Assert.That(coldef[1].Width, Is.EqualTo(new GridLength(2, GridUnitType.Star)));
				Assert.That(coldef[2].Width, Is.EqualTo(new GridLength(1, GridUnitType.Auto)));
				Assert.That(coldef[3].Width, Is.EqualTo(new GridLength(1, GridUnitType.Star)));
				Assert.That(coldef[4].Width, Is.EqualTo(new GridLength(300, GridUnitType.Absolute)));

				Assert.That(rowdef.Count, Is.EqualTo(5));
				Assert.That(rowdef[0].Height, Is.EqualTo(new GridLength(1, GridUnitType.Star)));
				Assert.That(rowdef[1].Height, Is.EqualTo(new GridLength(1, GridUnitType.Auto)));
				Assert.That(rowdef[2].Height, Is.EqualTo(new GridLength(25, GridUnitType.Absolute)));
				Assert.That(rowdef[3].Height, Is.EqualTo(new GridLength(14, GridUnitType.Absolute)));
				Assert.That(rowdef[4].Height, Is.EqualTo(new GridLength(20, GridUnitType.Absolute)));

			}
		}
	}
}