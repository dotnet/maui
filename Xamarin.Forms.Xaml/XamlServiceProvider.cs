using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Xaml.Internals
{
	public class XamlServiceProvider : IServiceProvider
	{
		readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

		internal XamlServiceProvider(INode node, HydrationContext context)
		{
			object targetObject;
			if (node != null && node.Parent != null && context.Values.TryGetValue(node.Parent, out targetObject))
				IProvideValueTarget = new XamlValueTargetProvider(targetObject, node, context, null);
			if (context != null)
				IRootObjectProvider = new XamlRootObjectProvider(context.RootElement);
			if (node != null)
			{
				IXamlTypeResolver = new XamlTypeResolver(node.NamespaceResolver, XamlParser.GetElementType,
					context.RootElement.GetType().GetTypeInfo().Assembly);

				var enode = node;
				while (enode != null && !(enode is IElementNode))
					enode = enode.Parent;
				if (enode != null)
					INameScopeProvider = new NameScopeProvider { NameScope = (enode as IElementNode).Namescope };
			}

			var xmlLineInfo = node as IXmlLineInfo;
			if (xmlLineInfo != null)
				IXmlLineInfoProvider = new XmlLineInfoProvider(xmlLineInfo);

			IValueConverterProvider = new ValueConverterProvider();
		}

		public XamlServiceProvider()
		{
			IValueConverterProvider = new ValueConverterProvider();
		}

		internal IProvideValueTarget IProvideValueTarget
		{
			get { return (IProvideValueTarget)GetService(typeof (IProvideValueTarget)); }
			set { services[typeof (IProvideValueTarget)] = value; }
		}

		internal IXamlTypeResolver IXamlTypeResolver
		{
			get { return (IXamlTypeResolver)GetService(typeof (IXamlTypeResolver)); }
			set { services[typeof (IXamlTypeResolver)] = value; }
		}

		internal IRootObjectProvider IRootObjectProvider
		{
			get { return (IRootObjectProvider)GetService(typeof (IRootObjectProvider)); }
			set { services[typeof (IRootObjectProvider)] = value; }
		}

		internal IXmlLineInfoProvider IXmlLineInfoProvider
		{
			get { return (IXmlLineInfoProvider)GetService(typeof (IXmlLineInfoProvider)); }
			set { services[typeof (IXmlLineInfoProvider)] = value; }
		}

		internal INameScopeProvider INameScopeProvider
		{
			get { return (INameScopeProvider)GetService(typeof (INameScopeProvider)); }
			set { services[typeof (INameScopeProvider)] = value; }
		}

		internal IValueConverterProvider IValueConverterProvider
		{
			get { return (IValueConverterProvider)GetService(typeof (IValueConverterProvider)); }
			set { services[typeof (IValueConverterProvider)] = value; }
		}

		public object GetService(Type serviceType)
		{
			object service;
			return services.TryGetValue(serviceType, out service) ? service : null;
		}

		public void Add(Type type, object service)
		{
			services.Add(type, service);
		}
	}

	class XamlValueTargetProvider : IProvideParentValues, IProvideValueTarget
	{
		public XamlValueTargetProvider(object targetObject, INode node, HydrationContext context, object targetProperty)
		{
			Context = context;
			Node = node;
			TargetObject = targetObject;
			TargetProperty = targetProperty;
		}

		INode Node { get; }

		HydrationContext Context { get; }
		public object TargetObject { get; }
		public object TargetProperty { get; internal set; } = null;

		IEnumerable<object> IProvideParentValues.ParentObjects
		{
			get
			{
				if (Node == null || Context == null)
					yield break;
				var n = Node;
				object obj = null;
				var context = Context;
				while (n.Parent != null && context != null)
				{
					if (n.Parent is IElementNode)
					{
						if (context.Values.TryGetValue(n.Parent, out obj))
							yield return obj;
						else
						{
							context = context.ParentContext;
							continue;
						}
					}
					n = n.Parent;
				}
			}
		}
	}

	public class SimpleValueTargetProvider : IProvideParentValues, IProvideValueTarget
	{
		readonly object[] objectAndParents;
		readonly object targetProperty;

		[Obsolete("SimpleValueTargetProvider(object[] objectAndParents) is obsolete as of version 2.3.4. Please use SimpleValueTargetProvider(object[] objectAndParents, object targetProperty) instead.")]
		public SimpleValueTargetProvider(object[] objectAndParents) : this (objectAndParents, null)
		{
		}

		public SimpleValueTargetProvider(object[] objectAndParents, object targetProperty)
		{
			if (objectAndParents == null)
				throw new ArgumentNullException(nameof(objectAndParents));
			if (objectAndParents.Length == 0)
				throw new ArgumentException();

			this.objectAndParents = objectAndParents;
			this.targetProperty = targetProperty;
		}

		IEnumerable<object> IProvideParentValues.ParentObjects
		{
			get { return objectAndParents; }
		}

		object IProvideValueTarget.TargetObject
		{
			get { return objectAndParents[0]; }
		}

		object IProvideValueTarget.TargetProperty
		{
			get { return targetProperty; }
		}
	}

	public class XamlTypeResolver : IXamlTypeResolver
	{
		readonly Assembly currentAssembly;
		readonly GetTypeFromXmlName getTypeFromXmlName;
		readonly IXmlNamespaceResolver namespaceResolver;

		public XamlTypeResolver(IXmlNamespaceResolver namespaceResolver, Assembly currentAssembly)
			: this(namespaceResolver, XamlParser.GetElementType, currentAssembly)
		{
		}

		internal XamlTypeResolver(IXmlNamespaceResolver namespaceResolver, GetTypeFromXmlName getTypeFromXmlName,
			Assembly currentAssembly)
		{
			this.currentAssembly = currentAssembly;
			if (namespaceResolver == null)
				throw new ArgumentNullException();
			if (getTypeFromXmlName == null)
				throw new ArgumentNullException();

			this.namespaceResolver = namespaceResolver;
			this.getTypeFromXmlName = getTypeFromXmlName;
		}

		Type IXamlTypeResolver.Resolve(string qualifiedTypeName, IServiceProvider serviceProvider)
		{
			XamlParseException e;
			var type = Resolve(qualifiedTypeName, serviceProvider, out e);
			if (e != null)
				throw e;
			return type;
		}

		bool IXamlTypeResolver.TryResolve(string qualifiedTypeName, out Type type)
		{
			XamlParseException exception;
			type = Resolve(qualifiedTypeName, null, out exception);
			return exception == null;
		}

		Type Resolve(string qualifiedTypeName, IServiceProvider serviceProvider, out XamlParseException exception)
		{
			exception = null;
			var split = qualifiedTypeName.Split(':');
			if (split.Length > 2)
				return null;

			string prefix, name;
			if (split.Length == 2)
			{
				prefix = split[0];
				name = split[1];
			}
			else
			{
				prefix = "";
				name = split[0];
			}

			IXmlLineInfo xmlLineInfo = null;
			if (serviceProvider != null)
			{
				var lineInfoProvider = serviceProvider.GetService(typeof (IXmlLineInfoProvider)) as IXmlLineInfoProvider;
				if (lineInfoProvider != null)
					xmlLineInfo = lineInfoProvider.XmlLineInfo;
			}

			var namespaceuri = namespaceResolver.LookupNamespace(prefix);
			if (namespaceuri == null)
			{
				exception = new XamlParseException(string.Format("No xmlns declaration for prefix \"{0}\"", prefix), xmlLineInfo);
				return null;
			}

			return getTypeFromXmlName(new XmlType(namespaceuri, name, null), xmlLineInfo, currentAssembly, out exception);
		}

		internal delegate Type GetTypeFromXmlName(
			XmlType xmlType, IXmlLineInfo xmlInfo, Assembly currentAssembly, out XamlParseException exception);
	}

	class XamlRootObjectProvider : IRootObjectProvider
	{
		public XamlRootObjectProvider(object rootObject)
		{
			RootObject = rootObject;
		}

		public object RootObject { get; }
	}

	public class XmlLineInfoProvider : IXmlLineInfoProvider
	{
		public XmlLineInfoProvider(IXmlLineInfo xmlLineInfo)
		{
			XmlLineInfo = xmlLineInfo;
		}

		public IXmlLineInfo XmlLineInfo { get; }
	}

	interface INameScopeProvider
	{
		INameScope NameScope { get; }
	}

	public class NameScopeProvider : INameScopeProvider
	{
		public INameScope NameScope { get; set; }
	}

	public class XmlNamespaceResolver : IXmlNamespaceResolver
	{
		readonly Dictionary<string, string> namespaces = new Dictionary<string, string>();

		public IDictionary<string, string> GetNamespacesInScope(XmlNamespaceScope scope)
		{
			throw new NotImplementedException();
		}

		public string LookupNamespace(string prefix)
		{
			string result;
			if (namespaces.TryGetValue(prefix, out result))
				return result;
			return null;
		}

		public string LookupPrefix(string namespaceName)
		{
			throw new NotImplementedException();
		}

		public void Add(string prefix, string ns)
		{
			namespaces.Add(prefix, ns);
		}
	}
}