using System;
using System.Xml;

namespace Microsoft.Maui.Controls.SourceGen;

class XamlProjectItemForIC
{
	public XamlProjectItemForIC(ProjectItem projectItem, SGRootNode? root/*, XmlNamespaceManager nsmgr*/)
	{
		ProjectItem = projectItem;
		Root = root;
		// Nsmgr = nsmgr;
	}

	public XamlProjectItemForIC(ProjectItem projectItem, Exception exception)
	{
		ProjectItem = projectItem;
		Exception = exception;
	}

	public ProjectItem ProjectItem { get; }
	public SGRootNode? Root { get; }
	// public XmlNamespaceManager? Nsmgr { get; }
	public Exception? Exception { get; }
}