// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

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


	public class Tests : IDisposable
	{
		public Tests()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
			DispatcherProvider.SetCurrent(null);
			Application.SetCurrentApplication(null);
		}
		[Theory]
		[Values]
		public void SingleQuoteAndTrailingSpaceInMarkupValue(XamlInflator inflator)
		{
			var layout = new Gh9212(inflator);
			Assert.Equal("Foo, Bar", layout.label.Text);
		}
	}
}
