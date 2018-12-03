using System;
using System.Collections.Generic;
using System.Threading;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	public class DataTemplate : ElementTemplate, IDataTemplateController
	{
		static int idCounter = 1;

		int _id;
		string _idString;
		public DataTemplate()
		{
			_id = Interlocked.Increment(ref idCounter);
			_idString = GetType().FullName + _id;
		}

		public DataTemplate(Type type) : base(type)
		{
			_id = Interlocked.Increment(ref idCounter);
			_idString = type.FullName;
		}

		public DataTemplate(Func<object> loadTemplate) : base(loadTemplate)
		{
			_id = Interlocked.Increment(ref idCounter);
			_idString = GetType().FullName + _id;
		}

		public IDictionary<BindableProperty, BindingBase> Bindings { get; } = new Dictionary<BindableProperty, BindingBase>();

		public IDictionary<BindableProperty, object> Values { get; } = new Dictionary<BindableProperty, object>();

		string IDataTemplateController.IdString => _idString;

		int IDataTemplateController.Id => _id;

		public void SetBinding(BindableProperty property, BindingBase binding)
		{
			Values.Remove(property ?? throw new ArgumentNullException(nameof(property)));
			Bindings[property] = binding ?? throw new ArgumentNullException(nameof(binding));
		}

		public void SetValue(BindableProperty property, object value)
		{
			Bindings.Remove(property ?? throw new ArgumentNullException(nameof(property)));
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