#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Converters;

namespace Microsoft.Maui.Controls
{
	/// <summary>A BindableProperty is a backing store for properties allowing bindings on <see cref="T:Microsoft.Maui.Controls.BindableObject"/>.</summary>
	[DebuggerDisplay("{PropertyName}")]
	[System.ComponentModel.TypeConverter(typeof(BindablePropertyConverter))]
	public sealed class BindableProperty
	{
		internal const DynamicallyAccessedMemberTypes DeclaringTypeMembers = DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicMethods;
		internal const DynamicallyAccessedMemberTypes ReturnTypeMembers = DynamicallyAccessedMemberTypes.PublicParameterlessConstructor;

		public delegate void BindingPropertyChangedDelegate(BindableObject bindable, object oldValue, object newValue);

		public delegate void BindingPropertyChangedDelegate<in TPropertyType>(BindableObject bindable, TPropertyType oldValue, TPropertyType newValue);

		public delegate void BindingPropertyChangingDelegate(BindableObject bindable, object oldValue, object newValue);

		public delegate void BindingPropertyChangingDelegate<in TPropertyType>(BindableObject bindable, TPropertyType oldValue, TPropertyType newValue);

		public delegate object CoerceValueDelegate(BindableObject bindable, object value);

		public delegate TPropertyType CoerceValueDelegate<TPropertyType>(BindableObject bindable, TPropertyType value);

		public delegate object CreateDefaultValueDelegate(BindableObject bindable);

		public delegate TPropertyType CreateDefaultValueDelegate<in TDeclarer, out TPropertyType>(TDeclarer bindable);

		public delegate bool ValidateValueDelegate(BindableObject bindable, object value);

		public delegate bool ValidateValueDelegate<in TPropertyType>(BindableObject bindable, TPropertyType value);

		internal static readonly Dictionary<Type, TypeConverter> KnownTypeConverters = new Dictionary<Type, TypeConverter>
		{
			{ typeof(Uri), new UriTypeConverter() },
			{ typeof(Easing), new Maui.Converters.EasingTypeConverter() },
			{ typeof(Maui.Graphics.Color), new ColorTypeConverter() },
			{ typeof(ImageSource), new ImageSourceConverter() }
		};

		internal static readonly Dictionary<Type, IValueConverter> KnownIValueConverters = new Dictionary<Type, IValueConverter>
		{
			{ typeof(string), new ToStringValueConverter() },
		};

