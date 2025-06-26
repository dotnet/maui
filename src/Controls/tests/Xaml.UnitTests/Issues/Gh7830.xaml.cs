// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh7830 : ContentPage
	{
		public static string StaticText = "Foo";
		public Gh7830() => InitializeComponent();
		public Gh7830(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			[SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			[TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

			[Fact]
			public void CanResolvexStaticWithShortName([Values(false, true)] bool useCompiledXaml)
			{
				var layout = new Gh7830(useCompiledXaml);
				var cell = layout.listView.ItemTemplate.CreateContent() as ViewCell;
				Assert.Equal(StaticText, (cell.View as Label).Text);
			}
		}
	}
}
