using Mono.Cecil;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Build.Tasks
{
	class ILRootNode : RootNode
	{
		public ILRootNode(XmlType xmlType, TypeReference typeReference) : base(xmlType, null)
		{
			TypeReference = typeReference;
		}

		public TypeReference TypeReference { get; private set; }
	}
}