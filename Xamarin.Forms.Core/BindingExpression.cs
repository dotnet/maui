using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	internal class BindingExpression
	{
		internal const string PropertyNotFoundErrorMessage = "'{0}' property not found on '{1}', target property: '{2}.{3}'";
		static readonly char[] ExpressionSplit = new[] { '.' };

		readonly List<BindingExpressionPart> _parts = new List<BindingExpressionPart>();

		BindableProperty _targetProperty;
		WeakReference<object> _weakSource;
		WeakReference<BindableObject> _weakTarget;
		List<WeakReference<Element>> _ancestryChain;
		bool _isBindingContextRelativeSource;

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
				ApplyCore(source, target, _targetProperty, fromTarget);
		}

		/// <summary>
		///     Applies the binding expression to a new source or target.
		/// </summary>
		internal void Apply(object sourceObject, BindableObject target, BindableProperty property)
		{
			_targetProperty = property;

			if (_weakTarget != null && _weakTarget.TryGetTarget(out BindableObject prevTarget) && !ReferenceEquals(prevTarget, target))
				throw new InvalidOperationException("Binding instances can not be reused");

			if (_weakSource != null && _weakSource.TryGetTarget(out var previousSource) && !ReferenceEquals(previousSource, sourceObject))
				throw new InvalidOperationException("Binding instances can not be reused");

			_weakSource = new WeakReference<object>(sourceObject);
			_weakTarget = new WeakReference<BindableObject>(target);

			ApplyCore(sourceObject, target, property);
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
		void ApplyCore(object sourceObject, BindableObject target, BindableProperty property, bool fromTarget = false)
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
					Log.Warning("Binding", PropertyNotFoundErrorMessage, part.Content, current, target.GetType(), property.PropertyName);
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

				if (!TryConvert(ref value, property, property.ReturnType, true))
				{
					Log.Warning("Binding", "'{0}' can not be converted to type '{1}'.", value, property.ReturnType);
					return;
				}

				target.SetValueCore(property, value, SetValueFlags.ClearDynamicResource, BindableObject.SetValuePrivateFlags.Default | BindableObject.SetValuePrivateFlags.Converted);
			}
			else if (needsSetter && part.LastSetter != null && current != null)
			{
				object value = Binding.GetTargetValue(target.GetValue(property), part.SetterType);

				if (!TryConvert(ref value, property, part.SetterType, false))
				{
					Log.Warning("Binding", "'{0}' can not be converted to type '{1}'.", value, part.SetterType);
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

				int lbIndex = part.IndexOf('[');
				if (lbIndex != -1)
				{
					int rbIndex = part.LastIndexOf(']');
					if (rbIndex == -1)
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
						Log.Warning("Binding", "{0} could not be parsed as an index for a {1}", part.Content, sourceType);
					else
						part.Arguments = new object[] { index };

					part.LastGetter = sourceType.GetDeclaredMethod("Get");
					part.LastSetter = sourceType.GetDeclaredMethod("Set");
					part.SetterType = sourceType.GetElementType();
				}

				string indexerName = "Item";
				foreach (DefaultMemberAttribute attrib in sourceType.GetCustomAttributes(typeof(DefaultMemberAttribute), true))
				{
					indexerName = attrib.MemberName;
					break;
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
				while (type != null && property == null)
				{
					property = type.GetDeclaredProperty(part.Content);
					type = type.BaseType?.GetTypeInfo();
				}
			}
			if (property != null)
			{
				if (property.CanRead && property.GetMethod.IsPublic && !property.GetMethod.IsStatic)
					part.LastGetter = property.GetMethod;
				if (property.CanWrite && property.SetMethod.IsPublic && !property.SetMethod.IsStatic)
				{
					part.LastSetter = property.SetMethod;
					var lastSetterParameters = part.LastSetter.GetParameters();
					part.SetterType = lastSetterParameters[lastSetterParameters.Length - 1].ParameterType;

					if (Binding.AllowChaining)
					{
						FieldInfo bindablePropertyField = sourceType.GetDeclaredField(part.Content + "Property");
						if (bindablePropertyField != null && bindablePropertyField.FieldType == typeof(BindableProperty) && sourceType.ImplementedInterfaces.Contains(typeof(IElementController)))
						{
							MethodInfo setValueMethod = null;
#if NETSTANDARD1_0
							foreach (MethodInfo m in sourceType.AsType().GetRuntimeMethods())
							{
								if (m.Name.EndsWith("IElementController.SetValueFromRenderer"))
								{
									ParameterInfo[] parameters = m.GetParameters();
									if (parameters.Length == 2 && parameters[0].ParameterType == typeof(BindableProperty))
									{
										setValueMethod = m;
										break;
									}
								}
							}
#else
							setValueMethod = typeof(IElementController).GetMethod("SetValueFromRenderer", new[] { typeof(BindableProperty), typeof(object) });
#endif
							if (setValueMethod != null)
							{
								part.LastSetter = setValueMethod;
								part.IsBindablePropertySetter = true;
								part.BindablePropertyField = bindablePropertyField.GetValue(null);
							}
						}
					}
				}
#if !NETSTANDARD1_0
				if (property != null
					&& part.NextPart != null
					&& property.PropertyType.IsGenericType
					&& (property.PropertyType.GetGenericTypeDefinition() == typeof(ValueTuple<>)
						|| property.PropertyType.GetGenericTypeDefinition() == typeof(ValueTuple<,>)
						|| property.PropertyType.GetGenericTypeDefinition() == typeof(ValueTuple<,,>)
						|| property.PropertyType.GetGenericTypeDefinition() == typeof(ValueTuple<,,,>)
						|| property.PropertyType.GetGenericTypeDefinition() == typeof(ValueTuple<,,,,>)
						|| property.PropertyType.GetGenericTypeDefinition() == typeof(ValueTuple<,,,,,>)
						|| property.PropertyType.GetGenericTypeDefinition() == typeof(ValueTuple<,,,,,,>)
						|| property.PropertyType.GetGenericTypeDefinition() == typeof(ValueTuple<,,,,,,,>))
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
#endif
			}

		}
		static readonly Type[] DecimalTypes = { typeof(float), typeof(decimal), typeof(double) };

		internal static bool TryConvert(ref object value, BindableProperty targetProperty, Type convertTo, bool toTarget)
		{
			if (value == null)
				return !convertTo.GetTypeInfo().IsValueType || Nullable.GetUnderlyingType(convertTo) != null;
			try
			{
				if ((toTarget && targetProperty.TryConvert(ref value)) || (!toTarget && convertTo.IsInstanceOfType(value)))
					return true;
			}
			catch (InvalidOperationException)
			{ //that's what TypeConverters ususally throw
				return false;
			}

			object original = value;
			try
			{
				var stringValue = value as string ?? string.Empty;
				// see: https://bugzilla.xamarin.com/show_bug.cgi?id=32871
				// do not canonicalize "*.[.]"; "1." should not update bound BindableProperty
				if (stringValue.EndsWith(".", StringComparison.Ordinal) && DecimalTypes.Contains(convertTo))
				{
					value = original;
					return false;
				}

				// do not canonicalize "-0"; user will likely enter a period after "-0"
				if (stringValue == "-0" && DecimalTypes.Contains(convertTo))
				{
					value = original;
					return false;
				}

				convertTo = Nullable.GetUnderlyingType(convertTo) ?? convertTo;

				value = Convert.ChangeType(value, convertTo, CultureInfo.InvariantCulture);
				return true;
			}
			catch (Exception ex) when (ex is InvalidCastException || ex is FormatException || ex is InvalidOperationException || ex is OverflowException)
			{
				value = original;
				return false;
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
			binding.Apply(null, target, _targetProperty);
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
				this.ApplyCore(null, target, _targetProperty);
			}
			else
			{
				binding.Unapply();
				binding.Apply(null, target, _targetProperty);
			}
		}

		class BindingPair
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

		internal class WeakPropertyChangedProxy
		{
			readonly WeakReference<INotifyPropertyChanged> _source = new WeakReference<INotifyPropertyChanged>(null);
			readonly WeakReference<PropertyChangedEventHandler> _listener = new WeakReference<PropertyChangedEventHandler>(null);
			readonly PropertyChangedEventHandler _handler;
			readonly EventHandler _bchandler;
			internal WeakReference<INotifyPropertyChanged> Source => _source;

			public WeakPropertyChangedProxy()
			{
				_handler = new PropertyChangedEventHandler(OnPropertyChanged);
				_bchandler = new EventHandler(OnBCChanged);
			}

			public WeakPropertyChangedProxy(INotifyPropertyChanged source, PropertyChangedEventHandler listener) : this()
			{
				SubscribeTo(source, listener);
			}

			public void SubscribeTo(INotifyPropertyChanged source, PropertyChangedEventHandler listener)
			{
				source.PropertyChanged += _handler;
				var bo = source as BindableObject;
				if (bo != null)
					bo.BindingContextChanged += _bchandler;
				_source.SetTarget(source);
				_listener.SetTarget(listener);
			}

			public void Unsubscribe()
			{
				INotifyPropertyChanged source;
				if (_source.TryGetTarget(out source) && source != null)
					source.PropertyChanged -= _handler;
				var bo = source as BindableObject;
				if (bo != null)
					bo.BindingContextChanged -= _bchandler;

				_source.SetTarget(null);
				_listener.SetTarget(null);
			}

			void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
			{
				if (_listener.TryGetTarget(out var handler) && handler != null)
					handler(sender, e);
				else
					Unsubscribe();
			}

			void OnBCChanged(object sender, EventArgs e)
			{
				OnPropertyChanged(sender, new PropertyChangedEventArgs("BindingContext"));
			}
		}

		class BindingExpressionPart
		{
			readonly BindingExpression _expression;
			readonly PropertyChangedEventHandler _changeHandler;
			WeakPropertyChangedProxy _listener;

			public BindingExpressionPart(BindingExpression expression, string content, bool isIndexer = false)
			{
				_expression = expression;
				IsSelf = content == Forms.Binding.SelfPath;
				Content = content;
				IsIndexer = isIndexer;

				_changeHandler = PropertyChanged;
			}

			public void Subscribe(INotifyPropertyChanged handler)
			{
				INotifyPropertyChanged source;
				if (_listener != null && _listener.Source.TryGetTarget(out source) && ReferenceEquals(handler, source))
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
						if (name.Contains("["))
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

				Action action = () => _expression.Apply();
				if (_expression._weakTarget != null && _expression._weakTarget.TryGetTarget(out BindableObject obj) && obj.Dispatcher != null && obj.Dispatcher.IsInvokeRequired)
				{
					obj.Dispatcher.BeginInvokeOnMainThread(action);
				}
				else if (Device.IsInvokeRequired)
				{
					Device.BeginInvokeOnMainThread(action);
				}
				else
				{
					action();
				}

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