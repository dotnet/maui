using System.ComponentModel;
using Microsoft.Maui;

namespace Microsoft.Maui
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal interface ISafeAreaElement
	{
		//note to implementor: implement this property publicly
		SafeAreaEdges SafeAreaEdges { get; }

		SafeAreaEdges SafeAreaEdgesDefaultValueCreator();
	}
}