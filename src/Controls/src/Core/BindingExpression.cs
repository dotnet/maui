#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml.Diagnostics;

namespace Microsoft.Maui.Controls
{
	[RequiresUnreferencedCode(TrimmerConstants.StringPathBindingWarning, Url = TrimmerConstants.ExpressionBasedBindingsDocsUrl)]
	internal sealed class BindingExpression
	{
		internal const string PropertyNotFoundErrorMessage = "'{0}' property not found on '{1}', target property: '{2}.{3}'";
		internal const string CannotConvertTypeErrorMessage = "'{0}' cannot be converted to type '{1}'";
		internal const string ParseIndexErrorMessage = "'{0}' could not be parsed as an index for a '{1}'";
		static readonly char[] ExpressionSplit = new[] { '.' };

		readonly List<BindingExpressionPart> _parts = new List<BindingExpressionPart>();

		BindableProperty _targetProperty;
		WeakReference<object> _weakSource;
		WeakReference<BindableObject> _weakTarget;
		List<WeakReference<Element>> _ancestryChain;
		bool _isBindingContextRelativeSource;
		SetterSpecificity _specificity;

		internal BindingExpression(BindingBase binding, string path)
		{
			Binding = binding ?? throw new ArgumentNullException(nameof(binding));
			Path = path ?? throw new ArgumentNullException(nameof(path));

			ParsePath();
		}

		internal BindingBase Binding { get; }

		internal string Path { get; }

		/// <summary>
		///     Applies the binding expression to a previously set source and target.
		/// </summary>
		internal void Apply(bool fromTarget = false)
		{
			if (_weakSource == null || _weakTarget == null)
				return;

			if (!_weakTarget.TryGetTarget(out BindableObject target))
			{
				Unapply();
				return;
			}

			if (_weakSource.TryGetTarget(out var source) && _targetProperty != null)
				ApplyCore(source, target, _targetProperty, fromTarget, _specificity);
		}

		/// <summary>
		///     Applies the binding expression to a new source or target.
		/// </summary>
		internal void Apply(object sourceObject, BindableObject target, BindableProperty property, SetterSpecificity specificity)
		{
			if (Binding is Binding { Source: var source, DataType: Type dataType })
			{
				// Do not check type mismatch if this is a binding with Source and compilation of bindings with Source is disale
				bool skipTypeMismatchCheck = source is not null && !RuntimeFeature.IsXamlCBindingWithSourceCompilationEnabled;
				if (!skipTypeMismatchCheck)
				{
					if (sourceObject != null && !dataType.IsAssignableFrom(sourceObject.GetType()))
					{
						BindingDiagnostics.SendBindingFailure(Binding, "Binding", $"Mismatch between the specified x:DataType ({dataType}) and the current binding context ({sourceObject.GetType()}).");
						sourceObject = null;
					}
				}
			}

			_targetProperty = property;
			_specificity = specificity;

			if (_weakTarget != null && _weakTarget.TryGetTarget(out BindableObject prevTarget) && !ReferenceEquals(prevTarget, target))
				throw new InvalidOperationException("Binding instances cannot be reused");

			if (_weakSource != null && _weakSource.TryGetTarget(out var previousSource) && !ReferenceEquals(previousSource, sourceObject))
				throw new InvalidOperationException("Binding instances cannot be reused");

			_weakSource = new WeakReference<object>(sourceObject);
			_weakTarget = new WeakReference<BindableObject>(target);

			ApplyCore(sourceObject, target, property, false, specificity);
		}

		internal void Unapply()
		{
			if (_weakSource != null && _weakSource.TryGetTarget(out var sourceObject))
			{
				for (var i = 0; i < _parts.Count - 1; i++)
				{
					BindingExpressionPart part = _parts[i];

					if (!part.IsSelf)
						part.TryGetValue(sourceObject, out sourceObject);

					part.Unsubscribe();
				}
			}

			_weakSource = null;
			_weakTarget = null;

			ClearAncestryChangeSubscriptions();
		}

