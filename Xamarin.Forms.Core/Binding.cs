using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

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
				throw new ArgumentNullException("path");
			if (string.IsNullOrWhiteSpace(path))
				throw new ArgumentException("path can not be an empty string", "path");

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
			}
		}

		internal string UpdateSourceEventName {
			get { return _updateSourceEventName; }
			set {
				ThrowIfApplied();
				_updateSourceEventName = value;
			}
		}

		[Obsolete]
		public static Binding Create<TSource>(Expression<Func<TSource, object>> propertyGetter, BindingMode mode = BindingMode.Default, IValueConverter converter = null, object converterParameter = null,
											  string stringFormat = null)
		{
			if (propertyGetter == null)
				throw new ArgumentNullException("propertyGetter");

			string path = GetBindingPath(propertyGetter);
			return new Binding(path, mode, converter, converterParameter, stringFormat);
		}

		internal override void Apply(bool fromTarget)
		{
			base.Apply(fromTarget);

			if (_expression == null)
				_expression = new BindingExpression(this, SelfPath);

			_expression.Apply(fromTarget);
		}

		internal override void Apply(object newContext, BindableObject bindObj, BindableProperty targetProperty)
		{
			object src = _source;
			base.Apply(src ?? newContext, bindObj, targetProperty);

			object bindingContext = src ?? Context ?? newContext;
			if (_expression == null && bindingContext != null)
				_expression = new BindingExpression(this, SelfPath);

			_expression.Apply(bindingContext, bindObj, targetProperty);
		}

		internal override BindingBase Clone()
		{
			return new Binding(Path, Mode) { Converter = Converter, ConverterParameter = ConverterParameter, StringFormat = StringFormat, Source = Source, UpdateSourceEventName = UpdateSourceEventName };
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

		internal override void Unapply()
		{
			base.Unapply();

			if (_expression != null)
				_expression.Unapply();
		}

		[Obsolete]
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