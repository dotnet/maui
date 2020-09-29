using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	public sealed class Binding : BindingBase
	{
		internal const string SelfPath = ".";
		IValueConverter _converter;
		object _converterParameter;

		BindingExpression _expression;
		string _path;
		object _source;
		string _updateSourceEventName;

		public Binding()
		{
		}

		public Binding(string path, BindingMode mode = BindingMode.Default, IValueConverter converter = null, object converterParameter = null, string stringFormat = null, object source = null)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));
			if (string.IsNullOrWhiteSpace(path))
				throw new ArgumentException("path can not be an empty string", nameof(path));

			Path = path;
			Converter = converter;
			ConverterParameter = converterParameter;
			Mode = mode;
			StringFormat = stringFormat;
			Source = source;
		}

		public IValueConverter Converter
		{
			get { return _converter; }
			set
			{
				ThrowIfApplied();

				_converter = value;
			}
		}

		public object ConverterParameter
		{
			get { return _converterParameter; }
			set
			{
				ThrowIfApplied();

				_converterParameter = value;
			}
		}

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

		public static readonly object DoNothing = new object();

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

		[Obsolete]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static Binding Create<TSource>(Expression<Func<TSource, object>> propertyGetter, BindingMode mode = BindingMode.Default, IValueConverter converter = null, object converterParameter = null,
											  string stringFormat = null)
		{
			if (propertyGetter == null)
				throw new ArgumentNullException(nameof(propertyGetter));

			return new Binding(GetBindingPath(propertyGetter), mode, converter, converterParameter, stringFormat);
		}

		internal override void Apply(bool fromTarget)
		{
			base.Apply(fromTarget);

			if (_expression == null)
				_expression = new BindingExpression(this, SelfPath);

			_expression.Apply(fromTarget);
		}

		internal override void Apply(object context, BindableObject bindObj, BindableProperty targetProperty, bool fromBindingContextChanged = false)
		{
			object src = _source;
			var isApplied = IsApplied;

			base.Apply(src ?? context, bindObj, targetProperty, fromBindingContextChanged: fromBindingContextChanged);

			if (src != null && isApplied && fromBindingContextChanged)
				return;

			if (Source is RelativeBindingSource)
			{
				ApplyRelativeSourceBinding(bindObj, targetProperty);
			}
			else
			{
				object bindingContext = src ?? Context ?? context;
				if (_expression == null && bindingContext != null)
					_expression = new BindingExpression(this, SelfPath);
				_expression.Apply(bindingContext, bindObj, targetProperty);
			}
		}

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
		async void ApplyRelativeSourceBinding(
			BindableObject targetObject,
			BindableProperty targetProperty)
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
					ApplyAncestorTypeBinding(targetObject, relativeSourceTarget, targetProperty);
					return;

				default:
					throw new InvalidOperationException();
			}

			_expression.Apply(resolvedSource, targetObject, targetProperty);
		}

		void ApplyAncestorTypeBinding(
			BindableObject actualTarget,
			Element relativeSourceTarget,
			BindableProperty targetProperty,
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
				_expression.Apply(null, actualTarget, targetProperty);
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
					_expression.Apply(resolvedSource, actualTarget, targetProperty);
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
				relativeSource.AncestorType.GetTypeInfo().IsAssignableFrom(element.GetType().GetTypeInfo());

			bool fitsBindingContextType =
				element.BindingContext != null &&
				relativeSource.Mode == RelativeBindingSourceMode.FindAncestorBindingContext &&
				relativeSource.AncestorType.GetTypeInfo().IsAssignableFrom(element.BindingContext.GetType().GetTypeInfo());

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

		[Obsolete]
		[EditorBrowsable(EditorBrowsableState.Never)]
		static string GetBindingPath<TSource>(Expression<Func<TSource, object>> propertyGetter)
		{
			Expression expr = propertyGetter.Body;

			var unary = expr as UnaryExpression;
			if (unary != null)
				expr = unary.Operand;

			var builder = new StringBuilder();

			var indexed = false;

			var member = expr as MemberExpression;
			if (member == null)
			{
				var methodCall = expr as MethodCallExpression;
				if (methodCall != null)
				{
					if (methodCall.Arguments.Count == 0)
						throw new ArgumentException("Method calls are not allowed in binding expression");

					var arguments = new List<string>(methodCall.Arguments.Count);
					foreach (Expression arg in methodCall.Arguments)
					{
						if (arg.NodeType != ExpressionType.Constant)
							throw new ArgumentException("Only constants can be used as indexer arguments");

						object value = ((ConstantExpression)arg).Value;
						arguments.Add(value != null ? value.ToString() : "null");
					}

					Type declarerType = methodCall.Method.DeclaringType;
					DefaultMemberAttribute defaultMember = declarerType.GetTypeInfo().GetCustomAttributes(typeof(DefaultMemberAttribute), true).OfType<DefaultMemberAttribute>().FirstOrDefault();
					string indexerName = defaultMember != null ? defaultMember.MemberName : "Item";

					MethodInfo getterInfo =
						declarerType.GetProperties().Where(pi => pi.Name == indexerName && pi.CanRead && pi.GetMethod.IsPublic && !pi.GetMethod.IsStatic).Select(pi => pi.GetMethod).FirstOrDefault();
					if (getterInfo != null)
					{
						if (getterInfo == methodCall.Method)
						{
							indexed = true;
							builder.Append("[");

							var first = true;
							foreach (string argument in arguments)
							{
								if (!first)
									builder.Append(",");

								builder.Append(argument);
								first = false;
							}

							builder.Append("]");

							member = methodCall.Object as MemberExpression;
						}
						else
							throw new ArgumentException("Method calls are not allowed in binding expressions");
					}
					else
						throw new ArgumentException("Public indexer not found");
				}
				else
					throw new ArgumentException("Invalid expression type");
			}

			while (member != null)
			{
				var property = (PropertyInfo)member.Member;
				if (builder.Length != 0)
				{
					if (!indexed)
						builder.Insert(0, ".");
					else
						indexed = false;
				}

				builder.Insert(0, property.Name);

				//				member = member.Expression as MemberExpression ?? (member.Expression as UnaryExpression)?.Operand as MemberExpression;
				member = member.Expression as MemberExpression ?? (member.Expression is UnaryExpression ? (member.Expression as UnaryExpression).Operand as MemberExpression : null);
			}

			return builder.ToString();
		}
	}
}