		/// <summary>
		///     Applies the binding expression to a previously set source or target.
		/// </summary>
		void ApplyCore(object sourceObject, BindableObject target, BindableProperty property, bool fromTarget, SetterSpecificity specificity)
		{
			BindingMode mode = Binding.GetRealizedMode(_targetProperty);
			if ((mode == BindingMode.OneWay || mode == BindingMode.OneTime) && fromTarget)
				return;

			bool needsGetter = (mode == BindingMode.TwoWay && !fromTarget) || mode == BindingMode.OneWay || mode == BindingMode.OneTime;
			bool needsSetter = !needsGetter && ((mode == BindingMode.TwoWay && fromTarget) || mode == BindingMode.OneWayToSource);

			object current = sourceObject;
			BindingExpressionPart part = null;

			for (var i = 0; i < _parts.Count; i++)
			{
				part = _parts[i];

				if (!part.IsSelf && current != null)
				{
					// Allow the object instance itself to provide its own TypeInfo 
					TypeInfo currentType = current is IReflectableType reflectable ? reflectable.GetTypeInfo() : current.GetType().GetTypeInfo();
					if (part.LastGetter == null || !part.LastGetter.DeclaringType.GetTypeInfo().IsAssignableFrom(currentType))
						SetupPart(currentType, part);

					if (i < _parts.Count - 1)
						part.TryGetValue(current, out current);
				}

				if (!part.IsSelf
					&& current != null
					&& ((needsGetter && part.LastGetter == null)
						|| (needsSetter && part.NextPart == null && part.LastSetter == null)))
				{
					BindingDiagnostics.SendBindingFailure(Binding, current, target, property, "Binding", PropertyNotFoundErrorMessage, part.Content, current, target.GetType(), property.PropertyName);
					break;
				}

				if (part.NextPart != null && (mode == BindingMode.OneWay || mode == BindingMode.TwoWay)
					&& current is INotifyPropertyChanged inpc)
					part.Subscribe(inpc);
			}

			Debug.Assert(part != null, "There should always be at least the self part in the expression.");

			if (needsGetter)
			{
				if (part.TryGetValue(current, out object value) || part.IsSelf)
				{
					value = Binding.GetSourceValue(value, property.ReturnType);
				}
				else
					value = Binding.FallbackValue ?? property.GetDefaultValue(target);

				if (!BindingExpressionHelper.TryConvert(ref value, property, property.ReturnType, true))
				{
					BindingDiagnostics.SendBindingFailure(Binding, current, target, property, "Binding", CannotConvertTypeErrorMessage, value, property.ReturnType);
					return;
				}

				target.SetValueCore(property, value, SetValueFlags.ClearDynamicResource, BindableObject.SetValuePrivateFlags.Default | BindableObject.SetValuePrivateFlags.Converted, specificity);
			}
			else if (needsSetter && part.LastSetter != null && current != null)
			{
				object value = Binding.GetTargetValue(target.GetValue(property), part.SetterType);

				if (!BindingExpressionHelper.TryConvert(ref value, property, part.SetterType, false))
				{
					BindingDiagnostics.SendBindingFailure(Binding, current, target, property, "Binding", CannotConvertTypeErrorMessage, value, part.SetterType);
					return;
				}

				object[] args;
				if (part.IsIndexer)
				{
					args = new object[part.Arguments.Length + 1];
					part.Arguments.CopyTo(args, 0);
					args[args.Length - 1] = value;
				}
				else if (part.IsBindablePropertySetter)
				{
					args = new[] { part.BindablePropertyField, value };
				}
				else
				{
					args = new[] { value };
				}

				part.LastSetter.Invoke(current, args);
			}
		}

