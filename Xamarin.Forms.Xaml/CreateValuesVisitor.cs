using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml.Internals;

namespace Xamarin.Forms.Xaml
{
	class CreateValuesVisitor : IXamlNodeVisitor
	{
		public CreateValuesVisitor(HydrationContext context)
		{
			Context = context;
		}

		Dictionary<INode, object> Values
		{
			get { return Context.Values; }
		}

		HydrationContext Context { get; }

		public TreeVisitingMode VisitingMode => TreeVisitingMode.BottomUp;
		public bool StopOnDataTemplate => true;
		public bool StopOnResourceDictionary => false;
		public bool VisitNodeOnDataTemplate => false;
		public bool SkipChildren(INode node, INode parentNode) => false;
		public bool IsResourceDictionary(ElementNode node) => typeof(ResourceDictionary).IsAssignableFrom(Context.Types[node]);

		public void Visit(ValueNode node, INode parentNode)
		{
			Values[node] = node.Value;
		}

		public void Visit(MarkupNode node, INode parentNode)
		{
		}

		public void Visit(ElementNode node, INode parentNode)
		{
			object value = null;

			var type = XamlParser.GetElementType(node.XmlType, node, Context.RootElement?.GetType().GetTypeInfo().Assembly,
				out XamlParseException xpe);
			if (xpe != null)
			{
				if (Context.ExceptionHandler != null)
				{
					Context.ExceptionHandler(xpe);
					return;
				}
				throw xpe;
			}
			Context.Types[node] = type;
			if (IsXaml2009LanguagePrimitive(node))
				value = CreateLanguagePrimitive(type, node);
			else if (node.Properties.ContainsKey(XmlName.xArguments) || node.Properties.ContainsKey(XmlName.xFactoryMethod))
				value = CreateFromFactory(type, node);
			else if (
				type.GetTypeInfo()
					.DeclaredConstructors.Any(
						ci =>
							ci.IsPublic && ci.GetParameters().Length != 0 &&
							ci.GetParameters().All(pi => pi.CustomAttributes.Any(attr => attr.AttributeType == typeof(ParameterAttribute)))) &&
				ValidateCtorArguments(type, node, out string ctorargname))
				value = CreateFromParameterizedConstructor(type, node);
			else if (!type.GetTypeInfo().DeclaredConstructors.Any(ci => ci.IsPublic && ci.GetParameters().Length == 0) &&
					 !ValidateCtorArguments(type, node, out ctorargname))
			{
				throw new XamlParseException($"The Property {ctorargname} is required to create a {type.FullName} object.", node);
			}
			else
			{
				//this is a trick as the DataTemplate parameterless ctor is internal, and we can't CreateInstance(..., false) on WP7
				try
				{
					if (type == typeof(DataTemplate))
						value = new DataTemplate();
					if (type == typeof(ControlTemplate))
						value = new ControlTemplate();
					if (value == null && node.CollectionItems.Any() && node.CollectionItems.First() is ValueNode)
					{
						var serviceProvider = new XamlServiceProvider(node, Context);
						var converted = ((ValueNode)node.CollectionItems.First()).Value.ConvertTo(type, () => type.GetTypeInfo(),
							serviceProvider, out Exception exception);
						if (exception != null)
						{
							if (Context.ExceptionHandler != null)
							{
								Context.ExceptionHandler(exception);
								return;
							}
							throw exception;
						}
						if (converted != null && converted.GetType() == type)
							value = converted;
					}
					if (value == null)
					{
						try
						{
							value = Activator.CreateInstance(type);
						}
						catch (Exception e) when (e is TargetInvocationException || e is MemberAccessException)
						{
							value = XamlLoader.InstantiationFailedCallback?.Invoke(new XamlLoader.CallbackTypeInfo { XmlNamespace = node.XmlType.NamespaceUri, XmlTypeName = node.XmlType.Name }, type, e) ?? throw e;
						}
					}
				}
				catch (TargetInvocationException e) when (e.InnerException is XamlParseException || e.InnerException is XmlException)
				{
					throw e.InnerException;
				}
				catch (MissingMemberException mme)
				{
					throw new XamlParseException(mme.Message, node, mme);
				}
			}

			Values[node] = value;

			if (value is IMarkupExtension markup && (value is TypeExtension || value is StaticExtension || value is ArrayExtension))
			{
				var serviceProvider = new XamlServiceProvider(node, Context);

				var visitor = new ApplyPropertiesVisitor(Context);
				foreach (var cnode in node.Properties.Values.ToList())
					cnode.Accept(visitor, node);
				foreach (var cnode in node.CollectionItems)
					cnode.Accept(visitor, node);

				try
				{
					value = markup.ProvideValue(serviceProvider);
				}
				catch (Exception e)
				{
					var xamlpe = e as XamlParseException ?? new XamlParseException("Markup extension failed", serviceProvider, e);
					if (Context.ExceptionHandler != null)
					{
						Context.ExceptionHandler(xamlpe);
					}
					else
						throw xamlpe;
				}
				if (!node.Properties.TryGetValue(XmlName.xKey, out INode xKey))
					xKey = null;

				node.Properties.Clear();
				node.CollectionItems.Clear();

				if (xKey != null)
					node.Properties.Add(XmlName.xKey, xKey);

				Values[node] = value;
			}

			if (value is BindableObject bindableValue && node.NameScopeRef != (parentNode as IElementNode)?.NameScopeRef)
				NameScope.SetNameScope(bindableValue, node.NameScopeRef.NameScope);

			if (XamlLoader.ValueCreatedCallback != null)
			{
				var name = node.XmlType.Name;
				if (name.Contains(":"))
					name = name.Substring(name.LastIndexOf(':') + 1);
				XamlLoader.ValueCreatedCallback(new XamlLoader.CallbackTypeInfo { XmlNamespace = node.XmlType.NamespaceUri, XmlTypeName = name }, value);
			}

			var assemblyName = (Context.RootAssembly ?? Context.RootElement?.GetType().GetTypeInfo().Assembly)?.GetName().Name;
			if (assemblyName != null && value != null && !value.GetType().GetTypeInfo().IsValueType && XamlFilePathAttribute.GetFilePathForObject(Context.RootElement) is string path)
				Diagnostics.VisualDiagnostics.RegisterSourceInfo(value, new Uri($"{path};assembly={assemblyName}", UriKind.Relative), ((IXmlLineInfo)node).LineNumber, ((IXmlLineInfo)node).LinePosition);

		}

