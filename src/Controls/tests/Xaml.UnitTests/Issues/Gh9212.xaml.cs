// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[ContentProperty(nameof(Text))]
	[AcceptEmptyServiceProvider]
	public class Gh9212MarkupExtension : IMarkupExtension
	{
		public string Text { get; set; }
		public object ProvideValue(IServiceProvider serviceProvider) => Text;
	}

	public partial class Gh9212 : ContentPage
	{
		public Gh9212() => InitializeComponent();
		public Gh9212(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[Test]
			public void SingleQuoteAndTrailingSpaceInMarkupValue([Values(false, true)] bool useCompiledXaml)
			{
				var layout = new Gh9212(useCompiledXaml);
				Assert.That(layout.label.Text, Is.EqualTo("Foo, Bar"));
			}
		}
	}
}
