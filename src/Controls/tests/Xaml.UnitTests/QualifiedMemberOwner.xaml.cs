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

// A qualified Owner.Member attribute or property element ends up assigning a member of the target instance.
// The member has to be one the named owner declares or inherits, otherwise a base class or an interface would
// give access to everything the concrete target happens to declare.
public partial class QualifiedMemberOwner : ContentPage
{
	public QualifiedMemberOwner() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests : IDisposable
	{
		public Tests() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		public void Dispose() => DispatcherProvider.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void BaseOwnerResolvesItsOwnProperty(XamlInflator inflator)
		{
			var page = new QualifiedMemberOwner(inflator);

			Assert.Equal("from base owner", page.probe0.BaseOnly);
		}

		[Theory]
		[XamlInflatorData]
		internal void InterfaceOwnerResolvesItsOwnProperty(XamlInflator inflator)
		{
			var page = new QualifiedMemberOwner(inflator);

			Assert.Equal("from interface owner", page.probe1.InterfaceOnly);
		}

		[Theory]
		[XamlInflatorData]
		internal void BaseOwnerResolvesItsOwnPropertyFromPropertyElementSyntax(XamlInflator inflator)
		{
			var page = new QualifiedMemberOwner(inflator);

			Assert.Equal("from base owner property element", page.probe2.BaseOnly);
		}

		static string ProbeWith(string attribute) =>
			$"""
			<local:QualifiedMemberProbe xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
					xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
					xmlns:local="clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests;assembly=Microsoft.Maui.Controls.Xaml.UnitTests"
					{attribute} />
			""";

		[Fact]
		public void BaseOwnerCannotReachADerivedOnlyProperty()
		{
			var xaml = ProbeWith("local:QualifiedMemberProbeBase.DerivedOnly=\"wrong\"");
			var probe = new QualifiedMemberProbe();

			Assert.Throws<XamlParseException>(() => probe.LoadFromXaml(xaml));
			Assert.Null(probe.DerivedOnly);
		}

		[Fact]
		public void InterfaceOwnerCannotReachADerivedOnlyProperty()
		{
			var xaml = ProbeWith("local:IQualifiedMemberProbe.DerivedOnly=\"wrong\"");
			var probe = new QualifiedMemberProbe();

			Assert.Throws<XamlParseException>(() => probe.LoadFromXaml(xaml));
			Assert.Null(probe.DerivedOnly);
		}

		[Fact]
		public void InterfaceOwnerCannotReachABaseOnlyProperty()
		{
			// BaseOnly is declared by QualifiedMemberProbeBase, which IQualifiedMemberProbe knows nothing about
			var xaml = ProbeWith("local:IQualifiedMemberProbe.BaseOnly=\"wrong\"");
			var probe = new QualifiedMemberProbe();

			Assert.Throws<XamlParseException>(() => probe.LoadFromXaml(xaml));
			Assert.Null(probe.BaseOnly);
		}

		[Fact]
		public void UnrelatedOwnerCannotReachAPropertyOfTheTarget()
		{
			var xaml = ProbeWith("local:MockView.DerivedOnly=\"wrong\"");
			var probe = new QualifiedMemberProbe();

			Assert.Throws<XamlParseException>(() => probe.LoadFromXaml(xaml));
			Assert.Null(probe.DerivedOnly);
		}
	}
}