		// more or less the encoding of this, without the need to reflect
		// http://msdn.microsoft.com/en-us/library/y5b434w4.aspx
		internal static readonly Dictionary<Type, Type[]> SimpleConvertTypes = new Dictionary<Type, Type[]>
		{
			{ typeof(sbyte), new[] { typeof(string), typeof(short), typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal) } },
			{ typeof(byte), new[] { typeof(string), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(ulong), typeof(float), typeof(double), typeof(decimal) } },
			{ typeof(short), new[] { typeof(string), typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal) } },
			{ typeof(ushort), new[] { typeof(string), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) } },
			{ typeof(int), new[] { typeof(string), typeof(long), typeof(float), typeof(double), typeof(decimal) } },
			{ typeof(uint), new[] { typeof(string), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) } },
			{ typeof(long), new[] { typeof(string), typeof(float), typeof(double), typeof(decimal) } },
			{ typeof(char), new[] { typeof(string), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) } },
			{ typeof(float), new[] { typeof(string), typeof(double) } },
			{ typeof(ulong), new[] { typeof(string), typeof(float), typeof(double), typeof(decimal) } },
			{ typeof(double), new[] { typeof(string) } },
			{ typeof(bool), new[] { typeof(string) } },
		};

		/// <include file="../../docs/Microsoft.Maui.Controls/BindableProperty.xml" path="//Member[@MemberName='UnsetValue']/Docs/*" />
		public static readonly object UnsetValue = new object();

		BindableProperty(string propertyName, [DynamicallyAccessedMembers(ReturnTypeMembers)] Type returnType, [DynamicallyAccessedMembers(DeclaringTypeMembers)] Type declaringType, object defaultValue, BindingMode defaultBindingMode = BindingMode.OneWay,
								 ValidateValueDelegate validateValue = null, BindingPropertyChangedDelegate propertyChanged = null, BindingPropertyChangingDelegate propertyChanging = null,
								 CoerceValueDelegate coerceValue = null, BindablePropertyBindingChanging bindingChanging = null, bool isReadOnly = false, CreateDefaultValueDelegate defaultValueCreator = null)
		{
			if (propertyName == null)
				throw new ArgumentNullException(nameof(propertyName));
			if (returnType is null)
				throw new ArgumentNullException(nameof(returnType));
			if (declaringType is null)
				throw new ArgumentNullException(nameof(declaringType));

			// don't use Enum.IsDefined as its redonkulously expensive for what it does
			if (defaultBindingMode != BindingMode.Default && defaultBindingMode != BindingMode.OneWay && defaultBindingMode != BindingMode.OneWayToSource && defaultBindingMode != BindingMode.TwoWay && defaultBindingMode != BindingMode.OneTime)
				throw new ArgumentException($"Not a valid type of BindingMode. Property: {returnType} {declaringType.Name}.{propertyName}. Default binding mode: {defaultBindingMode}", nameof(defaultBindingMode));

			if (defaultValue == null && Nullable.GetUnderlyingType(returnType) == null && returnType.IsValueType)
				defaultValue = Activator.CreateInstance(returnType);

			if (defaultValue != null && !returnType.IsInstanceOfType(defaultValue))
				throw new ArgumentException($"Default value did not match return type. Property: {returnType} {declaringType.Name}.{propertyName} Default value type: {defaultValue.GetType().Name}, ", nameof(defaultValue));

			if (defaultBindingMode == BindingMode.Default)
				defaultBindingMode = BindingMode.OneWay;

			PropertyName = propertyName;
			ReturnType = returnType;
			DeclaringType = declaringType;
			DefaultValue = defaultValue;
			DefaultBindingMode = defaultBindingMode;
			PropertyChanged = propertyChanged;
			PropertyChanging = propertyChanging;
			ValidateValue = validateValue;
			CoerceValue = coerceValue;
			BindingChanging = bindingChanging;
			IsReadOnly = isReadOnly;
			DefaultValueCreator = defaultValueCreator;
		}

		/// <summary>Gets the type declaring the BindableProperty</summary>
		/// <remarks>Unused</remarks>
		[DynamicallyAccessedMembers(DeclaringTypeMembers)]
		public Type DeclaringType { get; private set; }

		/// <summary>Gets the default BindingMode.</summary>
		public BindingMode DefaultBindingMode { get; private set; }

		/// <summary>Gets the default value for the BindableProperty.</summary>
		public object DefaultValue { get; }

		/// <summary>Gets a value indicating if the BindableProperty is created form a BindablePropertyKey.</summary>
		public bool IsReadOnly { get; private set; }

		/// <summary>Gets the property name.</summary>
		public string PropertyName { get; }

		/// <summary>Gets the type of the BindableProperty.</summary>
		[DynamicallyAccessedMembers(ReturnTypeMembers)]
		public Type ReturnType { get; }

		internal BindablePropertyBindingChanging BindingChanging { get; private set; }

		internal CoerceValueDelegate CoerceValue { get; private set; }

		internal CreateDefaultValueDelegate DefaultValueCreator { get; }

		internal BindingPropertyChangedDelegate PropertyChanged { get; private set; }

		internal BindingPropertyChangingDelegate PropertyChanging { get; private set; }

		internal ValidateValueDelegate ValidateValue { get; private set; }

		/// <summary>Creates a new instance of the BindableProperty class.</summary>
		/// <param name="propertyName">The name of the BindableProperty.</param>
		/// <param name="returnType">The type of the property.</param>
		/// <param name="declaringType">The type of the declaring object.</param>
		/// <param name="defaultValue">The default value for the property.</param>
		/// <param name="defaultBindingMode">The BindingMode to use on SetBinding() if no BindingMode is given. This parameter is optional. Default is BindingMode.OneWay.</param>
		/// <param name="validateValue">A delegate to be run when a value is set. This parameter is optional. Default is null.</param>
		/// <param name="propertyChanged">A delegate to be run when the value has changed. This parameter is optional. Default is null.</param>
		/// <param name="propertyChanging">A delegate to be run when the value will change. This parameter is optional. Default is null.</param>
		/// <param name="coerceValue">A delegate used to coerce the range of a value. This parameter is optional. Default is null.</param>
		/// <param name="defaultValueCreator">A Func used to initialize default value for reference types.</param>
		/// <returns>A newly created BindableProperty.</returns>
		public static BindableProperty Create(string propertyName, [DynamicallyAccessedMembers(ReturnTypeMembers)] Type returnType, [DynamicallyAccessedMembers(DeclaringTypeMembers)] Type declaringType, object defaultValue = null, BindingMode defaultBindingMode = BindingMode.OneWay,
											  ValidateValueDelegate validateValue = null, BindingPropertyChangedDelegate propertyChanged = null, BindingPropertyChangingDelegate propertyChanging = null,
											  CoerceValueDelegate coerceValue = null, CreateDefaultValueDelegate defaultValueCreator = null)
		{
			return new BindableProperty(propertyName, returnType, declaringType, defaultValue, defaultBindingMode, validateValue, propertyChanged, propertyChanging, coerceValue,
				defaultValueCreator: defaultValueCreator);
		}

		/// <summary>Creates a new instance of the BindableProperty class for an attached property.</summary>
		/// <param name="propertyName">The name of the BindableProperty.</param>
		/// <param name="returnType">The type of the property.</param>
		/// <param name="declaringType">The type of the declaring object.</param>
		/// <param name="defaultValue">The default value for the property.</param>
		/// <param name="defaultBindingMode">The BindingMode to use on SetBinding() if no BindingMode is given. This parameter is optional. Default is BindingMode.OneWay.</param>
		/// <param name="validateValue">A delegate to be run when a value is set. This parameter is optional. Default is null.</param>
		/// <param name="propertyChanged">A delegate to be run when the value has changed. This parameter is optional. Default is null.</param>
		/// <param name="propertyChanging">A delegate to be run when the value will change. This parameter is optional. Default is null.</param>
		/// <param name="coerceValue">A delegate used to coerce the range of a value. This parameter is optional. Default is null.</param>
		/// <param name="defaultValueCreator">A Func used to initialize default value for reference types.</param>
		/// <returns>A newly created attached BindableProperty.</returns>
		public static BindableProperty CreateAttached(string propertyName, [DynamicallyAccessedMembers(ReturnTypeMembers)] Type returnType, [DynamicallyAccessedMembers(DeclaringTypeMembers)] Type declaringType, object defaultValue, BindingMode defaultBindingMode = BindingMode.OneWay,
													  ValidateValueDelegate validateValue = null, BindingPropertyChangedDelegate propertyChanged = null, BindingPropertyChangingDelegate propertyChanging = null,
													  CoerceValueDelegate coerceValue = null, CreateDefaultValueDelegate defaultValueCreator = null)
		{
			return CreateAttached(propertyName, returnType, declaringType, defaultValue, defaultBindingMode, validateValue, propertyChanged, propertyChanging, coerceValue, null, false, defaultValueCreator);
		}

		/// <summary>Creates a new instance of the BindableProperty class for attached read-only properties.</summary>
		/// <param name="propertyName">The name of the BindableProperty.</param>
		/// <param name="returnType">The type of the property.</param>
		/// <param name="declaringType">The type of the declaring object.</param>
		/// <param name="defaultValue">The default value for the property.</param>
		/// <param name="defaultBindingMode">The BindingMode to use on SetBinding() if no BindingMode is given. This parameter is optional. Default is BindingMode.OneWay.</param>
		/// <param name="validateValue">A delegate to be run when a value is set. This parameter is optional. Default is null.</param>
		/// <param name="propertyChanged">A delegate to be run when the value has changed. This parameter is optional. Default is null.</param>
		/// <param name="propertyChanging">A delegate to be run when the value will change. This parameter is optional. Default is null.</param>
		/// <param name="coerceValue">A delegate used to coerce the range of a value. This parameter is optional. Default is null.</param>
		/// <param name="defaultValueCreator">A Func used to initialize default value for reference types.</param>
		/// <returns>A newly created attached read-only BindableProperty.</returns>
		/// <remarks>Attached properties are bindable properties that are bound to an object other than their parent. Often, they are used for child items in tables and grids, where data about the location of an item is maintained by its parent, but must be accessed from the child item itself.</remarks>
		public static BindablePropertyKey CreateAttachedReadOnly(string propertyName, [DynamicallyAccessedMembers(ReturnTypeMembers)] Type returnType, [DynamicallyAccessedMembers(DeclaringTypeMembers)] Type declaringType, object defaultValue, BindingMode defaultBindingMode = BindingMode.OneWayToSource,
																 ValidateValueDelegate validateValue = null, BindingPropertyChangedDelegate propertyChanged = null, BindingPropertyChangingDelegate propertyChanging = null,
																 CoerceValueDelegate coerceValue = null, CreateDefaultValueDelegate defaultValueCreator = null)
		{
			return
				new BindablePropertyKey(CreateAttached(propertyName, returnType, declaringType, defaultValue, defaultBindingMode, validateValue, propertyChanged, propertyChanging, coerceValue, null, true,
					defaultValueCreator));
		}

		/// <summary>Creates a new instance of the BindablePropertyKey class.</summary>
		/// <param name="propertyName">The name of the BindableProperty.</param>
		/// <param name="returnType">The type of the property.</param>
		/// <param name="declaringType">The type of the declaring object.</param>
		/// <param name="defaultValue">The default value for the property.</param>
		/// <param name="defaultBindingMode">The BindingMode to use on SetBinding() if no BindingMode is given. This parameter is optional. Default is BindingMode.OneWay.</param>
		/// <param name="validateValue">A delegate to be run when a value is set. This parameter is optional. Default is null.</param>
		/// <param name="propertyChanged">A delegate to be run when the value has changed. This parameter is optional. Default is null.</param>
		/// <param name="propertyChanging">A delegate to be run when the value will change. This parameter is optional. Default is null.</param>
		/// <param name="coerceValue">A delegate used to coerce the range of a value. This parameter is optional. Default is null.</param>
		/// <param name="defaultValueCreator">A Func used to initialize default value for reference types.</param>
		public static BindablePropertyKey CreateReadOnly(string propertyName, [DynamicallyAccessedMembers(ReturnTypeMembers)] Type returnType, [DynamicallyAccessedMembers(DeclaringTypeMembers)] Type declaringType, object defaultValue, BindingMode defaultBindingMode = BindingMode.OneWayToSource,
														 ValidateValueDelegate validateValue = null, BindingPropertyChangedDelegate propertyChanged = null, BindingPropertyChangingDelegate propertyChanging = null,
														 CoerceValueDelegate coerceValue = null, CreateDefaultValueDelegate defaultValueCreator = null)
		{
			return
				new BindablePropertyKey(new BindableProperty(propertyName, returnType, declaringType, defaultValue, defaultBindingMode, validateValue, propertyChanged, propertyChanging, coerceValue,
					isReadOnly: true, defaultValueCreator: defaultValueCreator));
		}

		internal static BindableProperty Create(string propertyName, [DynamicallyAccessedMembers(ReturnTypeMembers)] Type returnType, [DynamicallyAccessedMembers(DeclaringTypeMembers)] Type declaringType, object defaultValue, BindingMode defaultBindingMode, ValidateValueDelegate validateValue,
												BindingPropertyChangedDelegate propertyChanged, BindingPropertyChangingDelegate propertyChanging, CoerceValueDelegate coerceValue, BindablePropertyBindingChanging bindingChanging,
												CreateDefaultValueDelegate defaultValueCreator = null)
		{
			return new BindableProperty(propertyName, returnType, declaringType, defaultValue, defaultBindingMode, validateValue, propertyChanged, propertyChanging, coerceValue, bindingChanging,
				defaultValueCreator: defaultValueCreator);
		}

		internal static BindableProperty CreateAttached(string propertyName, [DynamicallyAccessedMembers(ReturnTypeMembers)] Type returnType, [DynamicallyAccessedMembers(DeclaringTypeMembers)] Type declaringType, object defaultValue, BindingMode defaultBindingMode, ValidateValueDelegate validateValue,
														BindingPropertyChangedDelegate propertyChanged, BindingPropertyChangingDelegate propertyChanging, CoerceValueDelegate coerceValue, BindablePropertyBindingChanging bindingChanging,
														bool isReadOnly, CreateDefaultValueDelegate defaultValueCreator = null)
		{
			return new BindableProperty(propertyName, returnType, declaringType, defaultValue, defaultBindingMode, validateValue, propertyChanged, propertyChanging, coerceValue, bindingChanging, isReadOnly,
				defaultValueCreator);
		}

		internal object GetDefaultValue(BindableObject bindable)
		{
			if (DefaultValueCreator != null)
				return DefaultValueCreator(bindable);

			return DefaultValue;
		}

		internal bool TryConvert(ref object value)
		{
			Type returnType = ReturnType;

			if (value == null)
				return !returnType.IsValueType || returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Nullable<>);

			Type valueType = value.GetType();

			// already the same type, no need to convert
			if (returnType == valueType)
				return true;

			// Dont support arbitrary IConvertible by limiting which types can use this
			if (SimpleConvertTypes.TryGetValue(valueType, out Type[] convertibleTo) && Array.IndexOf(convertibleTo, returnType) != -1)
			{
				value = Convert.ChangeType(value, returnType);
				return true;
			}
			if (KnownTypeConverters.TryGetValue(returnType, out TypeConverter typeConverterTo) && typeConverterTo.CanConvertFrom(valueType))
			{
				value = typeConverterTo.ConvertFromInvariantString(value.ToString());
				return true;
			}
			if (returnType.IsAssignableFrom(valueType))
				return true;

			if (TypeConversionHelper.TryConvert(value, returnType, out var convertedValue))
			{
				value = convertedValue;
				return true;
			}
			if (KnownIValueConverters.TryGetValue(returnType, out IValueConverter valueConverter))
			{
				value = valueConverter.Convert(value, returnType, null, CultureInfo.CurrentUICulture);
				return true;
			}

			return false;
		}

		internal delegate void BindablePropertyBindingChanging(BindableObject bindable, BindingBase oldValue, BindingBase newValue);
	}
}
