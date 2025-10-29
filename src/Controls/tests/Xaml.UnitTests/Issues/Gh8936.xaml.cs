// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class Dict : Dictionary<string, string> { }
	public class Gh8936VM
	{
		public Dict Data { get; set; } = new Dict { { "Key", "Value" } };
	}

	public partial class Gh8936 : ContentPage
	{
		public Gh8936() => InitializeComponent();
		public Gh8936(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public class Tests
		{
			[Theory]
			[InlineData(false)]
			[InlineData(true)]
			public void Method(bool useCompiledXaml)
			{
				var layout = new Gh8936(useCompiledXaml) { BindingContext = new Gh8936VM() };
				Assert.Equal("Value", layout.entry0.Text);
				layout.entry0.Text = "Bar";
				Assert.Equal("Bar", layout.entry0.Text);
				Assert.Equal("Bar", ((Gh8936VM)layout.BindingContext).Data["Key"]);
			}
		}
	}
}
