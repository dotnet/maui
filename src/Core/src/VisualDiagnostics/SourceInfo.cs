// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Maui
{
	/// <summary>
	/// The source info for a given object.
	/// Used for locating where a given object is created
	/// in a given project.
	/// </summary>
	public class SourceInfo
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SourceInfo"/> class.
		/// </summary>
		/// <param name="sourceUri">The location of the source file where the object was created.</param>
		/// <param name="lineNumber">The line number of the object.</param>
		/// <param name="linePosition">The line position of the object.</param>
		public SourceInfo(Uri sourceUri, int lineNumber, int linePosition)
		{
			SourceUri = sourceUri;
			LineNumber = lineNumber;
			LinePosition = linePosition;
		}

		public Uri SourceUri { get; }
		public int LineNumber { get; }
		public int LinePosition { get; }

		/// <summary>
		/// Deconstructs a given <see cref="SourceInfo"/> back to its URI and line numbers.
		/// </summary>
		/// <param name="sourceUri">The location of the source file where the object was created.</param>
		/// <param name="lineNumber">The line number of the object.</param>
		/// <param name="linePosition">The line position of the object.</param>
		public void Deconstruct(out Uri sourceUri, out int lineNumber, out int linePosition)
		{
			sourceUri = SourceUri;
			lineNumber = LineNumber;
			linePosition = LinePosition;
		}
	}
}