		public void Visit(RootNode node, INode parentNode)
		{
			var rnode = (XamlLoader.RuntimeRootNode)node;
			Values[node] = rnode.Root;
			Context.Types[node] = rnode.Root.GetType();
			if (rnode.Root is BindableObject bindable)
			{
				if (NameScope.GetNameScope(bindable) is INameScope existingNs)
					node.NameScopeRef.NameScope = existingNs;
				else
					NameScope.SetNameScope(bindable, node.NameScopeRef?.NameScope);
			}

			var assemblyName = (Context.RootAssembly ?? Context.RootElement.GetType().GetTypeInfo().Assembly)?.GetName().Name;
			if (rnode.Root != null && !rnode.Root.GetType().GetTypeInfo().IsValueType && XamlFilePathAttribute.GetFilePathForObject(Context.RootElement) is string path)
				Diagnostics.VisualDiagnostics.RegisterSourceInfo(rnode.Root, new Uri($"{path};assembly={assemblyName}", UriKind.Relative), ((IXmlLineInfo)node).LineNumber, ((IXmlLineInfo)node).LinePosition);
		}

		public void Visit(ListNode node, INode parentNode)
		{
			//this is a gross hack to keep ListNode alive. ListNode must go in favor of Properties
			if (ApplyPropertiesVisitor.TryGetPropertyName(node, parentNode, out XmlName name))
				node.XmlName = name;
		}

		bool ValidateCtorArguments(Type nodeType, IElementNode node, out string missingArgName)
		{
			missingArgName = null;
			var ctorInfo =
				nodeType.GetTypeInfo()
					.DeclaredConstructors.FirstOrDefault(
						ci =>
							ci.GetParameters().Length != 0 && ci.IsPublic &&
							ci.GetParameters().All(pi => pi.CustomAttributes.Any(attr => attr.AttributeType == typeof(ParameterAttribute))));
			if (ctorInfo == null)
				return true;
			foreach (var parameter in ctorInfo.GetParameters())
			{
				var propname =
					parameter.CustomAttributes.First(ca => ca.AttributeType.FullName == "Xamarin.Forms.ParameterAttribute")
						.ConstructorArguments.First()
						.Value as string;
				if (!node.Properties.ContainsKey(new XmlName("", propname)))
				{
					missingArgName = propname;
					return false;
				}
			}

			return true;
		}

