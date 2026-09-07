// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
#nullable enable
using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Xaml.UnitTests.GlobalScopedExtensions;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

// No xmlns is declared at all, so the effective default xmlns is the MAUI global one, and the containers
// an [XmlnsDefinition] maps to it are in scope.
public partial class GlobalScopedExtensionProperties : ContentPage
{
	public GlobalScopedExtensionProperties() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests : IDisposable
	{
		public Tests()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose() => AppInfo.SetCurrent(null);

		[Fact]
		public void UnqualifiedExtensionPropertyFromTheGlobalXmlns()
			=> Assert.Equal("from the global xmlns", new GlobalScopedExtensionProperties().label.GlobalTag);
	}
}
