#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Microsoft.Maui.Controls.Internals
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/NameScope.xml" path="Type[@FullName='Microsoft.Maui.Controls.Internals.NameScope']/Docs/*" />
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class NameScope : INameScope
	{
		/// <summary>Bindable property for <see cref="NameScope"/>.</summary>
		public static readonly BindableProperty NameScopeProperty =
			BindableProperty.CreateAttached("NameScope", typeof(INameScope), typeof(NameScope), default(INameScope));

		readonly Dictionary<string, object> _names = new(StringComparer.Ordinal);
		readonly Dictionary<object, string> _values = new Dictionary<object, string>();

		object INameScope.FindByName(string name)
		{
			if (!RuntimeFeature.AreNamescopesSupported)
				throw new NotSupportedException("Namescopes are not supported. Please enable the feature switch 'Microsoft.Maui.RuntimeFeature.AreNamescopesSupported' to keep using namescopes.");

			return _names.TryGetValue(name, out var element) ? element : null;
		}

		void INameScope.RegisterName(string name, object scopedElement)
		{
			if (!RuntimeFeature.AreNamescopesSupported)
				throw new NotSupportedException("Namescopes are not supported. Please enable the feature switch 'Microsoft.Maui.RuntimeFeature.AreNamescopesSupported' to keep using namescopes.");

			if (_names.ContainsKey(name))
				throw new ArgumentException($"An element with the key '{name}' already exists in NameScope", nameof(name));

			_names[name] = scopedElement;
			_values[scopedElement] = name;
		}

		//used by VS Live Visual Tree
		internal string NameOf(object scopedObject)
		{
			if (!RuntimeFeature.AreNamescopesSupported)
				throw new NotSupportedException("Namescopes are not supported. Please enable the feature switch 'Microsoft.Maui.RuntimeFeature.AreNamescopesSupported' to keep using namescopes.");

			return _values.TryGetValue(scopedObject, out var name) ? name : null;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/NameScope.xml" path="//Member[@MemberName='GetNameScope']/Docs/*" />
		public static INameScope GetNameScope(BindableObject bindable)
		{
			if (!RuntimeFeature.AreNamescopesSupported)
				throw new NotSupportedException("Namescopes are not supported. Please enable the feature switch 'Microsoft.Maui.RuntimeFeature.AreNamescopesSupported' to keep using namescopes.");

			return (INameScope)bindable.GetValue(NameScopeProperty);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/NameScope.xml" path="//Member[@MemberName='SetNameScope']/Docs/*" />
		public static void SetNameScope(BindableObject bindable, INameScope value)
		{
			if (!RuntimeFeature.AreNamescopesSupported)
				throw new NotSupportedException("Namescopes are not supported. Please enable the feature switch 'Microsoft.Maui.RuntimeFeature.AreNamescopesSupported' to keep using namescopes.");

			if (bindable.GetValue(NameScopeProperty) == null)
				bindable.SetValue(NameScopeProperty, value);
		}

		void INameScope.UnregisterName(string name)
		{
			if (!RuntimeFeature.AreNamescopesSupported)
				throw new NotSupportedException("Namescopes are not supported. Please enable the feature switch 'Microsoft.Maui.RuntimeFeature.AreNamescopesSupported' to keep using namescopes.");

			if (name == null)
				throw new ArgumentNullException(nameof(name));

			if (name == "")
				throw new ArgumentException("name was provided as empty string.", nameof(name));

			if (!_names.ContainsKey(name))
				throw new ArgumentException("name provided had not been registered.", nameof(name));

			_values.Remove(_names[name]);
			_names.Remove(name);
		}
	}
}
