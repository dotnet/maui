#nullable disable
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Maui.Controls.Xaml.Diagnostics;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// A single 1:1 immutable data binding.
	/// </summary>
	[RequiresUnreferencedCode(TrimmerConstants.StringPathBindingWarning, Url = TrimmerConstants.ExpressionBasedBindingsDocsUrl)]
	public sealed class Binding : BindingBase
	{
		public const string SelfPath = ".";
		IValueConverter _converter;
		object _converterParameter;
		CultureInfo _converterCulture;

		BindingExpression _expression;
		string _path;
		object _source;
		string _updateSourceEventName;

		/// <summary>
		/// Constructs and initializes a new instance of the <see cref="Binding" /> class.
		/// </summary>
		public Binding()
		{
		}

		/// <summary>
		/// Constructs and initializes a new instance of the <see cref="Binding" /> class.
		/// </summary>
		/// <param name="path">The property path.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is <see langword="null"/> or whitespace.</exception>
		[EditorBrowsable(EditorBrowsableState.Never)] // We don't really want people to use this, mostly added for backwards compatibility
		public Binding(string path)
			: this(path, BindingMode.Default, null, null, null, null, null)
		{
		}

		/// <summary>
		/// Constructs and initializes a new instance of the <see cref="Binding" /> class.
		/// </summary>
		/// <param name="path">The property path.</param>
		/// <param name="mode">The binding mode.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is <see langword="null"/> or whitespace.</exception>
		[EditorBrowsable(EditorBrowsableState.Never)] // We don't really want people to use this, mostly added for backwards compatibility
		public Binding(string path, BindingMode mode)
			: this(path, mode, null, null, null, null, null)
		{
		}

		/// <summary>
		/// Constructs and initializes a new instance of the <see cref="Binding" /> class.
		/// </summary>
		/// <param name="path">The property path.</param>
		/// <param name="mode">The binding mode.</param>
		/// <param name="converter">The converter.</param>
		/// <param name="converterParameter">An user-defined parameter to pass to the converter.</param>
		/// <param name="stringFormat">A String format.</param>
		/// <param name="source">An object used as the source for this binding.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is <see langword="null"/> or whitespace.</exception>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Binding(string path, BindingMode mode, IValueConverter converter, object converterParameter, string stringFormat, object source)
			: this(path, mode, converter, converterParameter, null, stringFormat, source)
		{
		}

		/// <summary>
		/// Constructs and initializes a new instance of the <see cref="Binding" /> class.
		/// </summary>
		/// <param name="path">The property path.</param>
		/// <param name="mode">The binding mode. This property is optional. Default is <see cref="BindingMode.Default" />.</param>
		/// <param name="converter">The converter. This parameter is optional. Default is <see langword="null" />.</param>
		/// <param name="converterParameter">An user-defined parameter to pass to the converter. This parameter is optional. Default is <see langword="null" />.</param>
		/// <param name="converterCulture">A user-defined culture information object to pass to the converter. This parameter is optional. Default is <see langword="null" />.</param>
		/// <param name="stringFormat">A String format. This parameter is optional. Default is <see langword="null" />.</param>
		/// <param name="source">An object used as the source for this binding. This parameter is optional. Default is <see langword="null" />.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is <see langword="null"/> or whitespace.</exception>
		public Binding(string path, BindingMode mode = BindingMode.Default, IValueConverter converter = null, object converterParameter = null, CultureInfo converterCulture = null, string stringFormat = null, object source = null)
		{
			if (path is null)
				throw new ArgumentNullException(nameof(path));
			if (string.IsNullOrWhiteSpace(path))
				throw new ArgumentException("path cannot be an empty string", nameof(path));

			Path = path;
			Converter = converter;
			ConverterParameter = converterParameter;
			ConverterCulture = converterCulture;
			Mode = mode;
			StringFormat = stringFormat;
			Source = source;
		}

		/// <summary>
		/// Gets or sets the converter to be used for this binding.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown when the binding is already applied.</exception>
		public IValueConverter Converter
		{
			get { return _converter; }
			set
			{
				ThrowIfApplied();

				_converter = value;
			}
		}

		/// <summary>
		/// Gets or sets the parameter passed as argument to the converter.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown when the binding is already applied.</exception>
		public object ConverterParameter
		{
			get { return _converterParameter; }
			set
			{
				ThrowIfApplied();

				_converterParameter = value;
			}
		}

		/// <summary>
		/// Gets or sets the culture information used by the converter.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown when the binding is already applied.</exception>
		[TypeConverter(typeof(CultureInfoIetfLanguageTagConverter))]
		public CultureInfo ConverterCulture
		{
			get { return _converterCulture; }
			set
			{
				ThrowIfApplied();

				_converterCulture = value;
			}
		}

		/// <summary>
		/// Gets or sets the path of the property.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown when the binding is already applied.</exception>
		public string Path
		{
			get { return _path; }
			set
			{
				ThrowIfApplied();

				_path = value;
				_expression = new BindingExpression(this, !string.IsNullOrWhiteSpace(value) ? value : SelfPath);
			}
		}

		/// <summary>
		/// Gets or sets the source of the binding.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown when the binding is already applied.</exception>
		public object Source
		{
			get { return _source; }
			set
			{
				ThrowIfApplied();
				_source = value;
				if ((value as RelativeBindingSource)?.Mode == RelativeBindingSourceMode.TemplatedParent)
					AllowChaining = true;
			}
		}

		/// <summary>
		/// Does nothing.
		/// </summary>
		public static readonly object DoNothing = MultiBinding.DoNothing; // the instance was moved to MultiBinding because the Binding class is annotated with [RequiresUnreferencedCode]

		/// <summary>
		/// Gets or sets the name of the update source event. The setter throws an exception if the property has already been
		/// applied.
		/// </summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public string UpdateSourceEventName
		{
			get { return _updateSourceEventName; }
			set
			{
				ThrowIfApplied();
				_updateSourceEventName = value;
			}
		}

		internal Type DataType { get; set; }

		internal override void Apply(bool fromTarget)
		{
			base.Apply(fromTarget);

			if (_expression is null)
				_expression = new BindingExpression(this, SelfPath);

			_expression.Apply(fromTarget);
		}

		internal override void Apply(object context, BindableObject bindObj, BindableProperty targetProperty, bool fromBindingContextChanged, SetterSpecificity specificity)
		{
			object src = _source;
			var isApplied = IsApplied;

			var bindingContext = src ?? Context ?? context;
			base.Apply(bindingContext, bindObj, targetProperty, fromBindingContextChanged, specificity);

			if (src is not null && isApplied && fromBindingContextChanged)
				return;

			if (Source is RelativeBindingSource relativeBindingSource)
			{
				var relativeSourceTarget = RelativeSourceTargetOverride ?? bindObj as Element;
				if (relativeSourceTarget is not Element)
				{
					var message = bindObj is not null
						? $"Cannot apply relative binding to {bindObj.GetType().FullName} because it is not a superclass of Element."
						: "Cannot apply relative binding when the target object is null.";

					throw new InvalidOperationException(message);
				}

				ApplyRelativeSourceBinding(relativeBindingSource, relativeSourceTarget, bindObj, targetProperty, specificity);
			}
			else
			{
				if (_expression is null)
					_expression = new BindingExpression(this, SelfPath);
				_expression.Apply(bindingContext, bindObj, targetProperty, specificity);
			}
		}

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
		async void ApplyRelativeSourceBinding(RelativeBindingSource relativeSource, Element relativeSourceTarget, BindableObject targetObject, BindableProperty targetProperty, SetterSpecificity specificity)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
		{
			await relativeSource.Apply(_expression, relativeSourceTarget, targetObject, targetProperty, specificity);
		}

		internal override BindingBase Clone()
		{
			var clone = new Binding(Path, Mode)
			{
				Converter = Converter,
				ConverterParameter = ConverterParameter,
				ConverterCulture = ConverterCulture,
				StringFormat = StringFormat,
				Source = Source,
				UpdateSourceEventName = UpdateSourceEventName,
				TargetNullValue = TargetNullValue,
				FallbackValue = FallbackValue,
			};

			if (VisualDiagnostics.IsEnabled && VisualDiagnostics.GetSourceInfo(this) is SourceInfo info)
				VisualDiagnostics.RegisterSourceInfo(clone, info.SourceUri, info.LineNumber, info.LinePosition);

			return clone;
		}

		internal override object GetSourceValue(object value, Type targetPropertyType)
		{
			if (Converter != null)
				value = Converter.Convert(value, targetPropertyType, ConverterParameter, ConverterCulture);

			return base.GetSourceValue(value, targetPropertyType);
		}

		internal override object GetTargetValue(object value, Type sourcePropertyType)
		{
			if (Converter is not null)
				value = Converter.ConvertBack(value, sourcePropertyType, ConverterParameter, ConverterCulture);

			return base.GetTargetValue(value, sourcePropertyType);
		}

		internal override void Unapply(bool fromBindingContextChanged = false)
		{
			if (Source is not null && !(Source is RelativeBindingSource) && fromBindingContextChanged && IsApplied)
				return;

			base.Unapply(fromBindingContextChanged: fromBindingContextChanged);

			if (_expression is not null)
			{
				_expression.Unapply();
			}
		}
	}
}