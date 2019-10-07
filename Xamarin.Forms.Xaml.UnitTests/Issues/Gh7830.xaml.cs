// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class Gh7830 : ContentPage
	{
		public static string StaticText = "Foo";
		public Gh7830() => InitializeComponent();
		public Gh7830(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[SetUp] public void Setup() => Device.PlatformServices = new MockPlatformServices();
			[TearDown] public void TearDown() => Device.PlatformServices = null;

			[Test]
			public void CanResolvexStaticWithShortName([Values(false, true)]bool useCompiledXaml)
			{
				var layout = new Gh7830(useCompiledXaml);
				var cell = layout.listView.ItemTemplate.CreateContent() as ViewCell;
				Assert.That((cell.View as Label).Text, Is.EqualTo(StaticText));
			}
		}
	}
}