		void ParsePath()
		{
			string p = Path.Trim();

			var last = new BindingExpressionPart(this, ".");
			_parts.Add(last);

			if (p[0] == '.')
			{
				if (p.Length == 1)
					return;

				p = p.Substring(1);
			}

			string[] pathParts = p.Split(ExpressionSplit);
			for (var i = 0; i < pathParts.Length; i++)
			{
				string part = pathParts[i].Trim();
				if (part == string.Empty)
					throw new FormatException("Path contains an empty part");

				BindingExpressionPart indexer = null;

				int lbIndex = part.IndexOf("[", StringComparison.Ordinal);
				if (lbIndex != -1)
				{
					int rbIndex = part.Length - 1;
					if (part[rbIndex] != ']')
						throw new FormatException("Indexer did not contain closing bracket");

					int argLength = rbIndex - lbIndex - 1;
					if (argLength == 0)
						throw new FormatException("Indexer did not contain arguments");

					string argString = part.Substring(lbIndex + 1, argLength);
					indexer = new BindingExpressionPart(this, argString, true);

					part = part.Substring(0, lbIndex);
					part = part.Trim();
				}
				if (part.Length > 0)
				{
					var next = new BindingExpressionPart(this, part);
					last.NextPart = next;
					_parts.Add(next);
					last = next;
				}
				if (indexer != null)
				{
					last.NextPart = indexer;
					_parts.Add(indexer);
					last = indexer;
				}
			}
		}

		PropertyInfo GetIndexer(TypeInfo sourceType, string indexerName, string content)
		{
			if (int.TryParse(content, out _))
			{ //try to find an indexer taking an int
				foreach (var pi in sourceType.DeclaredProperties)
				{
					if (pi.Name != indexerName)
						continue;
					if (pi.CanRead && pi.GetMethod.GetParameters()[0].ParameterType == typeof(int))
						return pi;
					if (pi.CanWrite && pi.SetMethod.ReturnType == typeof(int))
						return pi;
				}
			}


			//property isn't an int, or there wasn't any int indexer
			foreach (var pi in sourceType.DeclaredProperties)
			{
				if (pi.Name != indexerName)
					continue;
				if (pi.CanRead && pi.GetMethod.GetParameters()[0].ParameterType == typeof(string))
					return pi;
				if (pi.CanWrite && pi.SetMethod.ReturnType == typeof(string))
					return pi;
			}

			//try to fallback to an object indexer
			foreach (var pi in sourceType.DeclaredProperties)
			{
				if (pi.Name != indexerName)
					continue;
				if (pi.CanRead && pi.GetMethod.GetParameters()[0].ParameterType == typeof(object))
					return pi;
				if (pi.CanWrite && pi.SetMethod.ReturnType == typeof(object))
					return pi;
			}

			//defined on a base class ?
			if (sourceType.BaseType is Type baseT && GetIndexer(baseT.GetTypeInfo(), indexerName, content) is PropertyInfo p)
				return p;

			//defined on an interface ?
			foreach (var face in sourceType.ImplementedInterfaces)
			{
				if (GetIndexer(face.GetTypeInfo(), indexerName, content) is PropertyInfo pi)
					return pi;
			}

			return null;
		}


