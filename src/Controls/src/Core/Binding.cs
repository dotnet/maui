#nullable disable
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Maui.Controls.Xaml.Diagnostics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/Binding.xml" path="Type[@FullName='Microsoft.Maui.Controls.Binding']/Docs/*" />
	[RequiresUnreferencedCode(TrimmerConstants.StringPathBindingWarning, Url = TrimmerConstants.ExpressionBasedBindingsDocsUrl)]
	public sealed class Binding : BindingBase
	{
		public const string SelfPath = ".";
		IValueConverter _converter;
		object _converterParameter;

		BindingExpression _expression;
		string _path;
		object _source;
		string _updateSourceEventName;

		/// <include file="../../docs/Microsoft.Maui.Controls/Binding.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public Binding()
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Binding.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public Binding(string path, BindingMode mode = BindingMode.Default, IValueConverter converter = null, object converterParameter = null, string stringFormat = null, object source = null)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));
			if (string.IsNullOrWhiteSpace(path))
				throw new ArgumentException("path cannot be an empty string", nameof(path));

			Path = path;
			Converter = converter;
			ConverterParameter = converterParameter;
			Mode = mode;
			StringFormat = stringFormat;
			Source = source;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Binding.xml" path="//Member[@MemberName='Converter']/Docs/*" />
		public IValueConverter Converter
		{
			get { return _converter; }
			set
			{
				ThrowIfApplied();

				_converter = value;
			}
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Binding.xml" path="//Member[@MemberName='ConverterParameter']/Docs/*" />
		public object ConverterParameter
		{
			get { return _converterParameter; }
			set
			{
				ThrowIfApplied();

				_converterParameter = value;
			}
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Binding.xml" path="//Member[@MemberName='Path']/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/Binding.xml" path="//Member[@MemberName='Source']/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/Binding.xml" path="//Member[@MemberName='DoNothing']/Docs/*" />
		public static readonly object DoNothing = MultiBinding.DoNothing; // the instance was moved to MultiBinding because the Binding class is annotated with [RequiresUnreferencedCode]

		/// <include file="../../docs/Microsoft.Maui.Controls/Binding.xml" path="//Member[@MemberName='UpdateSourceEventName']/Docs/*" />
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

			if (_expression == null)
				_expression = new BindingExpression(this, SelfPath);

			_expression.Apply(fromTarget);
		}

		internal override void Apply(object context, BindableObject bindObj, BindableProperty targetProperty, bool fromBindingContextChanged, SetterSpecificity specificity)
		{
			object src = _source;
			var isApplied = IsApplied;

			var bindingContext = src ?? Context ?? context;
			base.Apply(bindingContext, bindObj, targetProperty, fromBindingContextChanged, specificity);

			if (src != null && isApplied && fromBindingContextChanged)
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
				if (_expression == null)
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
			if (Source != null && !(Source is RelativeBindingSource) && fromBindingContextChanged && IsApplied)
				return;

			base.Unapply(fromBindingContextChanged: fromBindingContextChanged);

			_expression?.Unapply();
		}
	}
}