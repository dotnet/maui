using System;
using System.Xml;

namespace Microsoft.Maui.Controls.SourceGen;

class XamlProjectItemForIC
{
	public XamlProjectItemForIC(ProjectItem projectItem, string? xaml)
	{
		ProjectItem = projectItem;
		Xaml = xaml;
	}

	public XamlProjectItemForIC(ProjectItem projectItem, Exception exception)
	{
		ProjectItem = projectItem;
		Exception = exception;
	}

	public ProjectItem ProjectItem { get; }
	public string? Xaml { get; }
	public Exception? Exception { get; }
}