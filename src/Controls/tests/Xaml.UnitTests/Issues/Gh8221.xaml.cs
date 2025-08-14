// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Collections.Generic;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Gh8221VM
{
	public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string> { { "EntryOne", "One" }, { "EntryTwo", "Two" } };
}

public partial class Gh8221 : ContentPage
{
	public Gh8221() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void BindingWithMultipleIndexers([Values] XamlInflator inflator)
		{
			var layout = new Gh8221(inflator) { BindingContext = new Gh8221VM() };
			Assert.That(layout.entryone.Text, Is.EqualTo("One"));
			Assert.That(layout.entrytwo.Text, Is.EqualTo("Two"));
		}
	}
}
