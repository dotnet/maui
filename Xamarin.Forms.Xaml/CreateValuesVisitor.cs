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
	internal class CreateValuesVisitor : IXamlNodeVisitor
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

			XamlParseException xpe;
			var type = XamlParser.GetElementType(node.XmlType, node, Context.RootElement?.GetType().GetTypeInfo().Assembly,
				out xpe);
			if (xpe != null)
				throw xpe;

			Context.Types[node] = type;
			string ctorargname;
			if (IsXaml2009LanguagePrimitive(node))
				value = CreateLanguagePrimitive(type, node);
			else if (node.Properties.ContainsKey(XmlName.xArguments) || node.Properties.ContainsKey(XmlName.xFactoryMethod))
				value = CreateFromFactory(type, node);
			else if (
				type.GetTypeInfo()
					.DeclaredConstructors.Any(
						ci =>
							ci.IsPublic && ci.GetParameters().Length != 0 &&
							ci.GetParameters().All(pi => pi.CustomAttributes.Any(attr => attr.AttributeType == typeof (ParameterAttribute)))) &&
				ValidateCtorArguments(type, node, out ctorargname))
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
					if (type == typeof (DataTemplate))
						value = new DataTemplate();
					if (type == typeof (ControlTemplate))
						value = new ControlTemplate();
					if (value == null && node.CollectionItems.Any() && node.CollectionItems.First() is ValueNode)
					{
						var serviceProvider = new XamlServiceProvider(node, Context);
						var converted = ((ValueNode)node.CollectionItems.First()).Value.ConvertTo(type, () => type.GetTypeInfo(),
							serviceProvider);
						if (converted != null && converted.GetType() == type)
							value = converted;
					}
					if (value == null)
						value = Activator.CreateInstance(type);
				}
				catch (TargetInvocationException e)
				{
					if (e.InnerException is XamlParseException || e.InnerException is XmlException)
						throw e.InnerException;
					throw;
				}
			}

			Values[node] = value;

			var markup = value as IMarkupExtension;
			if (markup != null && (value is TypeExtension || value is StaticExtension || value is ArrayExtension))
			{
				var serviceProvider = new XamlServiceProvider(node, Context);

				var visitor = new ApplyPropertiesVisitor(Context);
				foreach (var cnode in node.Properties.Values.ToList())
					cnode.Accept(visitor, node);
				foreach (var cnode in node.CollectionItems)
					cnode.Accept(visitor, node);

				value = markup.ProvideValue(serviceProvider);

				INode xKey;
				if (!node.Properties.TryGetValue(XmlName.xKey, out xKey))
					xKey = null;
				
				node.Properties.Clear();
				node.CollectionItems.Clear();

				if (xKey != null)
					node.Properties.Add(XmlName.xKey, xKey);

				Values[node] = value;
			}

			if (value is BindableObject)
				NameScope.SetNameScope(value as BindableObject, node.Namescope);
		}

		public void Visit(RootNode node, INode parentNode)
		{
			var rnode = (XamlLoader.RuntimeRootNode)node;
			Values[node] = rnode.Root;
			Context.Types[node] = rnode.Root.GetType();
			var bindableRoot = rnode.Root as BindableObject;
			if (bindableRoot != null)
				NameScope.SetNameScope(bindableRoot, node.Namescope);
		}

		public void Visit(ListNode node, INode parentNode)
		{
			//this is a gross hack to keep ListNode alive. ListNode must go in favor of Properties
			XmlName name;
			if (ApplyPropertiesVisitor.TryGetPropertyName(node, parentNode, out name))
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
							ci.GetParameters().All(pi => pi.CustomAttributes.Any(attr => attr.AttributeType == typeof (ParameterAttribute))));
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
							ci.GetParameters().All(pi => pi.CustomAttributes.Any(attr => attr.AttributeType == typeof (ParameterAttribute))));
			object[] arguments = CreateArgumentsArray(node, ctorInfo);
			return ctorInfo.Invoke(arguments);
		}

		public object CreateFromFactory(Type nodeType, IElementNode node)
		{
			object[] arguments = CreateArgumentsArray(node);

			if (!node.Properties.ContainsKey(XmlName.xFactoryMethod))
			{
				//non-default ctor
				return Activator.CreateInstance(nodeType, arguments);
			}

			var factoryMethod = ((string)((ValueNode)node.Properties[XmlName.xFactoryMethod]).Value);
			Type[] types = arguments == null ? new Type[0] : arguments.Select(a => a.GetType()).ToArray();
			Func<MethodInfo, bool> isMatch = m => {
				if (m.Name != factoryMethod)
					return false;
				var p = m.GetParameters();
				if (p.Length != types.Length)
					return false;
				if (!m.IsStatic)
					return false;
				for (var i = 0; i < p.Length; i++) {
					if ((p [i].ParameterType.IsAssignableFrom(types [i])))
						continue;
					var op_impl = p [i].ParameterType.GetRuntimeMethod("op_Implicit", new [] { types [i]});
					if (op_impl == null)
						return false;
					arguments [i] = op_impl.Invoke(null, new [] { arguments [i]});
				}
				return true;
			};
			var mi = nodeType.GetRuntimeMethods().FirstOrDefault(isMatch);
			if (mi == null)
				throw new MissingMemberException($"No static method found for {nodeType.FullName}::{factoryMethod} ({string.Join(", ", types.Select(t => t.FullName))})");
			return mi.Invoke(null, arguments);
		}

		public object[] CreateArgumentsArray(IElementNode enode)
		{
			if (!enode.Properties.ContainsKey(XmlName.xArguments))
				return null;
			var node = enode.Properties[XmlName.xArguments];
			var elementNode = node as ElementNode;
			if (elementNode != null)
			{
				var array = new object[1];
				array[0] = Values[elementNode];
				return array;
			}

			var listnode = node as ListNode;
			if (listnode != null)
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
					parameter.CustomAttributes.First(attr => attr.AttributeType == typeof (ParameterAttribute))
						.ConstructorArguments.First()
						.Value as string;
				var name = new XmlName("", propname);
				INode node;
				if (!enode.Properties.TryGetValue(name, out node))
				{
					throw new XamlParseException(
						String.Format("The Property {0} is required to create a {1} object.", propname, ctorInfo.DeclaringType.FullName),
						enode as IXmlLineInfo);
				}
				if (!enode.SkipProperties.Contains(name))
					enode.SkipProperties.Add(name);
				var value = Context.Values[node];
				var serviceProvider = new XamlServiceProvider(enode, Context);
				var convertedValue = value.ConvertTo(parameter.ParameterType, () => parameter, serviceProvider);
				array[i] = convertedValue;
			}

			return array;
		}

		static bool IsXaml2009LanguagePrimitive(IElementNode node)
		{
			return node.NamespaceURI == XamlParser.X2009Uri;
		}

		static object CreateLanguagePrimitive(Type nodeType, IElementNode node)
		{
			object value = null;
			if (nodeType == typeof (string))
				value = String.Empty;
			else if (nodeType == typeof (Uri))
				value = null;
			else
				value = Activator.CreateInstance(nodeType);

			if (node.CollectionItems.Count == 1 && node.CollectionItems[0] is ValueNode &&
			    ((ValueNode)node.CollectionItems[0]).Value is string)
			{
				var valuestring = ((ValueNode)node.CollectionItems[0]).Value as string;

				if (nodeType == typeof(SByte)) {
					sbyte retval;
					if (sbyte.TryParse(valuestring, NumberStyles.Number, CultureInfo.InvariantCulture, out retval))
						return retval;
				}
				if (nodeType == typeof(Int16)) {
					short retval;
					if (short.TryParse(valuestring, NumberStyles.Number, CultureInfo.InvariantCulture, out retval))
						return retval;
				}
				if (nodeType == typeof(Int32)) {
					int retval;
					if (int.TryParse(valuestring, NumberStyles.Number, CultureInfo.InvariantCulture, out retval))
						return retval;
				}
				if (nodeType == typeof(Int64)) {
					long retval;
					if (long.TryParse(valuestring, NumberStyles.Number, CultureInfo.InvariantCulture, out retval))
						return retval;
				}
				if (nodeType == typeof(Byte)) {
					byte retval;
					if (byte.TryParse(valuestring, NumberStyles.Number, CultureInfo.InvariantCulture, out retval))
						return retval;
				}
				if (nodeType == typeof(UInt16)) {
					ushort retval;
					if (ushort.TryParse(valuestring, NumberStyles.Number, CultureInfo.InvariantCulture, out retval))
						return retval;
				}
				if (nodeType == typeof(UInt32)) {
					uint retval;
					if (uint.TryParse(valuestring, NumberStyles.Number, CultureInfo.InvariantCulture, out retval))
						return retval;
				}
				if (nodeType == typeof(UInt64)) {
					ulong retval;
					if (ulong.TryParse(valuestring, NumberStyles.Number, CultureInfo.InvariantCulture, out retval))
						return retval;
				}
				if (nodeType == typeof(Single)) {
					float retval;
					if (float.TryParse(valuestring, NumberStyles.Number, CultureInfo.InvariantCulture, out retval))
						return retval;
				}
				if (nodeType == typeof(Double)) {
					double retval;
					if (double.TryParse(valuestring, NumberStyles.Number, CultureInfo.InvariantCulture, out retval))
						return retval;
				}
				if (nodeType == typeof (Boolean))
				{
					bool outbool;
					if (bool.TryParse(valuestring, out outbool))
						return outbool;
				}
				if (nodeType == typeof(TimeSpan)) {
					TimeSpan retval;
					if (TimeSpan.TryParse(valuestring, CultureInfo.InvariantCulture, out retval))
						return retval;
				}
				if (nodeType == typeof (char))
				{
					char retval;
					if (char.TryParse(valuestring, out retval))
						return retval;
				}
				if (nodeType == typeof (string))
					return valuestring;
				if (nodeType == typeof (decimal))
				{
					decimal retval;
					if (decimal.TryParse(valuestring, NumberStyles.Number, CultureInfo.InvariantCulture, out retval))
						return retval;
				}

				else if (nodeType == typeof (Uri))
				{
					Uri retval;
					if (Uri.TryCreate(valuestring, UriKind.RelativeOrAbsolute, out retval))
						return retval;
				}
			}
			return value;
		}
	}
}