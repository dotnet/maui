using System;
using System.Linq;

#nullable disable

namespace Microsoft.Maui.Controls.Xaml
{
	class SimplifyTypeExtensionVisitor : IXamlNodeVisitor
	{
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
			//markup was already expanded to element
		}

		public void Visit(ElementNode node, INode parentNode)
		{
			// Only simplify property setter of TypeExtension and x:Type
			// TargetType="{x:Type typeNameValue}" -> TargetType="typeNameValue"
			// x:DataType="{x:Type typeNameValue}" -> x:DataType="typeNameValue"

			if (IsValueOfXDataTypeOrTargetType(node, parentNode, out XmlName propertyName)
				&& IsTypeExtension(node, out ValueNode typeNameValueNode))
			{
				(parentNode as IElementNode).Properties[propertyName] = typeNameValueNode;
			}

			static bool IsValueOfXDataTypeOrTargetType(ElementNode node, INode parentNode, out XmlName propertyName)
				=> node.TryGetPropertyName(parentNode, out propertyName)
					&& (IsXDataType(propertyName) || IsTargetTypePropertyOfMauiType(parentNode, propertyName));

			static bool IsTargetTypePropertyOfMauiType(INode parentNode, XmlName propertyName)
				=> propertyName == new XmlName("", "TargetType")
					&& parentNode is ElementNode { XmlType: var parentType }
					&& (IsStyle(parentType)
						|| IsTrigger(parentType)
						|| IsDataTrigger(parentType)
						|| IsMultiTrigger(parentType));

			static bool IsXDataType(XmlName name) => name == XmlName.xDataType;
			static bool IsStyle(XmlType type) => type.Name == nameof(Style) && type.NamespaceUri == XamlParser.MauiUri;
			static bool IsTrigger(XmlType type) => type.Name == nameof(Trigger) && type.NamespaceUri == XamlParser.MauiUri;
			static bool IsDataTrigger(XmlType type) => type.Name == nameof(DataTrigger) && type.NamespaceUri == XamlParser.MauiUri;
			static bool IsMultiTrigger(XmlType type) => type.Name == nameof(MultiTrigger) && type.NamespaceUri == XamlParser.MauiUri;

			static bool IsTypeExtension(ElementNode node, out ValueNode typeNameValueNode)
			{
				if (node.XmlType.Name == nameof(TypeExtension) && node.XmlType.NamespaceUri == XamlParser.X2009Uri)
				{
					XmlName typeNameXmlName = new("", "TypeName");
					if (node.Properties.TryGetValue(typeNameXmlName, out INode value)
						&& value is ValueNode { Value: string } propertyValueNode)
					{
						typeNameValueNode = propertyValueNode;
						return true;
					}

					if (node.CollectionItems.Count == 1
						&& node.CollectionItems[0] is ValueNode { Value: string } collectionValueNode)
					{
						typeNameValueNode = collectionValueNode;
						return true;
					}
				}

				typeNameValueNode = null;
				return false;
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