		void SetupPart(TypeInfo sourceType, BindingExpressionPart part)
		{
			part.Arguments = null;
			part.LastGetter = null;
			part.LastSetter = null;

			PropertyInfo property = null;
			if (part.IsIndexer)
			{
				if (sourceType.IsArray)
				{
					if (!int.TryParse(part.Content, out var index))
					{
						BindingDiagnostics.SendBindingFailure(Binding, "Binding", ParseIndexErrorMessage, part.Content, sourceType);
					}
					else
						part.Arguments = new object[] { index };

					part.LastGetter = sourceType.GetDeclaredMethod("Get");
					part.LastSetter = sourceType.GetDeclaredMethod("Set");
					part.SetterType = sourceType.GetElementType();
				}

				string indexerName = "Item";
				var defaultMemberAttribute = (DefaultMemberAttribute)sourceType.GetCustomAttribute(typeof(DefaultMemberAttribute), true);
				if (defaultMemberAttribute != null)
				{
					indexerName = defaultMemberAttribute.MemberName;
				}

				part.IndexerName = indexerName;

				property = GetIndexer(sourceType, indexerName, part.Content);

				if (property != null)
				{
					ParameterInfo parameter = null;
					ParameterInfo[] array = property.GetIndexParameters();

					if (array.Length > 0)
						parameter = array[0];

					if (parameter != null)
					{
						try
						{
							object arg = Convert.ChangeType(part.Content, parameter.ParameterType, CultureInfo.InvariantCulture);
							part.Arguments = new[] { arg };
						}
						catch (FormatException)
						{
						}
						catch (InvalidCastException)
						{
						}
						catch (OverflowException)
						{
						}
					}
				}
			}
			else
			{
				TypeInfo type = sourceType;
				do
				{
					property = type.GetDeclaredProperty(part.Content);
				} while (property == null && (type = type.BaseType?.GetTypeInfo()) != null);
			}
			if (property != null)
			{
				var propertyType = property.PropertyType;

				if (property is { CanRead: true, GetMethod: { IsPublic: true, IsStatic: false } propertyGetMethod })
				{
					part.LastGetter = propertyGetMethod;
				}

				if (property is { CanWrite: true, SetMethod: { IsPublic: true, IsStatic: false } propertySetMethod })
				{
					part.LastSetter = propertySetMethod;
					part.SetterType = propertyType;

					if (Binding.AllowChaining)
					{
						FieldInfo bindablePropertyField = sourceType.GetDeclaredField(part.Content + "Property");
						if (bindablePropertyField != null && bindablePropertyField.FieldType == typeof(BindableProperty) && sourceType.ImplementedInterfaces.Contains(typeof(IElementController)))
						{
							MethodInfo setValueMethod = typeof(IElementController).GetMethod(nameof(IElementController.SetValueFromRenderer), new[] { typeof(BindableProperty), typeof(object) });
							if (setValueMethod != null)
							{
								part.LastSetter = setValueMethod;
								part.IsBindablePropertySetter = true;
								part.BindablePropertyField = bindablePropertyField.GetValue(null);
							}
						}
					}
				}

				if (part.NextPart != null && propertyType.IsGenericType && propertyType.IsValueType)
				{
					Type genericTypeDefinition = propertyType.GetGenericTypeDefinition();
					if ((genericTypeDefinition == typeof(ValueTuple<>)
						|| genericTypeDefinition == typeof(ValueTuple<,>)
						|| genericTypeDefinition == typeof(ValueTuple<,,>)
						|| genericTypeDefinition == typeof(ValueTuple<,,,>)
						|| genericTypeDefinition == typeof(ValueTuple<,,,,>)
						|| genericTypeDefinition == typeof(ValueTuple<,,,,,>)
						|| genericTypeDefinition == typeof(ValueTuple<,,,,,,>)
						|| genericTypeDefinition == typeof(ValueTuple<,,,,,,,>))
						&& property.GetCustomAttribute(typeof(TupleElementNamesAttribute)) is TupleElementNamesAttribute tupleEltNames)
					{
						//modify the nextPart to access the tuple item via the ITuple indexer
						var nextPart = part.NextPart;
						var name = nextPart.Content;
						var index = tupleEltNames.TransformNames.IndexOf(name);
						if (index >= 0)
						{
							nextPart.IsIndexer = true;
							nextPart.Content = index.ToString();
						}
					}
				}
			}
		}

