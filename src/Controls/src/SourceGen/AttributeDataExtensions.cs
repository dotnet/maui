using System;

using Microsoft.CodeAnalysis;

namespace Microsoft.Maui.Controls.SourceGen;

static class AttributeDataExtensions
{
	public static bool IsInherited(this AttributeData attribute)
	{
		if (attribute.AttributeClass == null)
			return false;

		foreach (var attributeAttribute in attribute.AttributeClass.GetAttributes())
		{
			var @class = attributeAttribute.AttributeClass;
			if (@class != null && @class.Name == nameof(AttributeUsageAttribute) &&
				@class.ContainingNamespace?.Name == "System")
			{
				foreach (var kvp in attributeAttribute.NamedArguments)
				{
					if (kvp.Key == nameof(AttributeUsageAttribute.Inherited))
						return (bool)kvp.Value.Value!;
				}

				// Default value of Inherited is true
				return true;
			}
		}
		// An attribute without an `AttributeUsage` attribute will also default to being inherited.
		return true;
	}
}
