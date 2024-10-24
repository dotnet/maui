#nullable disable
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.StyleSheets;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="Type[@FullName='Microsoft.Maui.Controls.VisualElement']/Docs/*" />
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields)]
	public partial class VisualElement : IStylable
	{
		BindableProperty IStylable.GetProperty(string key, bool inheriting)
		{
			if (!Internals.Registrar.StyleProperties.TryGetValue(key, out var attrList))
				return null;

			StylePropertyAttribute styleAttribute = null;
			for (int i = 0; i < attrList.Count; i++)
			{
				styleAttribute = attrList[i];
				if (styleAttribute.TargetType.IsAssignableFrom(GetType()))
					break;
				styleAttribute = null;
			}

			if (styleAttribute == null)
				return null;

			//do not inherit non-inherited properties
			if (inheriting && !styleAttribute.Inherited)
				return null;

			if (styleAttribute.BindableProperty != null)
				return styleAttribute.BindableProperty;

			var propertyOwnerType = styleAttribute.PropertyOwnerType ?? GetType();
			var bpField = propertyOwnerType.GetField(styleAttribute.BindablePropertyName,
															  BindingFlags.Public
															| BindingFlags.NonPublic
															| BindingFlags.Static
															| BindingFlags.FlattenHierarchy);
			if (bpField == null)
				return null;

			return (styleAttribute.BindableProperty = bpField.GetValue(null) as BindableProperty);
		}
	}
}