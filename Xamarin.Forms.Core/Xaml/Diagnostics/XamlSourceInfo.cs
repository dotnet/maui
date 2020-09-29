// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Xamarin.Forms.Xaml.Diagnostics
{
	public class XamlSourceInfo
	{
		public XamlSourceInfo(Uri sourceUri, int lineNumber, int linePosition)
		{
			SourceUri = sourceUri;
			LineNumber = lineNumber;
			LinePosition = linePosition;
		}

		public Uri SourceUri { get; }
		public int LineNumber { get; }
		public int LinePosition { get; }

		public void Deconstruct(out Uri sourceUri, out int lineNumber, out int linePosition)
		{
			sourceUri = SourceUri;
			lineNumber = LineNumber;
			linePosition = LinePosition;
		}
	}
}