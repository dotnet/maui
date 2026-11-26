// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
#nullable enable
using System;
using Controls.Xaml.UnitTests.ExternalAssembly.ScopedExtensions;
using Microsoft.Maui.ApplicationModel;
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
		static readonly Lazy<Type> PageType = new(() => ExtensionPropertyTestHelper.Compile(typeof(ScopedExtensionProperties)));

		public Tests()
		{
			var application = new MockApplication();
			Application.SetCurrentApplication(application);
			application.Resources["applicationTag"] = "application";
			application.Resources["applicationImage"] = "application.png";
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
			Application.SetCurrentApplication(null!);
			DispatcherProvider.SetCurrent(null);
		}

		static ContentPage CreatePage() => (ContentPage)Activator.CreateInstance(PageType.Value)!;

		[Fact]
		public void ScopedExtensionProperties_UnqualifiedExtensionPropertyFromTheDefaultXmlns()
			=> Assert.Equal("from the default xmlns", CreatePage().Named<Label>("label0").ScopedTag);

		[Fact]
		public void ScopedExtensionProperties_UnqualifiedExtensionPropertyFromAnotherAssembly()
			=> Assert.Equal("from another assembly", CreatePage().Named<Label>("label1").ExternalScopedTag);

		[Fact]
		public void ScopedExtensionProperties_BindablePropertyWinsOverExtensionProperty()
		{
			var label = CreatePage().Named<Label>("label2");

			Assert.Equal("ordinary property wins", label.Text);
			Assert.Null(label.AutomationId);
		}

		[Fact]
		public void ScopedExtensionProperties_ClrPropertyWinsOverExtensionProperty()
		{
			var view = CreatePage().Named<ScopedPrecedenceView>("precedence");

			Assert.Equal("instance wins", view.PlainClr);
			Assert.Equal(string.Empty, ThreeWayRecorder.Get(view));
		}

		[Fact]
		public void ScopedExtensionProperties_ExplicitInterfaceImplementationDoesNotHideExtensionProperty()
			=> Assert.Equal("extension ExplicitTag:from xaml", ThreeWayRecorder.Get(CreatePage().Named<ScopedPrecedenceView>("explicitInterface")));

		[Fact]
		public void ScopedExtensionProperties_WritableExplicitInterfaceImplementationDoesNotHideExtensionProperty()
			=> Assert.Equal("extension WritableExplicitTag:from xaml", ThreeWayRecorder.Get(CreatePage().Named<ScopedPrecedenceView>("writableExplicitInterface")));

		[Fact]
		public void ScopedExtensionProperties_QualifiedResourceUsesExtensionPropertyType()
			=> Assert.Equal("extension Collision:42", ThreeWayRecorder.Get(CreatePage().Named<ScopedPrecedenceView>("qualifiedCollision")));

		[Fact]
		public void ScopedExtensionProperties_VariantReceiverIsMoreSpecificThanObject()
			=> Assert.Equal("variant:from xaml", ThreeWayRecorder.Get(CreatePage().Named<VariantExtensionView>("variant")));

		[Fact]
		public void ScopedExtensionProperties_SelectedSetterCannotBeHijackedByAnOverload()
		{
			var page = CreatePage();
			Assert.Equal("extension object:literal", ThreeWayRecorder.Get(page.Named<Label>("objectLiteral")));
			Assert.Equal("extension object:markup", ThreeWayRecorder.Get(page.Named<Label>("objectMarkup")));
			Assert.Equal("extension dynamic:literal", ThreeWayRecorder.Get(page.Named<Label>("dynamicLiteral")));
			Assert.Equal("extension dynamic:markup", ThreeWayRecorder.Get(page.Named<Label>("dynamicMarkup")));
		}

		[Fact]
		public void ScopedExtensionProperties_SourceDeclaredExtensionBlockIsSupported()
		{
			var page = (ContentPage)Activator.CreateInstance(ExtensionPropertyTestHelper.CompileSourceDeclaredProperty())!;
			Assert.Equal("SourceTag:String", page.Named<Label>("label").StyleId);
		}

		[Fact]
		public void ScopedExtensionProperties_MarkupExtensionReceivesTargetProperty()
			=> Assert.Equal("extension object:ObjectImpostor:Object", ThreeWayRecorder.Get(CreatePage().Named<Label>("objectTarget")));

		[Fact]
		public void ScopedExtensionProperties_PropertyConverterReceivesTargetForLiteralAndResources()
		{
			var page = CreatePage();
			Assert.Equal("converted:literal", ThreeWayRecorder.Get(page.Named<Label>("convertedLiteral")));
			Assert.Equal("converted:quoted \"value\" \\path", ThreeWayRecorder.Get(page.Named<Label>("convertedEscaped")));
			Assert.Equal("converted:from a resource", ThreeWayRecorder.Get(page.Named<Label>("convertedLocal")));
			Assert.Equal("converted:application", ThreeWayRecorder.Get(page.Named<Label>("convertedApplication")));
			Assert.Equal("converted:idiom", ThreeWayRecorder.Get(page.Named<Label>("convertedIdiom")));
		}

		[Fact]
		public void ScopedExtensionProperties_ApplicationResourceConvertsToImageSource()
			=> Assert.Equal("application.png", ThreeWayRecorder.Get(CreatePage().Named<Label>("imageApplication")));

		[Fact]
		public void ScopedExtensionProperties_OnIdiomInfersExtensionPropertyType()
			=> Assert.Equal("idiom", CreatePage().Named<Label>("idiom").ScopedTag);

		[Fact]
		public void ScopedExtensionProperties_AcceptsValueKnownAsObject()
			=> Assert.Equal("provided as object", CreatePage().Named<Label>("objectValue").ScopedTag);

		[Fact]
		public void ScopedExtensionProperties_UsingNamespaceSearchesReferencedAssemblies()
			=> Assert.Equal("from using", CreatePage().Named<Label>("usingNamespace").AutomationId);

		[Fact]
		public void ScopedExtensionProperties_NamespaceBeginningWithHttpIsInScope()
			=> Assert.Equal("from clr namespace", ThreeWayRecorder.Get(CreatePage().Named<Label>("httpClrNamespace")));

		[Fact]
		public void ScopedExtensionProperties_StaticResourceIsConvertedToTheExtensionPropertyType()
		{
			var page = CreatePage();

			Assert.Equal("from a resource", page.Named<Label>("label7").ScopedTag);
			Assert.Equal("from a resource", page.Named<Label>("label8").ScopedTag);
		}

		[Fact]
		public void ScopedExtensionProperties_HandwrittenMethodNamedLikeAnAccessorIsNotCalled()
			=> Assert.Equal("extension(View):from xaml", ThreeWayRecorder.Get(CreatePage().Named<Label>("label9")));

		[Fact]
		public void ScopedExtensionProperties_MostSpecificReceiverWinsAcrossContainersInScope()
			=> Assert.Equal("label:from xaml", CreatePage().Named<Label>("label3").MostSpecific);

		[Fact]
		public void ScopedExtensionProperties_MostSpecificReceiverWinsWhateverTheEnumerationOrder()
		{
			// ThreeWayA declares it for IView, ThreeWayB for BindableObject, ThreeWayC for Label. Neither
			// IView nor BindableObject is comparable with the other, and both come before the Label
			// candidate for ThreeWayLast and after it for ThreeWayFirst
			var page = CreatePage();

			Assert.Equal("C(Label):from xaml", ThreeWayRecorder.Get(page.Named<Label>("label5")));
			Assert.Equal("A(Label):from xaml", ThreeWayRecorder.Get(page.Named<Label>("label6")));
		}

		[Fact]
		public void ScopedExtensionProperties_ExplicitContainerDisambiguates()
		{
			// ScopedAmbiguous alone is ambiguous, see ScopedExtensionPropertiesErrors
			var page = CreatePage();

			Assert.Equal("A(IView):explicit wins", ThreeWayRecorder.Get(page.Named<Label>("label4")));
		}

		[Fact]
		public void ScopedExtensionProperties_OrdinaryQualifiedPropertyElementIsUnaffected()
		{
			var page = CreatePage();

			Assert.True(page.Resources.TryGetValue("legacyKey", out var value));
			Assert.Equal("legacy value", value);
		}
	}
}
