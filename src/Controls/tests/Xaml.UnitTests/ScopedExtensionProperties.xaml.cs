// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
#nullable enable
using System;
using Controls.Xaml.UnitTests.ExternalAssembly.ScopedExtensions;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Xaml.UnitTests.ScopedExtensions;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

// An unqualified attribute name carries no xml namespace, so the scope it is resolved in is the default
// xmlns declared for the element. That is the same map type names are resolved through, so an extension
// container is in scope exactly when a type of its namespace would be.
public partial class ScopedExtensionProperties : ContentPage
{
	public ScopedExtensionProperties() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests : IDisposable
	{
		public Tests() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		public void Dispose() => DispatcherProvider.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void UnqualifiedExtensionPropertyFromTheDefaultXmlns(XamlInflator inflator)
		{
			var page = new ScopedExtensionProperties(inflator);

			Assert.Equal("from the default xmlns", page.label0.ScopedTag);
		}

		[Theory]
		[XamlInflatorData]
		internal void UnqualifiedExtensionPropertyFromAnotherAssembly(XamlInflator inflator)
		{
			var page = new ScopedExtensionProperties(inflator);

			Assert.Equal("from another assembly", page.label1.ExternalScopedTag);
		}

		[Theory]
		[XamlInflatorData]
		internal void InstancePropertyWinsOverExtensionProperty(XamlInflator inflator)
		{
			var page = new ScopedExtensionProperties(inflator);

			Assert.Equal("ordinary property wins", page.label2.Text);
			Assert.Null(page.label2.AutomationId);
		}

		[Theory]
		[XamlInflatorData]
		internal void MostSpecificReceiverWinsAcrossContainersInScope(XamlInflator inflator)
		{
			var page = new ScopedExtensionProperties(inflator);

			Assert.Equal("label:from xaml", page.label3.MostSpecific);
		}

		[Theory]
		[XamlInflatorData]
		internal void ExplicitContainerDisambiguates(XamlInflator inflator)
		{
			// ScopedAmbiguous alone is ambiguous, see ScopedExtensionPropertiesAmbiguous
			var page = new ScopedExtensionProperties(inflator);

			Assert.NotNull(page.label4);
		}
	}
}
