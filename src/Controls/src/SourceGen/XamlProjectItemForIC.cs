using System;
using System.Xml;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.Maui.Controls.SourceGen;

class XamlProjectItemForIC
{
	public XamlProjectItemForIC(ProjectItem projectItem, SourceText sourceText, string? xaml)
	{
		ProjectItem = projectItem;
		SourceText = sourceText;
		Xaml = xaml;
	}

	public XamlProjectItemForIC(ProjectItem projectItem, SourceText? sourceText, Exception exception)
	{
		ProjectItem = projectItem;
		SourceText = sourceText;
		Exception = exception;
	}

	public ProjectItem ProjectItem { get; }
	public SourceText? SourceText { get; }
	public string? Xaml { get; }
	public Exception? Exception { get; }
}