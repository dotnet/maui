using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms
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

		[Obsolete("Create<> (generic) is obsolete as of version 2.1.0 and is no longer supported.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static BindableProperty Create<TDeclarer, TPropertyType>(Expression<Func<TDeclarer, TPropertyType>> getter, TPropertyType defaultValue, BindingMode defaultBindingMode = BindingMode.OneWay,
																		ValidateValueDelegate<TPropertyType> validateValue = null, BindingPropertyChangedDelegate<TPropertyType> propertyChanged = null,
																		BindingPropertyChangingDelegate<TPropertyType> propertyChanging = null, CoerceValueDelegate<TPropertyType> coerceValue = null,
																		CreateDefaultValueDelegate<TDeclarer, TPropertyType> defaultValueCreator = null) where TDeclarer : BindableObject
		{
			return Create(getter, defaultValue, defaultBindingMode, validateValue, propertyChanged, propertyChanging, coerceValue, null, defaultValueCreator: defaultValueCreator);
		}

		public static BindableProperty Create(string propertyName, Type returnType, Type declaringType, object defaultValue = null, BindingMode defaultBindingMode = BindingMode.OneWay,
											  ValidateValueDelegate validateValue = null, BindingPropertyChangedDelegate propertyChanged = null, BindingPropertyChangingDelegate propertyChanging = null,
											  CoerceValueDelegate coerceValue = null, CreateDefaultValueDelegate defaultValueCreator = null)
		{
			return new BindableProperty(propertyName, returnType, declaringType, defaultValue, defaultBindingMode, validateValue, propertyChanged, propertyChanging, coerceValue,
				defaultValueCreator: defaultValueCreator);
		}

		[Obsolete("CreateAttached<> (generic) is obsolete as of version 2.1.0 and is no longer supported.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static BindableProperty CreateAttached<TDeclarer, TPropertyType>(Expression<Func<BindableObject, TPropertyType>> staticgetter, TPropertyType defaultValue,
																				BindingMode defaultBindingMode = BindingMode.OneWay, ValidateValueDelegate<TPropertyType> validateValue = null, BindingPropertyChangedDelegate<TPropertyType> propertyChanged = null,
																				BindingPropertyChangingDelegate<TPropertyType> propertyChanging = null, CoerceValueDelegate<TPropertyType> coerceValue = null,
																				CreateDefaultValueDelegate<BindableObject, TPropertyType> defaultValueCreator = null)
		{
			return CreateAttached<TDeclarer, TPropertyType>(staticgetter, defaultValue, defaultBindingMode, validateValue, propertyChanged, propertyChanging, coerceValue, null,
				defaultValueCreator: defaultValueCreator);
		}

		public static BindableProperty CreateAttached(string propertyName, Type returnType, Type declaringType, object defaultValue, BindingMode defaultBindingMode = BindingMode.OneWay,
													  ValidateValueDelegate validateValue = null, BindingPropertyChangedDelegate propertyChanged = null, BindingPropertyChangingDelegate propertyChanging = null,
													  CoerceValueDelegate coerceValue = null, CreateDefaultValueDelegate defaultValueCreator = null)
		{
			return CreateAttached(propertyName, returnType, declaringType, defaultValue, defaultBindingMode, validateValue, propertyChanged, propertyChanging, coerceValue, null, false, defaultValueCreator);
		}

		[Obsolete("CreateAttachedReadOnly<> (generic) is obsolete as of version 2.1.0 and is no longer supported.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static BindablePropertyKey CreateAttachedReadOnly<TDeclarer, TPropertyType>(Expression<Func<BindableObject, TPropertyType>> staticgetter, TPropertyType defaultValue,
																						   BindingMode defaultBindingMode = BindingMode.OneWayToSource, ValidateValueDelegate<TPropertyType> validateValue = null,
																						   BindingPropertyChangedDelegate<TPropertyType> propertyChanged = null, BindingPropertyChangingDelegate<TPropertyType> propertyChanging = null,
																						   CoerceValueDelegate<TPropertyType> coerceValue = null, CreateDefaultValueDelegate<BindableObject, TPropertyType> defaultValueCreator = null)

		{
			return
				new BindablePropertyKey(CreateAttached<TDeclarer, TPropertyType>(staticgetter, defaultValue, defaultBindingMode, validateValue, propertyChanged, propertyChanging, coerceValue, null, true,
					defaultValueCreator));
		}

		public static BindablePropertyKey CreateAttachedReadOnly(string propertyName, Type returnType, Type declaringType, object defaultValue, BindingMode defaultBindingMode = BindingMode.OneWayToSource,
																 ValidateValueDelegate validateValue = null, BindingPropertyChangedDelegate propertyChanged = null, BindingPropertyChangingDelegate propertyChanging = null,
																 CoerceValueDelegate coerceValue = null, CreateDefaultValueDelegate defaultValueCreator = null)
		{
			return
				new BindablePropertyKey(CreateAttached(propertyName, returnType, declaringType, defaultValue, defaultBindingMode, validateValue, propertyChanged, propertyChanging, coerceValue, null, true,
					defaultValueCreator));
		}

		[Obsolete("CreateReadOnly<> (generic) is obsolete as of version 2.1.0 and is no longer supported.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static BindablePropertyKey CreateReadOnly<TDeclarer, TPropertyType>(Expression<Func<TDeclarer, TPropertyType>> getter, TPropertyType defaultValue,
																				   BindingMode defaultBindingMode = BindingMode.OneWayToSource, ValidateValueDelegate<TPropertyType> validateValue = null,
																				   BindingPropertyChangedDelegate<TPropertyType> propertyChanged = null, BindingPropertyChangingDelegate<TPropertyType> propertyChanging = null,
																				   CoerceValueDelegate<TPropertyType> coerceValue = null, CreateDefaultValueDelegate<TDeclarer, TPropertyType> defaultValueCreator = null) where TDeclarer : BindableObject
		{
			return new BindablePropertyKey(Create(getter, defaultValue, defaultBindingMode, validateValue, propertyChanged, propertyChanging, coerceValue, null, true, defaultValueCreator));
		}

		public static BindablePropertyKey CreateReadOnly(string propertyName, Type returnType, Type declaringType, object defaultValue, BindingMode defaultBindingMode = BindingMode.OneWayToSource,
														 ValidateValueDelegate validateValue = null, BindingPropertyChangedDelegate propertyChanged = null, BindingPropertyChangingDelegate propertyChanging = null,
														 CoerceValueDelegate coerceValue = null, CreateDefaultValueDelegate defaultValueCreator = null)
		{
			return
				new BindablePropertyKey(new BindableProperty(propertyName, returnType, declaringType, defaultValue, defaultBindingMode, validateValue, propertyChanged, propertyChanging, coerceValue,
					isReadOnly: true, defaultValueCreator: defaultValueCreator));
		}

		[Obsolete("Create<> (generic) is obsolete as of version 2.1.0 and is no longer supported.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		internal static BindableProperty Create<TDeclarer, TPropertyType>(Expression<Func<TDeclarer, TPropertyType>> getter, TPropertyType defaultValue, BindingMode defaultBindingMode,
																		  ValidateValueDelegate<TPropertyType> validateValue, BindingPropertyChangedDelegate<TPropertyType> propertyChanged, BindingPropertyChangingDelegate<TPropertyType> propertyChanging,
																		  CoerceValueDelegate<TPropertyType> coerceValue, BindablePropertyBindingChanging bindingChanging, bool isReadOnly = false,
																		  CreateDefaultValueDelegate<TDeclarer, TPropertyType> defaultValueCreator = null) where TDeclarer : BindableObject
		{
			if (getter == null)
				throw new ArgumentNullException("getter");

			Expression expr = getter.Body;

			var unary = expr as UnaryExpression;
			if (unary != null)
				expr = unary.Operand;

			var member = expr as MemberExpression;
			if (member == null)
				throw new ArgumentException("getter must be a MemberExpression", "getter");

			var property = (PropertyInfo)member.Member;

			ValidateValueDelegate untypedValidateValue = null;
			BindingPropertyChangedDelegate untypedBindingPropertyChanged = null;
			BindingPropertyChangingDelegate untypedBindingPropertyChanging = null;
			CoerceValueDelegate untypedCoerceValue = null;
			CreateDefaultValueDelegate untypedDefaultValueCreator = null;
			if (validateValue != null)
				untypedValidateValue = (bindable, value) => validateValue(bindable, (TPropertyType)value);
			if (propertyChanged != null)
				untypedBindingPropertyChanged = (bindable, oldValue, newValue) => propertyChanged(bindable, (TPropertyType)oldValue, (TPropertyType)newValue);
			if (propertyChanging != null)
				untypedBindingPropertyChanging = (bindable, oldValue, newValue) => propertyChanging(bindable, (TPropertyType)oldValue, (TPropertyType)newValue);
			if (coerceValue != null)
				untypedCoerceValue = (bindable, value) => coerceValue(bindable, (TPropertyType)value);
			if (defaultValueCreator != null)
				untypedDefaultValueCreator = o => defaultValueCreator((TDeclarer)o);

			return new BindableProperty(property.Name, property.PropertyType, typeof(TDeclarer), defaultValue, defaultBindingMode, untypedValidateValue, untypedBindingPropertyChanged,
				untypedBindingPropertyChanging, untypedCoerceValue, bindingChanging, isReadOnly, untypedDefaultValueCreator);
		}

		internal static BindableProperty Create(string propertyName, Type returnType, Type declaringType, object defaultValue, BindingMode defaultBindingMode, ValidateValueDelegate validateValue,
												BindingPropertyChangedDelegate propertyChanged, BindingPropertyChangingDelegate propertyChanging, CoerceValueDelegate coerceValue, BindablePropertyBindingChanging bindingChanging,
												CreateDefaultValueDelegate defaultValueCreator = null)
		{
			return new BindableProperty(propertyName, returnType, declaringType, defaultValue, defaultBindingMode, validateValue, propertyChanged, propertyChanging, coerceValue, bindingChanging,
				defaultValueCreator: defaultValueCreator);
		}

		[Obsolete("CreateAttached<> (generic) is obsolete as of version 2.1.0 and is no longer supported.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		internal static BindableProperty CreateAttached<TDeclarer, TPropertyType>(Expression<Func<BindableObject, TPropertyType>> staticgetter, TPropertyType defaultValue, BindingMode defaultBindingMode,
																				  ValidateValueDelegate<TPropertyType> validateValue, BindingPropertyChangedDelegate<TPropertyType> propertyChanged, BindingPropertyChangingDelegate<TPropertyType> propertyChanging,
																				  CoerceValueDelegate<TPropertyType> coerceValue, BindablePropertyBindingChanging bindingChanging, bool isReadOnly = false,
																				  CreateDefaultValueDelegate<BindableObject, TPropertyType> defaultValueCreator = null)
		{
			if (staticgetter == null)
				throw new ArgumentNullException("staticgetter");

			Expression expr = staticgetter.Body;

			var unary = expr as UnaryExpression;
			if (unary != null)
				expr = unary.Operand;

			var methodcall = expr as MethodCallExpression;
			if (methodcall == null)
				throw new ArgumentException("staticgetter must be a MethodCallExpression", "staticgetter");

			MethodInfo method = methodcall.Method;
			if (!method.Name.StartsWith("Get", StringComparison.Ordinal))
				throw new ArgumentException("staticgetter name must start with Get", "staticgetter");

			string propertyname = method.Name.Substring(3);

			ValidateValueDelegate untypedValidateValue = null;
			BindingPropertyChangedDelegate untypedBindingPropertyChanged = null;
			BindingPropertyChangingDelegate untypedBindingPropertyChanging = null;
			CoerceValueDelegate untypedCoerceValue = null;
			CreateDefaultValueDelegate untypedDefaultValueCreator = null;
			if (validateValue != null)
				untypedValidateValue = (bindable, value) => validateValue(bindable, (TPropertyType)value);
			if (propertyChanged != null)
				untypedBindingPropertyChanged = (bindable, oldValue, newValue) => propertyChanged(bindable, (TPropertyType)oldValue, (TPropertyType)newValue);
			if (propertyChanging != null)
				untypedBindingPropertyChanging = (bindable, oldValue, newValue) => propertyChanging(bindable, (TPropertyType)oldValue, (TPropertyType)newValue);
			if (coerceValue != null)
				untypedCoerceValue = (bindable, value) => coerceValue(bindable, (TPropertyType)value);
			if (defaultValueCreator != null)
				untypedDefaultValueCreator = o => defaultValueCreator(o);

			return new BindableProperty(propertyname, method.ReturnType, typeof(TDeclarer), defaultValue, defaultBindingMode, untypedValidateValue, untypedBindingPropertyChanged, untypedBindingPropertyChanging,
				untypedCoerceValue, bindingChanging, isReadOnly, untypedDefaultValueCreator);
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