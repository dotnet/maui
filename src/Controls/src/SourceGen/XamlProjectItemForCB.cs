using System;
using System.Xml;

namespace Microsoft.Maui.Controls.SourceGen;

class XamlProjectItemForCB
{
	public XamlProjectItemForCB(ProjectItem projectItem, XmlNode root, XmlNamespaceManager nsmgr)
	{
		ProjectItem = projectItem;
		Root = root;
		Nsmgr = nsmgr;
	}

	public XamlProjectItemForCB(ProjectItem projectItem, Exception exception)
	{
		ProjectItem = projectItem;
		Exception = exception;
	}

	public ProjectItem? ProjectItem { get; }
	public XmlNode? Root { get; }
	public XmlNamespaceManager? Nsmgr { get; }
	public Exception? Exception { get; }
}
