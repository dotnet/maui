using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls
{
	[DebuggerDisplay("{PropertyName}")]
	[TypeConverter(typeof(BindablePropertyConverter))]
	public sealed class BindableProperty
	{
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

		static readonly Dictionary<Type, TypeConverter> KnownTypeConverters = new Dictionary<Type, TypeConverter>
		{
			{ typeof(Uri), new UriTypeConverter() },
			{ typeof(Color), new ColorTypeConverter() },
			{ typeof(Easing), new EasingTypeConverter() },
		};

		static readonly Dictionary<Type, IValueConverter> KnownIValueConverters = new Dictionary<Type, IValueConverter>
		{
			{ typeof(string), new ToStringValueConverter() },
		};

		// more or less the encoding of this, without the need to reflect
		// http://msdn.microsoft.com/en-us/library/y5b434w4.aspx
		static readonly Dictionary<Type, Type[]> SimpleConvertTypes = new Dictionary<Type, Type[]>
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
		};

		public static readonly object UnsetValue = new object();

		BindableProperty(string propertyName, Type returnType, Type declaringType, object defaultValue, BindingMode defaultBindingMode = BindingMode.OneWay,
								 ValidateValueDelegate validateValue = null, BindingPropertyChangedDelegate propertyChanged = null, BindingPropertyChangingDelegate propertyChanging = null,
								 CoerceValueDelegate coerceValue = null, BindablePropertyBindingChanging bindingChanging = null, bool isReadOnly = false, CreateDefaultValueDelegate defaultValueCreator = null)
		{
			if (propertyName == null)
				throw new ArgumentNullException(nameof(propertyName));
			if (ReferenceEquals(returnType, null))
				throw new ArgumentNullException(nameof(returnType));
			if (ReferenceEquals(declaringType, null))
				throw new ArgumentNullException(nameof(declaringType));

			// don't use Enum.IsDefined as its redonkulously expensive for what it does
			if (defaultBindingMode != BindingMode.Default && defaultBindingMode != BindingMode.OneWay && defaultBindingMode != BindingMode.OneWayToSource && defaultBindingMode != BindingMode.TwoWay && defaultBindingMode != BindingMode.OneTime)
				throw new ArgumentException($"Not a valid type of BindingMode. Property: {returnType} {declaringType.Name}.{propertyName}. Default binding mode: {defaultBindingMode}", nameof(defaultBindingMode));

			if (defaultValue == null && Nullable.GetUnderlyingType(returnType) == null && returnType.GetTypeInfo().IsValueType)
				defaultValue = Activator.CreateInstance(returnType);

			if (defaultValue != null && !returnType.IsInstanceOfType(defaultValue))
				throw new ArgumentException($"Default value did not match return type. Property: {returnType} {declaringType.Name}.{propertyName} Default value type: {defaultValue.GetType().Name}, ", nameof(defaultValue));

			if (defaultBindingMode == BindingMode.Default)
				defaultBindingMode = BindingMode.OneWay;

			PropertyName = propertyName;
			ReturnType = returnType;
			ReturnTypeInfo = returnType.GetTypeInfo();
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

		public Type DeclaringType { get; private set; }

		public BindingMode DefaultBindingMode { get; private set; }

		public object DefaultValue { get; }

		public bool IsReadOnly { get; private set; }

		public string PropertyName { get; }

		public Type ReturnType { get; }

		internal BindablePropertyBindingChanging BindingChanging { get; private set; }

		internal CoerceValueDelegate CoerceValue { get; private set; }

		internal CreateDefaultValueDelegate DefaultValueCreator { get; }

		internal BindingPropertyChangedDelegate PropertyChanged { get; private set; }

		internal BindingPropertyChangingDelegate PropertyChanging { get; private set; }

		internal TypeInfo ReturnTypeInfo { get; }

		internal ValidateValueDelegate ValidateValue { get; private set; }

		public static BindableProperty Create(string propertyName, Type returnType, Type declaringType, object defaultValue = null, BindingMode defaultBindingMode = BindingMode.OneWay,
											  ValidateValueDelegate validateValue = null, BindingPropertyChangedDelegate propertyChanged = null, BindingPropertyChangingDelegate propertyChanging = null,
											  CoerceValueDelegate coerceValue = null, CreateDefaultValueDelegate defaultValueCreator = null)
		{
			return new BindableProperty(propertyName, returnType, declaringType, defaultValue, defaultBindingMode, validateValue, propertyChanged, propertyChanging, coerceValue,
				defaultValueCreator: defaultValueCreator);
		}

		public static BindableProperty CreateAttached(string propertyName, Type returnType, Type declaringType, object defaultValue, BindingMode defaultBindingMode = BindingMode.OneWay,
													  ValidateValueDelegate validateValue = null, BindingPropertyChangedDelegate propertyChanged = null, BindingPropertyChangingDelegate propertyChanging = null,
													  CoerceValueDelegate coerceValue = null, CreateDefaultValueDelegate defaultValueCreator = null)
		{
			return CreateAttached(propertyName, returnType, declaringType, defaultValue, defaultBindingMode, validateValue, propertyChanged, propertyChanging, coerceValue, null, false, defaultValueCreator);
		}

		public static BindablePropertyKey CreateAttachedReadOnly(string propertyName, Type returnType, Type declaringType, object defaultValue, BindingMode defaultBindingMode = BindingMode.OneWayToSource,
																 ValidateValueDelegate validateValue = null, BindingPropertyChangedDelegate propertyChanged = null, BindingPropertyChangingDelegate propertyChanging = null,
																 CoerceValueDelegate coerceValue = null, CreateDefaultValueDelegate defaultValueCreator = null)
		{
			return
				new BindablePropertyKey(CreateAttached(propertyName, returnType, declaringType, defaultValue, defaultBindingMode, validateValue, propertyChanged, propertyChanging, coerceValue, null, true,
					defaultValueCreator));
		}

		public static BindablePropertyKey CreateReadOnly(string propertyName, Type returnType, Type declaringType, object defaultValue, BindingMode defaultBindingMode = BindingMode.OneWayToSource,
														 ValidateValueDelegate validateValue = null, BindingPropertyChangedDelegate propertyChanged = null, BindingPropertyChangingDelegate propertyChanging = null,
														 CoerceValueDelegate coerceValue = null, CreateDefaultValueDelegate defaultValueCreator = null)
		{
			return
				new BindablePropertyKey(new BindableProperty(propertyName, returnType, declaringType, defaultValue, defaultBindingMode, validateValue, propertyChanged, propertyChanging, coerceValue,
					isReadOnly: true, defaultValueCreator: defaultValueCreator));
		}

		internal static BindableProperty Create(string propertyName, Type returnType, Type declaringType, object defaultValue, BindingMode defaultBindingMode, ValidateValueDelegate validateValue,
												BindingPropertyChangedDelegate propertyChanged, BindingPropertyChangingDelegate propertyChanging, CoerceValueDelegate coerceValue, BindablePropertyBindingChanging bindingChanging,
												CreateDefaultValueDelegate defaultValueCreator = null)
		{
			return new BindableProperty(propertyName, returnType, declaringType, defaultValue, defaultBindingMode, validateValue, propertyChanged, propertyChanging, coerceValue, bindingChanging,
				defaultValueCreator: defaultValueCreator);
		}

		internal static BindableProperty CreateAttached(string propertyName, Type returnType, Type declaringType, object defaultValue, BindingMode defaultBindingMode, ValidateValueDelegate validateValue,
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
			if (value == null)
				return !ReturnTypeInfo.IsValueType || ReturnTypeInfo.IsGenericType && ReturnTypeInfo.GetGenericTypeDefinition() == typeof(Nullable<>);

			Type valueType = value.GetType();
			Type type = ReturnType;

			// Dont support arbitrary IConvertible by limiting which types can use this
			if (SimpleConvertTypes.TryGetValue(valueType, out Type[] convertibleTo) && Array.IndexOf(convertibleTo, type) != -1)
			{
				value = Convert.ChangeType(value, type);
				return true;
			}
			if (KnownTypeConverters.TryGetValue(type, out TypeConverter typeConverterTo) && typeConverterTo.CanConvertFrom(valueType))
			{
				value = typeConverterTo.ConvertFromInvariantString(value.ToString());
				return true;
			}
			if (ReturnTypeInfo.IsAssignableFrom(valueType.GetTypeInfo()))
				return true;

			var cast = type.GetImplicitConversionOperator(fromType: valueType, toType: type) ?? valueType.GetImplicitConversionOperator(fromType: valueType, toType: type);
			if (cast != null)
			{
				value = cast.Invoke(null, new[] { value });
				return true;
			}
			if (KnownIValueConverters.TryGetValue(type, out IValueConverter valueConverter))
			{
				value = valueConverter.Convert(value, type, null, CultureInfo.CurrentUICulture);
				return true;
			}

			return false;
		}

		internal delegate void BindablePropertyBindingChanging(BindableObject bindable, BindingBase oldValue, BindingBase newValue);
	}
}
