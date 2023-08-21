// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.Maui
{
	public class VisualTreeChangeEventArgs : EventArgs
	{
		public VisualTreeChangeEventArgs(object? parent, object child, int childIndex, VisualTreeChangeType changeType)
		{
			Parent = parent;
			Child = child;
			ChildIndex = childIndex;
			ChangeType = changeType;
		}

		public object? Parent { get; }
		public object Child { get; }
		/// <include file="../../docs/Microsoft.Maui/VisualTreeChangeEventArgs.xml" path="//Member[@MemberName='ChildIndex']/Docs/*" />
		public int ChildIndex { get; }
		/// <include file="../../docs/Microsoft.Maui/VisualTreeChangeEventArgs.xml" path="//Member[@MemberName='ChangeType']/Docs/*" />
		public VisualTreeChangeType ChangeType { get; }
	}
}
