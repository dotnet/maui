// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
#nullable enable
using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public interface IQualifiedMemberProbe
{
	string? InterfaceOnly { get; set; }
}

public class QualifiedMemberProbeBase : View
{
	public string? BaseOnly { get; set; }
}

public class QualifiedMemberProbe : QualifiedMemberProbeBase, IQualifiedMemberProbe
{
	public string? DerivedOnly { get; set; }

	public string? InterfaceOnly { get; set; }
}

// XAML has always resolved a qualified Owner.Member against the target instance, using the owner only to
// find an attached bindable property. When there is none, the qualifier is ignored, even when the owner is
// unrelated to the target: <ContentView.Resources> on a ContentPage is used in the wild and has to keep
// working. Extension containers are the single exception, see ExtensionPropertiesQualifiedFallback.
public partial class QualifiedMemberLegacy : ContentPage
{
	public QualifiedMemberLegacy() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests : IDisposable
	{
		public Tests() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		public void Dispose() => DispatcherProvider.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void UnrelatedOwnerResolvesTheMemberOnTheTarget(XamlInflator inflator)
		{
			var page = new QualifiedMemberLegacy(inflator);

			Assert.True(page.Resources.TryGetValue("legacyKey", out var value));
			Assert.Equal("legacy value", value);
		}

		[Theory]
		[XamlInflatorData]
		internal void BaseOwnerResolvesItsOwnProperty(XamlInflator inflator)
		{
			var page = new QualifiedMemberLegacy(inflator);

			Assert.Equal("from base owner", page.probe0.BaseOnly);
		}

		[Theory]
		[XamlInflatorData]
		internal void InterfaceOwnerResolvesItsOwnProperty(XamlInflator inflator)
		{
			var page = new QualifiedMemberLegacy(inflator);

			Assert.Equal("from interface owner", page.probe1.InterfaceOnly);
		}

		[Theory]
		[XamlInflatorData]
		internal void OwnerThatDeclaresNothingIsIgnored(XamlInflator inflator)
		{
			var page = new QualifiedMemberLegacy(inflator);

			Assert.Equal("qualifier is ignored", page.probe2.DerivedOnly);
		}

		[Theory]
		[XamlInflatorData]
		internal void BaseOwnerResolvesItsOwnPropertyFromPropertyElementSyntax(XamlInflator inflator)
		{
			var page = new QualifiedMemberLegacy(inflator);

			Assert.Equal("from base owner property element", page.probe3.BaseOnly);
		}
	}
}
