#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Extension methods for working with <see cref="Setter"/> collections.
	/// </summary>
	public static class SettersExtensions
	{
		/// <summary>
		/// Adds a <see cref="Setter"/> with the specified property and value to the collection.
		/// </summary>
		/// <param name="setters">The setter collection.</param>
		/// <param name="property">The bindable property to set.</param>
		/// <param name="value">The value to apply.</param>
		public static void Add(this IList<Setter> setters, BindableProperty property, object value)
		{
			setters.Add(new Setter { Property = property, Value = value });
		}

		/// <summary>
		/// Adds a <see cref="Setter"/> with the specified property and binding to the collection.
		/// </summary>
		/// <param name="setters">The setter collection.</param>
		/// <param name="property">The bindable property to set.</param>
		/// <param name="binding">The binding to apply.</param>
		public static void AddBinding(this IList<Setter> setters, BindableProperty property, Binding binding)
		{
			if (binding == null)
				throw new ArgumentNullException(nameof(binding));

			setters.Add(new Setter { Property = property, Value = binding });
		}

		/// <summary>
		/// Adds a <see cref="Setter"/> with the specified property and dynamic resource key to the collection.
		/// </summary>
		/// <param name="setters">The setter collection.</param>
		/// <param name="property">The bindable property to set.</param>
		/// <param name="key">The dynamic resource key.</param>
		public static void AddDynamicResource(this IList<Setter> setters, BindableProperty property, string key)
		{
			if (string.IsNullOrEmpty(key))
				throw new ArgumentNullException(nameof(key));
			setters.Add(new Setter { Property = property, Value = new DynamicResource(key) });
		}
	}
}