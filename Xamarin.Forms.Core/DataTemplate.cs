using System;
using System.Collections.Generic;

namespace Xamarin.Forms
{
	public class DataTemplate : ElementTemplate
	{
		public DataTemplate()
		{
		}

		public DataTemplate(Type type) : base(type)
		{
		}

		public DataTemplate(Func<object> loadTemplate) : base(loadTemplate)
		{
		}

		public IDictionary<BindableProperty, BindingBase> Bindings { get; } = new Dictionary<BindableProperty, BindingBase>();

		public IDictionary<BindableProperty, object> Values { get; } = new Dictionary<BindableProperty, object>();

		public void SetBinding(BindableProperty property, BindingBase binding)
		{
			if (property == null)
				throw new ArgumentNullException("property");
			if (binding == null)
				throw new ArgumentNullException("binding");

			Values.Remove(property);
			Bindings[property] = binding;
		}

		public void SetValue(BindableProperty property, object value)
		{
			if (property == null)
				throw new ArgumentNullException("property");

			Bindings.Remove(property);
			Values[property] = value;
		}

		internal override void SetupContent(object item)
		{
			ApplyBindings(item);
			ApplyValues(item);
		}

		void ApplyBindings(object item)
		{
			if (Bindings == null)
				return;

			var bindable = item as BindableObject;
			if (bindable == null)
				return;

			foreach (KeyValuePair<BindableProperty, BindingBase> kvp in Bindings)
			{
				if (Values.ContainsKey(kvp.Key))
					throw new InvalidOperationException("Binding and Value found for " + kvp.Key.PropertyName);

				bindable.SetBinding(kvp.Key, kvp.Value.Clone());
			}
		}

		void ApplyValues(object item)
		{
			if (Values == null)
				return;

			var bindable = item as BindableObject;
			if (bindable == null)
				return;
			foreach (KeyValuePair<BindableProperty, object> kvp in Values)
				bindable.SetValue(kvp.Key, kvp.Value);
		}
	}
}