// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Dict : Dictionary<string, string> { }
public class Gh8936VM
{
	public Dict Data { get; set; } = new Dict { { "Key", "Value" } };
}

public partial class Gh8936 : ContentPage
{
	public Gh8936() => InitializeComponent();


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
		public void IndexerBindingOnSubclasses(XamlInflator inflator)
		{
			var layout = new Gh8936(inflator) { BindingContext = new Gh8936VM() };
			Assert.Equal("Value", layout.entry0.Text);
			layout.entry0.Text = "Bar";
			Assert.Equal("Bar", layout.entry0.Text);
			Assert.Equal("Bar", ((Gh8936VM)layout.BindingContext).Data["Key"]);
		}
	}
}
