using System;

#nullable disable

namespace Microsoft.Maui.Controls.Xaml
{
	class SimplifyOnPlatformVisitor : IXamlNodeVisitor
	{
		public SimplifyOnPlatformVisitor(string targetFramework)
		{
			TargetFramework = targetFramework;
		}

		public string TargetFramework { get; }

		public TreeVisitingMode VisitingMode => TreeVisitingMode.BottomUp;
		public bool StopOnDataTemplate => false;
		public bool VisitNodeOnDataTemplate => true;
		public bool StopOnResourceDictionary => false;
		public bool IsResourceDictionary(ElementNode node) => false;
		public bool SkipChildren(INode node, INode parentNode) => false;

		public void Visit(ValueNode node, INode parentNode)
		{
		}

		public void Visit(MarkupNode node, INode parentNode)
		{
		}

		public void Visit(ElementNode node, INode parentNode)
		{
			if (node.XmlType.Name != nameof(OnPlatformExtension) || node.XmlType.NamespaceUri != XamlParser.MauiUri)
				return;
			if (string.IsNullOrEmpty(TargetFramework))
				return;

			string target = null;
#if NETSTANDARD2_0
			if (TargetFramework.Contains("-android"))
#else
			if (TargetFramework.Contains("-android", StringComparison.Ordinal))
#endif
				target = nameof(OnPlatformExtension.Android);
#if NETSTANDARD2_0
			if (TargetFramework.Contains("-ios"))
#else
			if (TargetFramework.Contains("-ios", StringComparison.Ordinal))
#endif
				target = nameof(OnPlatformExtension.iOS);
#if NETSTANDARD2_0
			if (TargetFramework.Contains("-macos"))
#else
			if (TargetFramework.Contains("-macos", StringComparison.Ordinal))
#endif
				target = nameof(OnPlatformExtension.macOS);
#if NETSTANDARD2_0
			if (TargetFramework.Contains("-maccatalyst"))
#else
			if (TargetFramework.Contains("-maccatalyst", StringComparison.Ordinal))
#endif
				target = nameof(OnPlatformExtension.MacCatalyst);

			if (target is null)
				return;

			if (   node.Properties.TryGetValue(new XmlName("", target), out INode targetNode)
				|| node.Properties.TryGetValue(new XmlName("", nameof(OnPlatformExtension.Default)), out targetNode))
			{
				if (!ApplyPropertiesVisitor.TryGetPropertyName(node, parentNode, out XmlName name))
					return;
				if (parentNode is IElementNode parentEnode)
					parentEnode.Properties[name] = targetNode;
			}
			else //no prop for target and no Default set
			{
				if (!ApplyPropertiesVisitor.TryGetPropertyName(node, parentNode, out XmlName name))
					return;
				//if there's no value for the targetPlatform, ignore the node.
				//this is slightly different than what OnPlatform does (return default(T))
				if (parentNode is IElementNode parentEnode)
					parentEnode.Properties.Remove(name);
			}

		}

		public void Visit(RootNode node, INode parentNode)
		{
		}

		public void Visit(ListNode node, INode parentNode)
		{
		}
	}
}
