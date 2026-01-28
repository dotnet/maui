#nullable disable
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
	/// <summary>Binds a property in a control template to a templated parent property.</summary>
	[Obsolete("Use Binding.Source=RelativeBindingSource.TemplatedParent")]
	[RequiresUnreferencedCode(TrimmerConstants.StringPathBindingWarning, Url = TrimmerConstants.ExpressionBasedBindingsDocsUrl)]
	public sealed class TemplateBinding : BindingBase
	{
		internal const string SelfPath = ".";
		IValueConverter _converter;
		object _converterParameter;

		BindingExpression _expression;
		string _path;

		/// <summary>Creates a new <see cref="TemplateBinding"/> with default values.</summary>
		public TemplateBinding()
		{
		}

		/// <summary>Creates a new <see cref="TemplateBinding"/> with the specified path and optional parameters.</summary>
		public TemplateBinding(string path, BindingMode mode = BindingMode.Default, IValueConverter converter = null, object converterParameter = null, string stringFormat = null)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));
			if (string.IsNullOrWhiteSpace(path))
				throw new ArgumentException("path cannot be an empty string", nameof(path));

			AllowChaining = true;
			Path = path;
			Converter = converter;
			ConverterParameter = converterParameter;
			Mode = mode;
			StringFormat = stringFormat;
		}

		/// <summary>Gets or sets the converter used to convert values between source and target.</summary>
		public IValueConverter Converter
		{
			get { return _converter; }
			set
			{
				ThrowIfApplied();

				_converter = value;
			}
		}

		/// <summary>Gets or sets the parameter passed to the converter.</summary>
		public object ConverterParameter
		{
			get { return _converterParameter; }
			set
			{
				ThrowIfApplied();

				_converterParameter = value;
			}
		}

		/// <summary>Gets or sets the path to the property on the templated parent.</summary>
		public string Path
		{
			get { return _path; }
			set
			{
				ThrowIfApplied();

				_path = value;
				_expression = GetBindingExpression(value);
			}
		}

		internal override void Apply(bool fromTarget)
		{
			base.Apply(fromTarget);

			if (_expression == null)
				_expression = new BindingExpression(this, SelfPath);

			_expression.Apply(fromTarget);
		}

		internal override async void Apply(object newContext, BindableObject bindObj, BindableProperty targetProperty, bool fromBindingContextChanged, SetterSpecificity specificity)
		{
			var view = bindObj as Element;
			if (view == null)
				throw new InvalidOperationException();

			base.Apply(newContext, bindObj, targetProperty, fromBindingContextChanged, specificity);

			Element templatedParent = await TemplateUtilities.FindTemplatedParentAsync(view);
			ApplyInner(templatedParent, bindObj, targetProperty);
		}

		internal override BindingBase Clone()
		{
			var clone = new TemplateBinding(Path, Mode) { Converter = Converter, ConverterParameter = ConverterParameter, StringFormat = StringFormat };

			if (VisualDiagnostics.IsEnabled && VisualDiagnostics.GetSourceInfo(this) is SourceInfo info)
				VisualDiagnostics.RegisterSourceInfo(clone, info.SourceUri, info.LineNumber, info.LinePosition);

			return clone;
		}

		internal override object GetSourceValue(object value, Type targetPropertyType)
		{
			if (Converter != null)
				value = Converter.Convert(value, targetPropertyType, ConverterParameter, CultureInfo.CurrentUICulture);

			return base.GetSourceValue(value, targetPropertyType);
		}

		internal override object GetTargetValue(object value, Type sourcePropertyType)
		{
			if (Converter != null)
				value = Converter.ConvertBack(value, sourcePropertyType, ConverterParameter, CultureInfo.CurrentUICulture);

			return base.GetTargetValue(value, sourcePropertyType);
		}

		internal override void Unapply(bool fromBindingContextChanged = false)
		{
			base.Unapply(fromBindingContextChanged: fromBindingContextChanged);

			_expression?.Unapply();
		}

		void ApplyInner(Element templatedParent, BindableObject bindableObject, BindableProperty targetProperty)
		{
			if (_expression == null && templatedParent != null)
				_expression = new BindingExpression(this, SelfPath);

			_expression?.Apply(templatedParent, bindableObject, targetProperty, SetterSpecificity.FromBinding);
		}

		BindingExpression GetBindingExpression(string path)
		{
			return new BindingExpression(this, !string.IsNullOrWhiteSpace(path) ? path : SelfPath);
		}
	}
}