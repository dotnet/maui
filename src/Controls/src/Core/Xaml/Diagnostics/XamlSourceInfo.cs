// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Maui.Controls.Xaml.Diagnostics
{
	/// <include file="../../../../docs/Microsoft.Maui.Controls.Xaml.Diagnostics/XamlSourceInfo.xml" path="Type[@FullName='Microsoft.Maui.Controls.Xaml.Diagnostics.XamlSourceInfo']/Docs" />
	public class XamlSourceInfo
	{
		/// <include file="../../../../docs/Microsoft.Maui.Controls.Xaml.Diagnostics/XamlSourceInfo.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public XamlSourceInfo(Uri sourceUri, int lineNumber, int linePosition)
		{
			SourceUri = sourceUri;
			LineNumber = lineNumber;
			LinePosition = linePosition;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.Xaml.Diagnostics/XamlSourceInfo.xml" path="//Member[@MemberName='SourceUri']/Docs" />
		public Uri SourceUri { get; }
		/// <include file="../../../../docs/Microsoft.Maui.Controls.Xaml.Diagnostics/XamlSourceInfo.xml" path="//Member[@MemberName='LineNumber']/Docs" />
		public int LineNumber { get; }
		/// <include file="../../../../docs/Microsoft.Maui.Controls.Xaml.Diagnostics/XamlSourceInfo.xml" path="//Member[@MemberName='LinePosition']/Docs" />
		public int LinePosition { get; }

		/// <include file="../../../../docs/Microsoft.Maui.Controls.Xaml.Diagnostics/XamlSourceInfo.xml" path="//Member[@MemberName='Deconstruct']/Docs" />
		public void Deconstruct(out Uri sourceUri, out int lineNumber, out int linePosition)
		{
			sourceUri = SourceUri;
			lineNumber = LineNumber;
			linePosition = LinePosition;
		}
	}
}