		public object CreateFromParameterizedConstructor(Type nodeType, IElementNode node)
		{
			var ctorInfo =
				nodeType.GetTypeInfo()
					.DeclaredConstructors.FirstOrDefault(
						ci =>
							ci.GetParameters().Length != 0 && ci.IsPublic &&
							ci.GetParameters().All(pi => pi.CustomAttributes.Any(attr => attr.AttributeType == typeof(ParameterAttribute))));
			object[] arguments = CreateArgumentsArray(node, ctorInfo);
			try
			{
				return ctorInfo.Invoke(arguments);
			}
			catch (Exception e) when (e is TargetInvocationException || e is MissingMemberException)
			{
				return XamlLoader.InstantiationFailedCallback?.Invoke(new XamlLoader.CallbackTypeInfo { XmlNamespace = node.XmlType.NamespaceUri, XmlTypeName = node.XmlType.Name }, nodeType, e) ?? throw e;
			}

		}

		public object CreateFromFactory(Type nodeType, IElementNode node)
		{
			object[] arguments = CreateArgumentsArray(node);

			if (!node.Properties.ContainsKey(XmlName.xFactoryMethod))
			{
				//non-default ctor
				try
				{
					return Activator.CreateInstance(nodeType, arguments);
				}
				catch (Exception e) when (e is TargetInvocationException || e is MissingMemberException)
				{
					return XamlLoader.InstantiationFailedCallback?.Invoke(new XamlLoader.CallbackTypeInfo { XmlNamespace = node.XmlType.NamespaceUri, XmlTypeName = node.XmlType.Name }, nodeType, e) ?? throw e;
				}
			}

			var factoryMethod = ((string)((ValueNode)node.Properties[XmlName.xFactoryMethod]).Value);
			Type[] types = arguments == null ? new Type[0] : arguments.Select(a => a.GetType()).ToArray();

			bool isMatch(MethodInfo m)
			{
				if (m.Name != factoryMethod)
					return false;
				var p = m.GetParameters();
				if (p.Length != types.Length)
					return false;
				if (!m.IsStatic)
					return false;
				for (var i = 0; i < p.Length; i++)
				{
					if ((p[i].ParameterType.IsAssignableFrom(types[i])))
						continue;
					var op_impl = p[i].ParameterType.GetImplicitConversionOperator(fromType: types[i], toType: p[i].ParameterType)
								?? types[i].GetImplicitConversionOperator(fromType: types[i], toType: p[i].ParameterType);

					if (op_impl == null)
						return false;
					arguments[i] = op_impl.Invoke(null, new[] { arguments[i] });
				}
				return true;
			}

			try
			{
				var mi = nodeType.GetRuntimeMethods().FirstOrDefault(isMatch);
				if (mi == null)
					throw new MissingMemberException($"No static method found for {nodeType.FullName}::{factoryMethod} ({string.Join(", ", types.Select(t => t.FullName))})");
				return mi.Invoke(null, arguments);
			}
			catch (Exception e) when (e is TargetInvocationException || e is MissingMemberException)
			{
				return XamlLoader.InstantiationFailedCallback?.Invoke(new XamlLoader.CallbackTypeInfo { XmlNamespace = node.XmlType.NamespaceUri, XmlTypeName = node.XmlType.Name }, nodeType, e) ?? throw e;
			}
		}

		public object[] CreateArgumentsArray(IElementNode enode)
		{
			if (!enode.Properties.ContainsKey(XmlName.xArguments))
				return null;
			var node = enode.Properties[XmlName.xArguments];
			if (node is ElementNode elementNode)
			{
				var array = new object[1];
				array[0] = Values[elementNode];
				return array;
			}

			if (node is ListNode listnode)
			{
				var array = new object[listnode.CollectionItems.Count];
				for (var i = 0; i < listnode.CollectionItems.Count; i++)
					array[i] = Values[(ElementNode)listnode.CollectionItems[i]];
				return array;
			}
			return null;
		}

