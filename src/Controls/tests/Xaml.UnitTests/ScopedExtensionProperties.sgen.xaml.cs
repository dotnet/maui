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

		[Fact]
		public void UnqualifiedExtensionPropertyFromTheDefaultXmlns()
			=> Assert.Equal("from the default xmlns", new ScopedExtensionProperties().label0.ScopedTag);

		[Fact]
		public void UnqualifiedExtensionPropertyFromAnotherAssembly()
			=> Assert.Equal("from another assembly", new ScopedExtensionProperties().label1.ExternalScopedTag);

		[Fact]
		public void InstancePropertyWinsOverExtensionProperty()
		{
			var page = new ScopedExtensionProperties();

			Assert.Equal("ordinary property wins", page.label2.Text);
			Assert.Null(page.label2.AutomationId);
		}

		[Fact]
		public void MostSpecificReceiverWinsAcrossContainersInScope()
			=> Assert.Equal("label:from xaml", new ScopedExtensionProperties().label3.MostSpecific);

		[Fact]
		public void MostSpecificReceiverWinsWhateverTheEnumerationOrder()
		{
			// ThreeWayA declares it for IView, ThreeWayB for BindableObject, ThreeWayC for Label. Neither
			// IView nor BindableObject is comparable with the other, and both come before the Label
			// candidate for ThreeWayLast and after it for ThreeWayFirst
			var page = new ScopedExtensionProperties();

			Assert.Equal("C(Label):from xaml", ThreeWayRecorder.Get(page.label5));
			Assert.Equal("A(Label):from xaml", ThreeWayRecorder.Get(page.label6));
		}

		[Fact]
		public void ExplicitContainerDisambiguates()
		{
			// ScopedAmbiguous alone is ambiguous, see ScopedExtensionPropertiesErrors
			var page = new ScopedExtensionProperties();

			Assert.NotNull(page.label4);
		}

		[Fact]
		public void OrdinaryQualifiedPropertyElementIsUnaffected()
		{
			var page = new ScopedExtensionProperties();

			Assert.True(page.Resources.TryGetValue("legacyKey", out var value));
			Assert.Equal("legacy value", value);
		}
	}
}
