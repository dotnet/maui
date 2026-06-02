#nullable disable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Defines the visual structure for templated items. Used to display data objects with a consistent appearance.
	/// </summary>
	public class DataTemplate : ElementTemplate, IDataTemplateController
	{
		static int idCounter = 100;

		int _id;
		string _idString;
		/// <summary>Initializes a new instance of the <see cref="DataTemplate"/> class.</summary>
		public DataTemplate()
		{
			_id = Interlocked.Increment(ref idCounter);
			_idString = GetType().FullName + _id;
		}

		/// <summary>Initializes a new instance of the <see cref="DataTemplate"/> class with the specified type.</summary>
		/// <param name="type">The type of object to be created by the template.</param>
		public DataTemplate(
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type type)
			: base(type)
		{
			_id = Interlocked.Increment(ref idCounter);
			_idString = type.FullName;
		}

		/// <summary>Initializes a new instance with a factory function that creates template content.</summary>
		/// <param name="loadTemplate">A factory function that returns the template content.</param>
		public DataTemplate(Func<object> loadTemplate) : base(loadTemplate)
		{
			_id = Interlocked.Increment(ref idCounter);
			_idString = GetType().FullName + _id;
		}

		/// <summary>Gets the dictionary of bindings to apply to templated items.</summary>
		public IDictionary<BindableProperty, BindingBase> Bindings { get; } = new Dictionary<BindableProperty, BindingBase>();

		/// <summary>Gets the dictionary of property values to apply to templated items.</summary>
		public IDictionary<BindableProperty, object> Values { get; } = new Dictionary<BindableProperty, object>();

		string IDataTemplateController.IdString => _idString;

		internal int Id => _id;

		int IDataTemplateController.Id => _id;

		/// <summary>Sets a binding for a property on templated items.</summary>
		/// <param name="property">The property to bind.</param>
		/// <param name="binding">The binding to apply.</param>
		public void SetBinding(BindableProperty property, BindingBase binding)
		{
			Values.Remove(property ?? throw new ArgumentNullException(nameof(property)));
			Bindings[property] = binding ?? throw new ArgumentNullException(nameof(binding));
		}

		/// <summary>Sets a static value for a property on templated items.</summary>
		/// <param name="property">The property to set.</param>
		/// <param name="value">The value to apply.</param>
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