#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/Binding.xml" path="Type[@FullName='Microsoft.Maui.Controls.Binding']/Docs/*" />
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
		public static readonly object DoNothing = new object();

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

			base.Apply(src ?? context, bindObj, targetProperty, fromBindingContextChanged, specificity);

			if (src != null && isApplied && fromBindingContextChanged)
				return;

			if (Source is RelativeBindingSource)
			{
				ApplyRelativeSourceBinding(bindObj, targetProperty, specificity);
			}
			else
			{
				object bindingContext = src ?? Context ?? context;
				if (_expression == null)
					_expression = new BindingExpression(this, SelfPath);
				_expression.Apply(bindingContext, bindObj, targetProperty, specificity);
			}
		}

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
		async void ApplyRelativeSourceBinding(
			BindableObject targetObject,
			BindableProperty targetProperty, SetterSpecificity specificity)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
		{
			if (!(Source is RelativeBindingSource relativeSource))
				return;

			var relativeSourceTarget = RelativeSourceTargetOverride ?? targetObject as Element;
			if (!(relativeSourceTarget is Element))
				throw new InvalidOperationException();

			object resolvedSource = null;
			switch (relativeSource.Mode)
			{
				case RelativeBindingSourceMode.Self:
					resolvedSource = relativeSourceTarget;
					break;

				case RelativeBindingSourceMode.TemplatedParent:
					resolvedSource = await TemplateUtilities.FindTemplatedParentAsync(relativeSourceTarget);
					break;

				case RelativeBindingSourceMode.FindAncestor:
				case RelativeBindingSourceMode.FindAncestorBindingContext:
					ApplyAncestorTypeBinding(targetObject, relativeSourceTarget, targetProperty, specificity);
					return;

				default:
					throw new InvalidOperationException();
			}

			_expression.Apply(resolvedSource, targetObject, targetProperty, specificity);
		}

		void ApplyAncestorTypeBinding(
			BindableObject actualTarget,
			Element relativeSourceTarget,
			BindableProperty targetProperty,
			SetterSpecificity specificity,
			Element currentElement = null,
			int currentLevel = 0,
			List<Element> chain = null,
			object lastMatchingBctx = null)
		{
			currentElement = currentElement ?? relativeSourceTarget;
			chain = chain ?? new List<Element> { relativeSourceTarget };

			if (!(Source is RelativeBindingSource relativeSource))
				return;

			if (currentElement.RealParent is Application ||
				currentElement.RealParent == null)
			{
				// Couldn't find the desired ancestor type in the chain, but it may be added later, 
				// so apply with a null source for now.
				_expression.Apply(null, actualTarget, targetProperty, specificity);
				_expression.SubscribeToAncestryChanges(
					chain,
					relativeSource.Mode == RelativeBindingSourceMode.FindAncestorBindingContext,
					rootIsSource: false);
			}
			else if (currentElement.RealParent != null)
			{
				chain.Add(currentElement.RealParent);
				if (ElementFitsAncestorTypeAndLevel(currentElement.RealParent, ref currentLevel, ref lastMatchingBctx))
				{
					object resolvedSource;
					if (relativeSource.Mode == RelativeBindingSourceMode.FindAncestor)
						resolvedSource = currentElement.RealParent;
					else
						resolvedSource = currentElement.RealParent?.BindingContext;
					_expression.Apply(resolvedSource, actualTarget, targetProperty, specificity);
					_expression.SubscribeToAncestryChanges(
						chain,
						relativeSource.Mode == RelativeBindingSourceMode.FindAncestorBindingContext,
						rootIsSource: true);
				}
				else
				{
					ApplyAncestorTypeBinding(
						actualTarget,
						relativeSourceTarget,
						targetProperty,
						specificity,
						currentElement.RealParent,
						currentLevel,
						chain,
						lastMatchingBctx);
				}
			}
			else
			{
				EventHandler onElementParentSet = null;
				onElementParentSet = (sender, e) =>
				{
					currentElement.ParentSet -= onElementParentSet;
					ApplyAncestorTypeBinding(
						actualTarget,
						relativeSourceTarget,
						targetProperty,
						specificity,
						currentElement,
						currentLevel,
						chain,
						lastMatchingBctx);
				};
				currentElement.ParentSet += onElementParentSet;
			}
		}

		bool ElementFitsAncestorTypeAndLevel(Element element, ref int level, ref object lastPotentialBctx)
		{
			if (!(Source is RelativeBindingSource relativeSource))
				return false;

			bool fitsElementType =
				relativeSource.Mode == RelativeBindingSourceMode.FindAncestor &&
				relativeSource.AncestorType.IsAssignableFrom(element.GetType());

			bool fitsBindingContextType =
				element.BindingContext != null &&
				relativeSource.Mode == RelativeBindingSourceMode.FindAncestorBindingContext &&
				relativeSource.AncestorType.IsAssignableFrom(element.BindingContext.GetType());

			if (!fitsElementType && !fitsBindingContextType)
				return false;

			if (fitsBindingContextType)
			{
				if (!object.ReferenceEquals(lastPotentialBctx, element.BindingContext))
				{
					lastPotentialBctx = element.BindingContext;
					level++;
				}
			}
			else
			{
				level++;
			}

			return level >= relativeSource.AncestorLevel;
		}

		internal override BindingBase Clone()
		{
			return new Binding(Path, Mode)
			{
				Converter = Converter,
				ConverterParameter = ConverterParameter,
				StringFormat = StringFormat,
				Source = Source,
				UpdateSourceEventName = UpdateSourceEventName,
				TargetNullValue = TargetNullValue,
				FallbackValue = FallbackValue,
			};
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

			if (_expression != null)
				_expression.Unapply();
		}
	}
}