		// SubscribeToAncestryChanges, ClearAncestryChangeSubscriptions, FindAncestryIndex, and
		// OnElementParentSet are used with RelativeSource ancestor-type bindings, to detect when
		// there has been an ancestry change requiring re-applying the binding, and to minimize
		// re-applications especially during visual tree building.
		internal void SubscribeToAncestryChanges(List<Element> chain, bool includeBindingContext, bool rootIsSource)
		{
			ClearAncestryChangeSubscriptions();
			if (chain == null)
				return;
			_isBindingContextRelativeSource = includeBindingContext;
			_ancestryChain = new List<WeakReference<Element>>();
			for (int i = 0; i < chain.Count; i++)
			{
				var elem = chain[i];
				if (i != chain.Count - 1 || !rootIsSource)
					// don't care about a successfully resolved source's parents
					elem.ParentSet += OnElementParentSet;
				if (_isBindingContextRelativeSource)
					elem.BindingContextChanged += OnElementBindingContextChanged;
				_ancestryChain.Add(new WeakReference<Element>(elem));
			}
		}

		void ClearAncestryChangeSubscriptions(int beginningWith = 0)
		{
			if (_ancestryChain == null || _ancestryChain.Count == 0)
				return;
			int count = _ancestryChain.Count;
			for (int i = beginningWith; i < count; i++)
			{
				Element elem;
				var weakElement = _ancestryChain.Last();
				if (weakElement.TryGetTarget(out elem))
				{
					elem.ParentSet -= OnElementParentSet;
					if (_isBindingContextRelativeSource)
						elem.BindingContextChanged -= OnElementBindingContextChanged;
				}
				_ancestryChain.RemoveAt(_ancestryChain.Count - 1);
			}
		}

		// Returns -1 if the member is not in the chain or the
		// chain is no longer valid.
		int FindAncestryIndex(Element elem)
		{
			for (int i = 0; i < _ancestryChain.Count; i++)
			{
				WeakReference<Element> weak = _ancestryChain[i];
				Element chainMember = null;
				if (!weak.TryGetTarget(out chainMember))
					return -1;
				else if (object.Equals(elem, chainMember))
					return i;
			}
			return -1;
		}

		void OnElementBindingContextChanged(object sender, EventArgs e)
		{
			if (!(sender is Element elem) ||
				!(this.Binding is Binding binding))
				return;

			BindableObject target = null;
			if (_weakTarget?.TryGetTarget(out target) != true)
				return;

			object currentSource = null;
			if (_weakSource?.TryGetTarget(out currentSource) == true)
			{
				// make sure that this isn't just a repeat notice
				// from someone else in the chain about our already-resolved 
				// binding source
				if (object.ReferenceEquals(currentSource, elem.BindingContext))
					return;
			}

			binding.Unapply();
			binding.Apply(null, target, _targetProperty, false, SetterSpecificity.FromBinding);
		}

		void OnElementParentSet(object sender, EventArgs e)
		{
			if (!(sender is Element elem) ||
				!(this.Binding is Binding binding))
				return;

			BindableObject target = null;
			if (_weakTarget?.TryGetTarget(out target) != true)
				return;

			if (elem.Parent == null)
			{
				// Remove anything further up in the chain
				// than the element with the null parent
				int index = FindAncestryIndex(elem);
				if (index == -1)
				{
					binding.Unapply();
					return;
				}
				if (index + 1 < _ancestryChain.Count)
					ClearAncestryChangeSubscriptions(index + 1);

				// Force the binding expression to resolve to null
				// for now, until someone in the chain gets a new
				// non-null parent.
				this.ApplyCore(null, target, _targetProperty, false, _specificity);
			}
			else
			{
				binding.Unapply();
				binding.Apply(null, target, _targetProperty, false, _specificity);
			}
		}

		private sealed class BindingPair
		{
			public BindingPair(BindingExpressionPart part, object source, bool isLast)
			{
				Part = part;
				Source = source;
				IsLast = isLast;
			}

			public bool IsLast { get; set; }

			public BindingExpressionPart Part { get; private set; }

			public object Source { get; private set; }
		}

		internal sealed class WeakPropertyChangedProxy : WeakEventProxy<INotifyPropertyChanged, PropertyChangedEventHandler>
		{
			public WeakPropertyChangedProxy() { }

			public WeakPropertyChangedProxy(INotifyPropertyChanged source, PropertyChangedEventHandler listener)
			{
				Subscribe(source, listener);
			}



