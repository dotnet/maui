#nullable disable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/DataTemplate.xml" path="Type[@FullName='Microsoft.Maui.Controls.DataTemplate']/Docs/*" />
	public class DataTemplate : ElementTemplate, IDataTemplateController
	{
		static int idCounter = 100;

		int _id;
		string _idString;
		/// <include file="../../docs/Microsoft.Maui.Controls/DataTemplate.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public DataTemplate()
		{
			_id = Interlocked.Increment(ref idCounter);
			_idString = GetType().FullName + _id;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/DataTemplate.xml" path="//Member[@MemberName='.ctor'][3]/Docs/*" />
		public DataTemplate(
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type type)
			: base(type)
		{
			_id = Interlocked.Increment(ref idCounter);
			_idString = type.FullName;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/DataTemplate.xml" path="//Member[@MemberName='.ctor'][2]/Docs/*" />
		public DataTemplate(Func<object> loadTemplate) : base(loadTemplate)
		{
			_id = Interlocked.Increment(ref idCounter);
			_idString = GetType().FullName + _id;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/DataTemplate.xml" path="//Member[@MemberName='Bindings']/Docs/*" />
		public IDictionary<BindableProperty, BindingBase> Bindings { get; } = new Dictionary<BindableProperty, BindingBase>();

		/// <include file="../../docs/Microsoft.Maui.Controls/DataTemplate.xml" path="//Member[@MemberName='Values']/Docs/*" />
		public IDictionary<BindableProperty, object> Values { get; } = new Dictionary<BindableProperty, object>();

		string IDataTemplateController.IdString => _idString;

		internal int Id => _id;

		int IDataTemplateController.Id => _id;

		/// <include file="../../docs/Microsoft.Maui.Controls/DataTemplate.xml" path="//Member[@MemberName='SetBinding']/Docs/*" />
		public void SetBinding(BindableProperty property, BindingBase binding)
		{
			Values.Remove(property ?? throw new ArgumentNullException(nameof(property)));
			Bindings[property] = binding ?? throw new ArgumentNullException(nameof(binding));
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/DataTemplate.xml" path="//Member[@MemberName='SetValue']/Docs/*" />
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