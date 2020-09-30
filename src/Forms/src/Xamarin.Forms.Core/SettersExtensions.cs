using System;
using System.Collections.Generic;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	public static class SettersExtensions
	{
		public static void Add(this IList<Setter> setters, BindableProperty property, object value)
		{
			setters.Add(new Setter { Property = property, Value = value });
		}

		public static void AddBinding(this IList<Setter> setters, BindableProperty property, Binding binding)
		{
			if (binding == null)
				throw new ArgumentNullException("binding");

			setters.Add(new Setter { Property = property, Value = binding });
		}

		public static void AddDynamicResource(this IList<Setter> setters, BindableProperty property, string key)
		{
			if (string.IsNullOrEmpty(key))
				throw new ArgumentNullException("key");
			setters.Add(new Setter { Property = property, Value = new DynamicResource(key) });
		}
	}
}