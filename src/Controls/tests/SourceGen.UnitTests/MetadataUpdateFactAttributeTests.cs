#nullable enable

using System;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

public class MetadataUpdateFactAttributeTests
{
	[Theory]
	[InlineData(true, null)]
	[InlineData(false, MetadataUpdateRequirements.SkipMessage)]
	public void FactAttribute_ReflectsRuntimeSupport(bool isSupported, string? expectedSkip)
	{
		var attribute = new TestMetadataUpdateFactAttribute(isSupported, modifiableAssemblies: null);
		Assert.Equal(expectedSkip, attribute.Skip);
	}

	[Theory]
	[InlineData("debug")]
	[InlineData("DEBUG")]
	[InlineData("not-debug")]
	public void FactAttribute_ThrowsWhenMetadataUpdatesWereExplicitlyRequestedButUnavailable(string modifiableAssemblies)
	{
		var exception = Assert.Throws<InvalidOperationException>(
			() => new TestMetadataUpdateFactAttribute(isSupported: false, modifiableAssemblies));
		Assert.Contains(MetadataUpdateRequirements.EnableVariable, exception.Message, StringComparison.Ordinal);
		Assert.Contains(modifiableAssemblies, exception.Message, StringComparison.Ordinal);
	}

	sealed class TestMetadataUpdateFactAttribute : FactAttribute
	{
		public TestMetadataUpdateFactAttribute(bool isSupported, string? modifiableAssemblies)
		{
			Skip = MetadataUpdateRequirements.GetSkipReasonOrThrowIfMisconfigured(isSupported, modifiableAssemblies);
		}
	}
}
