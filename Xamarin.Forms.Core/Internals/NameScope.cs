using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Xamarin.Forms.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class NameScope : INameScope
	{
		public static readonly BindableProperty NameScopeProperty =
			BindableProperty.CreateAttached("NameScope", typeof(INameScope), typeof(NameScope), default(INameScope));

		readonly Dictionary<string, object> _names = new Dictionary<string, object>();

		object INameScope.FindByName(string name)
			=> _names.TryGetValue(name, out var element) ? element : null;

		void INameScope.RegisterName(string name, object scopedElement)
		{
			if (_names.ContainsKey(name))
				throw new ArgumentException($"An element with the key '{name}' already exists in NameScope", nameof(name));

			_names[name] = scopedElement;
		}

		public static INameScope GetNameScope(BindableObject bindable) => (INameScope)bindable.GetValue(NameScopeProperty);

		public static void SetNameScope(BindableObject bindable, INameScope value)
		{
			if (bindable.GetValue(NameScopeProperty) == null)
				bindable.SetValue(NameScopeProperty, value);
		}
	}
}