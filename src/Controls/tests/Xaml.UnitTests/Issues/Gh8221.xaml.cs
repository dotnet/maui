// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Collections.Generic;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Gh8221VM
{
	public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string> { { "EntryOne", "One" }, { "EntryTwo", "Two" } };
}

public partial class Gh8221 : ContentPage
{
	public Gh8221() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void BindingWithMultipleIndexers(XamlInflator inflator)
		{
			var layout = new Gh8221(inflator) { BindingContext = new Gh8221VM() };
			Assert.Equal("One", layout.entryone.Text);
			Assert.Equal("Two", layout.entrytwo.Text);
		}
	}
}
