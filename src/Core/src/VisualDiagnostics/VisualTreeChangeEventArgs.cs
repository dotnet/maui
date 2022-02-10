// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Maui
{
	/// <include file="../../docs/Microsoft.Maui/VisualTreeChangeEventArgs.xml" path="Type[@FullName='Microsoft.Maui.VisualTreeChangeEventArgs']/Docs" />
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
		/// <include file="../../docs/Microsoft.Maui/VisualTreeChangeEventArgs.xml" path="//Member[@MemberName='ChildIndex']/Docs" />
		public int ChildIndex { get; }
		/// <include file="../../docs/Microsoft.Maui/VisualTreeChangeEventArgs.xml" path="//Member[@MemberName='ChangeType']/Docs" />
		public VisualTreeChangeType ChangeType { get; }
	}
}
