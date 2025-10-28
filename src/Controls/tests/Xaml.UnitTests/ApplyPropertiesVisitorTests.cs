using System.Collections;
using Microsoft.Maui.Controls.Xaml;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{	public static class ApplyPropertiesVisitorTests
	{
		public class MarkupExtension : IMarkupExtension
		{
			public object ProvideValue(System.IServiceProvider serviceProvider)
			{
				return "provided value";
			}
		}

		public class ArrayListOwner
		{
			public ArrayList ArrayList { get; } = new ArrayList();
		}

		[Fact]
		public static void ProvideValueForCollectionItem()
		{
			const string NAMESPACE = "clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests;assembly=Microsoft.Maui.Controls.Xaml.UnitTests";
			var resolver = new MockNameSpaceResolver();
			var type = new XmlType(NAMESPACE, "ApplyPropertiesVisitorTests+MarkupExtension", null);
			var listNode = new ListNode(new[]
			{
				new ElementNode(type, NAMESPACE, resolver),
				new ElementNode(type, NAMESPACE, resolver)
			}, resolver);
			var rootElement = new ArrayListOwner();
			var rootType = new XmlType(NAMESPACE, "ApplyPropertiesVisitorTests+ArrayListOwner", null);
			var rootNode = new XamlLoader.RuntimeRootNode(rootType, rootElement, null);
			var context = new HydrationContext { RootElement = rootElement };

			rootNode.Properties.Add(new XmlName(null, "ArrayList"), listNode);
			rootNode.Accept(new XamlNodeVisitor((node, parent) => node.Parent = parent), null);
			rootNode.Accept(new CreateValuesVisitor(context), null);
			rootNode.Accept(new ApplyPropertiesVisitor(context), null);

			CollectionAssert.Equal(new[] { "provided value", "provided value" }, rootElement.ArrayList);
		}
	}
}
