using System.Xml;
using Microsoft.Maui.Controls.Xaml;


namespace Microsoft.Maui.Controls.SourceGen;

class SGRootNode : RootNode
	{
		public SGRootNode(XmlType xmlType, /*TypeReference typeReference,*/ IXmlNamespaceResolver nsResolver, int linenumber = -1, int lineposition = -1) : base(xmlType, nsResolver, linenumber: linenumber, lineposition: lineposition)
		{
			// TypeReference = typeReference;
		}

		// public TypeReference TypeReference { get; private set; }
	}