using System.Collections.Generic;
using System.Reflection;

using Xamarin.Forms.Internals;
using Xamarin.Forms.StyleSheets;

namespace Xamarin.Forms
{
	public partial class VisualElement : IStyleSelectable, IStylable
	{
		IList<string> IStyleSelectable.Classes
			=> StyleClass;

		BindableProperty IStylable.GetProperty(string key, bool inheriting)
		{
			StylePropertyAttribute styleAttribute;
			if (!Internals.Registrar.StyleProperties.TryGetValue(key, out styleAttribute))
				return null;

			if (!styleAttribute.TargetType.GetTypeInfo().IsAssignableFrom(GetType().GetTypeInfo()))
				return null;

			//do not inherit non-inherited properties
			if (inheriting && !styleAttribute.Inherited)
				return null;

			if (styleAttribute.BindableProperty != null)
				return styleAttribute.BindableProperty;

			var propertyOwnerType = styleAttribute.PropertyOwnerType ?? GetType();
#if NETSTANDARD1_0
			var bpField = propertyOwnerType.GetField(styleAttribute.BindablePropertyName);
#else
			var bpField = propertyOwnerType.GetField(styleAttribute.BindablePropertyName,
															  BindingFlags.Public
															| BindingFlags.NonPublic
															| BindingFlags.Static
															| BindingFlags.FlattenHierarchy);
#endif
			if (bpField == null)
				return null;

			return (styleAttribute.BindableProperty = bpField.GetValue(null) as BindableProperty);
		}

		void ApplyStyleSheets()
		{
			foreach (var styleSheet in this.GetStyleSheets())
				((IStyle)styleSheet).Apply(this);
		}
	}
}