		public object[] CreateArgumentsArray(IElementNode enode, ConstructorInfo ctorInfo)
		{
			var n = ctorInfo.GetParameters().Length;
			var array = new object[n];
			for (var i = 0; i < n; i++)
			{
				var parameter = ctorInfo.GetParameters()[i];
				var propname =
					parameter.CustomAttributes.First(attr => attr.AttributeType == typeof(ParameterAttribute))
						.ConstructorArguments.First()
						.Value as string;
				var name = new XmlName("", propname);
				if (!enode.Properties.TryGetValue(name, out INode node))
					throw new XamlParseException($"The Property {propname} is required to create a {ctorInfo.DeclaringType.FullName} object.", enode as IXmlLineInfo);
				if (!enode.SkipProperties.Contains(name))
					enode.SkipProperties.Add(name);
				var value = Context.Values[node];
				var serviceProvider = new XamlServiceProvider(enode, Context);
				var convertedValue = value.ConvertTo(parameter.ParameterType, () => parameter, serviceProvider, out Exception e);
				if (e != null)
					throw e;
				array[i] = convertedValue;
			}

			return array;
		}

		static bool IsXaml2009LanguagePrimitive(IElementNode node) => node.NamespaceURI == XamlParser.X2009Uri;

		static object CreateLanguagePrimitive(Type nodeType, IElementNode node)
		{
			object value;
			if (nodeType == typeof(string))
				value = String.Empty;
			else if (nodeType == typeof(Uri))
				value = null;
			else
				value = Activator.CreateInstance(nodeType);

			if (node.CollectionItems.Count == 1
				&& node.CollectionItems[0] is ValueNode
				&& ((ValueNode)node.CollectionItems[0]).Value is string valuestring)
			{
				if (nodeType == typeof(SByte) && sbyte.TryParse(valuestring, NumberStyles.Number, CultureInfo.InvariantCulture, out var sbyteval))
					return sbyteval;
				if (nodeType == typeof(Int16) && short.TryParse(valuestring, NumberStyles.Number, CultureInfo.InvariantCulture, out var int16val))
					return int16val;
				if (nodeType == typeof(Int32) && int.TryParse(valuestring, NumberStyles.Number, CultureInfo.InvariantCulture, out var int32val))
					return int32val;
				if (nodeType == typeof(Int64) && long.TryParse(valuestring, NumberStyles.Number, CultureInfo.InvariantCulture, out var int64val))
					return int64val;
				if (nodeType == typeof(Byte) && byte.TryParse(valuestring, NumberStyles.Number, CultureInfo.InvariantCulture, out var byteval))
					return byteval;
				if (nodeType == typeof(UInt16) && ushort.TryParse(valuestring, NumberStyles.Number, CultureInfo.InvariantCulture, out var uint16val))
					return uint16val;
				if (nodeType == typeof(UInt32) && uint.TryParse(valuestring, NumberStyles.Number, CultureInfo.InvariantCulture, out var uint32val))
					return uint32val;
				if (nodeType == typeof(UInt64) && ulong.TryParse(valuestring, NumberStyles.Number, CultureInfo.InvariantCulture, out var uint64val))
					return uint64val;
				if (nodeType == typeof(Single) && float.TryParse(valuestring, NumberStyles.Number, CultureInfo.InvariantCulture, out var singleval))
					return singleval;
				if (nodeType == typeof(Double) && double.TryParse(valuestring, NumberStyles.Number, CultureInfo.InvariantCulture, out var doubleval))
					return doubleval;
				if (nodeType == typeof(Boolean) && bool.TryParse(valuestring, out var boolval))
					return boolval;
				if (nodeType == typeof(TimeSpan) && TimeSpan.TryParse(valuestring, CultureInfo.InvariantCulture, out TimeSpan timespanval))
					return timespanval;
				if (nodeType == typeof(char) && char.TryParse(valuestring, out var charval))
					return charval;
				if (nodeType == typeof(string))
					return valuestring;
				if (nodeType == typeof(decimal) && decimal.TryParse(valuestring, NumberStyles.Number, CultureInfo.InvariantCulture, out var decimalval))
					return decimalval;
				if (nodeType == typeof(Uri) && Uri.TryCreate(valuestring, UriKind.RelativeOrAbsolute, out Uri urival))
					return urival;
			}
			return value;
		}
	}
}