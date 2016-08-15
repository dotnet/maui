using System.Xml;
using Mono.Cecil;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Build.Tasks
{
	class ILRootNode : RootNode
	{
		public ILRootNode(XmlType xmlType, TypeReference typeReference, IXmlNamespaceResolver nsResolver) : base(xmlType, nsResolver)
		{
			TypeReference = typeReference;
		}

		public TypeReference TypeReference { get; private set; }
	}
}