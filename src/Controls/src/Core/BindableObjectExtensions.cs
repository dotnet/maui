#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/BindableObjectExtensions.xml" path="Type[@FullName='Microsoft.Maui.Controls.BindableObjectExtensions']/Docs/*" />
	public static class BindableObjectExtensions
	{
		internal static void RefreshPropertyValue(this BindableObject self, BindableProperty property, object value)
		{
			var ctx = self.GetContext(property);
			if (ctx != null && ctx.Bindings.Count > 0)
			{
				var binding = ctx.Bindings.GetValue();

				// support bound properties
				if (!ctx.Attributes.HasFlag(BindableObject.BindableContextAttributes.IsBeingSet))
					binding.Apply(false);
			}
			else
			{
				// support normal/code properties
				self.SetValue(property, value, SetterSpecificity.FromHandler);
			}
		}

		internal static void PropagateBindingContext<T>(this BindableObject self, IEnumerable<T> children)
			=> PropagateBindingContext(self, children, BindableObject.SetInheritedBindingContext);

		internal static void PropagateBindingContext<T>(this BindableObject self, IEnumerable<T> children, Action<BindableObject, object> setChildBindingContext)
		{
			if (children == null)
				return;

			var bc = self.BindingContext;

			foreach (var child in children)
			{
				var bo = child as BindableObject;
				if (bo == null)
					continue;

				setChildBindingContext(bo, bc);
			}
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/BindableObjectExtensions.xml" path="//Member[@MemberName='SetBinding']/Docs/*" />
		[RequiresUnreferencedCode(TrimmerConstants.StringPathBindingWarning, Url = TrimmerConstants.ExpressionBasedBindingsDocsUrl)]
		public static void SetBinding(this BindableObject self, BindableProperty targetProperty, string path, BindingMode mode = BindingMode.Default, IValueConverter converter = null,
									  string stringFormat = null)
		{
			if (self == null)
				throw new ArgumentNullException(nameof(self));
			if (targetProperty == null)
				throw new ArgumentNullException(nameof(targetProperty));

			var binding = new Binding(path, mode, converter, stringFormat: stringFormat);
			self.SetBinding(targetProperty, binding);
		}

#nullable enable
		/// <summary>
		/// Creates a binding between a property on the source object and a property on the target object.
		/// </summary>
		/// <remarks>
		///   <para>The following example illustrates the setting of a binding using the extension method.</para>
		///   <example>
		///     <code lang="csharp lang-csharp"><![CDATA[
		///      public class PersonViewModel
		///      {
		///          public string Name { get; set; }
		///          public Address? Address { get; set; }
		///          // ...
		///      }
		///      
		///      var vm = new PersonViewModel { Name = "John Doe" };
		///      
		///      var label = new Label();
		///      label.SetBinding(Label.TextProperty, static (PersonViewModel vm) => vm.Name);
		///      label.BindingContext = vm;
		///      
		///      vm.Name = "Jane Doe";
		///      Debug.WriteLine(label.Text); // prints "Jane Doe"
		///  ]]></code>
		///   </example>
		///   <para>Not all methods can be used to define a binding. The expression must be a simple property access expression. The following are examples of valid and invalid expressions:</para>
		///   <example>
		///     <code lang="csharp lang-csharp"><![CDATA[
		///      // Valid: Property access
		///      static (PersonViewModel vm) => vm.Name;
		///      static (PersonViewModel vm) => vm.Address?.Street;
		///      
		///      // Valid: Array and indexer access
		///      static (PersonViewModel vm) => vm.PhoneNumbers[0];
		///      static (PersonViewModel vm) => vm.Config["Font"];
		///      
		///      // Valid: Casts
		///      static (Label label) => (label.BindingContext as PersonViewModel).Name;
		///      static (Label label) => ((PersonViewModel)label.BindingContext).Name;
		///      
		///      // Invalid: Method calls
		///      static (PersonViewModel vm) => vm.GetAddress();
		///      static (PersonViewModel vm) => vm.Address?.ToString();
		///      
		///      // Invalid: Complex expressions
		///      static (PersonViewModel vm) => vm.Address?.Street + " " + vm.Address?.City;
		///      static (PersonViewModel vm) => $"Name: {vm.Name}";
		///  ]]></code>
		///   </example>
		/// </remarks>
		/// <typeparam name="TSource">The source type.</typeparam>
		/// <typeparam name="TProperty">The property type.</typeparam>
		/// <param name="self">The <see cref="T:Microsoft.Maui.Controls.BindableObject" />.</param>
		/// <param name="targetProperty">The <see cref="T:Microsoft.Maui.Controls.BindableProperty" /> on which to set a binding.</param>
		/// <param name="getter">An getter method used to retrieve the source property.</param>
		/// <param name="mode">The binding mode. This property is optional. Default is <see cref="F:Microsoft.Maui.Controls.BindingMode.Default" />.</param>
		/// <param name="converter">The converter. This parameter is optional. Default is <see langword="null" />.</param>
		/// <param name="converterParameter">An user-defined parameter to pass to the converter. This parameter is optional. Default is <see langword="null" />.</param>
		/// <param name="stringFormat">A String format. This parameter is optional. Default is <see langword="null" />.</param>
		/// <param name="source">An object used as the source for this binding. This parameter is optional. Default is <see langword="null" />.</param>
		/// <param name="fallbackValue">The value to use instead of the default value for the property, if no specified value exists.</param>
		/// <param name="targetNullValue">The value to supply for a bound property when the target of the binding is <see langword="null" />.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public static void SetBinding<TSource, TProperty>(
			this BindableObject self,
			BindableProperty targetProperty,
			Func<TSource, TProperty> getter,
			BindingMode mode = BindingMode.Default,
			IValueConverter? converter = null,
			object? converterParameter = null,
			string? stringFormat = null,
			object? source = null,
			object? fallbackValue = null,
			object? targetNullValue = null)
		{
			if (!RuntimeFeature.AreBindingInterceptorsSupported)
			{
				throw new InvalidOperationException($"Call to SetBinding<{typeof(TSource)}, {typeof(TProperty)}> could not be intercepted because the feature has been disabled. Consider removing the DisableMauiAnalyzers property from your project file or set the _MauiBindingInterceptorsSupport property to true instead.");
			}

			throw new InvalidOperationException($"Call to SetBinding<{typeof(TSource)}, {typeof(TProperty)}> was not intercepted.");
		}
#nullable disable

		public static T GetPropertyIfSet<T>(this BindableObject bindableObject, BindableProperty bindableProperty, T returnIfNotSet)
		{
			if (bindableObject == null)
				return returnIfNotSet;

			if (bindableObject.IsSet(bindableProperty))
				return (T)bindableObject.GetValue(bindableProperty);

			return returnIfNotSet;
		}

		internal static bool TrySetDynamicThemeColor(
			this BindableObject bindableObject,
			string resourceKey,
			BindableProperty bindableProperty,
			out object outerColor)
		{
			if (Application.Current.TryGetResource(resourceKey, out outerColor))
			{
				bindableObject.SetDynamicResource(bindableProperty, resourceKey);
				return true;
			}

			return false;
		}

		internal static void AddRemoveLogicalChildren(this BindableObject bindable, object oldValue, object newValue)
		{
			if (!(bindable is Element owner))
				return;

			if (oldValue is Element oldView)
				owner.RemoveLogicalChild(oldView);

			if (newValue is Element newView)
				owner.AddLogicalChild(newView);
		}

		internal static bool TrySetAppTheme(
			this BindableObject self,
			string lightResourceKey,
			string darkResourceKey,
			BindableProperty bindableProperty,
			Brush defaultDark,
			Brush defaultLight,
			out object outerLight,
			out object outerDark)
		{
			if (!Application.Current.TryGetResource(lightResourceKey, out outerLight))
			{
				outerLight = defaultLight;
			}

			if (!Application.Current.TryGetResource(darkResourceKey, out outerDark))
			{
				outerDark = defaultDark;
			}

			self.SetAppTheme(bindableProperty, outerLight, outerDark);
			return (Brush)outerLight != defaultLight || (Brush)outerDark != defaultDark;
		}

		public static void SetAppTheme<T>(this BindableObject self, BindableProperty targetProperty, T light, T dark) => self.SetBinding(targetProperty, new AppThemeBinding { Light = light, Dark = dark });

		/// <include file="../../docs/Microsoft.Maui.Controls/BindableObjectExtensions.xml" path="//Member[@MemberName='SetAppThemeColor']/Docs/*" />
		public static void SetAppThemeColor(this BindableObject self, BindableProperty targetProperty, Color light, Color dark)
			=> SetAppTheme(self, targetProperty, light, dark);
	}
}
