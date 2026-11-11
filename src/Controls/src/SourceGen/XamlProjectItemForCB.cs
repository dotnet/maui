using System;
using System.Xml;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.Maui.Controls.SourceGen;

class XamlProjectItemForCB
{
	public XamlProjectItemForCB(ProjectItem projectItem, SourceText sourceText, XmlNode root, XmlNamespaceManager nsmgr)
	{
		ProjectItem = projectItem;
		SourceText = sourceText;
		Root = root;
		Nsmgr = nsmgr;
	}

	public XamlProjectItemForCB(ProjectItem projectItem, SourceText? sourceText, Exception exception)
	{
		ProjectItem = projectItem;
		SourceText = sourceText;
		Exception = exception;
	}

	public ProjectItem? ProjectItem { get; }
	public SourceText? SourceText { get; }
	public XmlNode? Root { get; }
	public XmlNamespaceManager? Nsmgr { get; }
	public Exception? Exception { get; }
}
