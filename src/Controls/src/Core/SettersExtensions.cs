#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/SettersExtensions.xml" path="Type[@FullName='Microsoft.Maui.Controls.SettersExtensions']/Docs/*" />
	public static class SettersExtensions
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/SettersExtensions.xml" path="//Member[@MemberName='Add']/Docs/*" />
		public static void Add(this IList<Setter> setters, BindableProperty property, object value)
		{
			setters.Add(new Setter { Property = property, Value = value });
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/SettersExtensions.xml" path="//Member[@MemberName='AddBinding']/Docs/*" />
		public static void AddBinding(this IList<Setter> setters, BindableProperty property, Binding binding)
		{
			if (binding == null)
				throw new ArgumentNullException(nameof(binding));

			setters.Add(new Setter { Property = property, Value = binding });
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/SettersExtensions.xml" path="//Member[@MemberName='AddDynamicResource']/Docs/*" />
		public static void AddDynamicResource(this IList<Setter> setters, BindableProperty property, string key)
		{
			if (string.IsNullOrEmpty(key))
				throw new ArgumentNullException(nameof(key));
			setters.Add(new Setter { Property = property, Value = new DynamicResource(key) });
		}
	}
}