			public override void Subscribe(INotifyPropertyChanged source, PropertyChangedEventHandler listener)
			{
				source.PropertyChanged += OnPropertyChanged;
				if (source is BindableObject bo)
					bo.BindingContextChanged += OnBCChanged;

				base.Subscribe(source, listener);
			}

			public override void Unsubscribe()
			{
				if (TryGetSource(out var source))
					source.PropertyChanged -= OnPropertyChanged;
				if (source is BindableObject bo)
					bo.BindingContextChanged -= OnBCChanged;

				base.Unsubscribe();
			}

			void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
			{
				if (TryGetHandler(out var handler))
					handler(sender, e);
				else
					Unsubscribe();
			}

			void OnBCChanged(object sender, EventArgs e)
			{
				OnPropertyChanged(sender, new PropertyChangedEventArgs(nameof(BindableObject.BindingContext)));
			}
		}

		private sealed class BindingExpressionPart
		{
			readonly BindingExpression _expression;
			readonly PropertyChangedEventHandler _changeHandler;
			WeakPropertyChangedProxy _listener;

			~BindingExpressionPart() => _listener?.Unsubscribe();

			public BindingExpressionPart(BindingExpression expression, string content, bool isIndexer = false)
			{
				_expression = expression;
				IsSelf = content == Maui.Controls.Binding.SelfPath;
				Content = content;
				IsIndexer = isIndexer;

				_changeHandler = PropertyChanged;
			}

			public void Subscribe(INotifyPropertyChanged handler)
			{
				INotifyPropertyChanged source;
				if (_listener != null && _listener.TryGetSource(out source) && ReferenceEquals(handler, source))
					// Already subscribed
					return;

				// Clear out the old subscription if necessary
				Unsubscribe();

				_listener = new WeakPropertyChangedProxy(handler, _changeHandler);
			}

			public void Unsubscribe()
			{
				var listener = _listener;
				if (listener != null)
				{
					listener.Unsubscribe();
					_listener = null;
				}
			}

			public object[] Arguments { get; set; }

			public object BindablePropertyField { get; set; }

			public string Content { get; internal set; }

			public string IndexerName { get; set; }

			public bool IsBindablePropertySetter { get; set; }

			public bool IsIndexer { get; internal set; }

			public bool IsSelf { get; }

			public MethodInfo LastGetter { get; set; }

			public MethodInfo LastSetter { get; set; }

			public BindingExpressionPart NextPart { get; set; }

			public Type SetterType { get; set; }

			public void PropertyChanged(object sender, PropertyChangedEventArgs args)
			{
				BindingExpressionPart part = NextPart ?? this;

				string name = args.PropertyName;

				if (!string.IsNullOrEmpty(name))
				{
					if (part.IsIndexer)
					{
						if (name.IndexOf("[", StringComparison.Ordinal) != -1)
						{
							if (name != string.Format("{0}[{1}]", part.IndexerName, part.Content))
								return;
						}
						else if (name != part.IndexerName)
							return;
					}
					else if (name != part.Content)
					{
						return;
					}
				}

				if (_expression._weakTarget is not null && _expression._weakTarget.TryGetTarget(out BindableObject obj))
					obj.Dispatcher.DispatchIfRequired(() => _expression.Apply());
				else
					_expression.Apply();
			}

			public bool TryGetValue(object source, out object value)
			{
				value = source;

				if (LastGetter != null && value != null)
				{
					if (IsIndexer)
					{
						try
						{
							value = LastGetter.Invoke(value, Arguments);
						}
						catch (TargetInvocationException ex)
						{
							if (ex.InnerException is KeyNotFoundException || ex.InnerException is IndexOutOfRangeException || ex.InnerException is ArgumentOutOfRangeException)
							{
								value = null;
								return false;
							}
							else
								throw ex.InnerException;
						}
						return true;
					}
					value = LastGetter.Invoke(value, Arguments);
					return true;
				}

				return false;
			}
		}
	}
}
