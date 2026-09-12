using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Maui.Controls.Xaml;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class BindablePropertyConverterTests : BaseTestFixture
{
	static readonly IXamlTypeResolver s_typeResolver = new Internals.XamlTypeResolver(
		new XmlNamespaceManager(new NameTable()),
		XamlParser.GetElementType,
		typeof(BindablePropertyConverterTests).Assembly);

	[Fact]
	public void TruncatedVisualStateParentChainsThrowXamlParseException()
	{
		AssertMissingTargetType(new Setter(), new VisualState());
		AssertMissingTargetType(new Setter(), new VisualState(), new VisualStateGroup());
		AssertMissingTargetType(new Setter(), new VisualState(), new VisualStateGroup(), new VisualStateGroupList());
		AssertMissingTargetType(new Setter(), new VisualState(), new VisualStateGroup(), new VisualStateGroupList(), new Setter());
	}

	[Fact]
	public void InvalidVisualStateGroupUsesSpecificError()
	{
		var exception = ConvertWithParents(new Setter(), new VisualState(), new object());

		Assert.Contains($"Expected {nameof(VisualStateGroup)}", exception.Message, StringComparison.Ordinal);
	}

	[Fact]
	public void InvalidVisualStateGroupListUsesSpecificError()
	{
		var exception = ConvertWithParents(new Setter(), new VisualState(), new VisualStateGroup(), new object());

		Assert.Contains($"Expected {nameof(VisualStateGroupList)}", exception.Message, StringComparison.Ordinal);
	}

	[Fact]
	public void InvalidSetterUsesSpecificError()
	{
		var exception = ConvertWithParents(new Setter(), new VisualState(), new VisualStateGroup(), new VisualStateGroupList(), new object());

		Assert.Contains($"Expected {nameof(Setter)}", exception.Message, StringComparison.Ordinal);
	}

	static void AssertMissingTargetType(params object[] parents)
	{
		var exception = ConvertWithParents(parents);

		Assert.Contains("Unable to find a TargetType", exception.Message, StringComparison.Ordinal);
	}

	static XamlParseException ConvertWithParents(params object[] parents)
	{
		var serviceProvider = new Internals.XamlServiceProvider
		{
			IXamlTypeResolver = s_typeResolver,
			IProvideValueTarget = new ParentValuesProvider(parents),
		};
		var converter = (IExtendedTypeConverter)new BindablePropertyConverter();

		return Assert.Throws<XamlParseException>(() => converter.ConvertFromInvariantString("Text", serviceProvider));
	}

	sealed class ParentValuesProvider : IProvideParentValues
	{
		public ParentValuesProvider(object[] parents)
		{
			ParentObjects = parents;
			TargetObject = parents[0];
		}

		public IEnumerable<object> ParentObjects { get; }

		public object TargetObject { get; }

		public object TargetProperty => null;
	}
}
