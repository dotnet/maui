// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Maui.Controls.Xaml.Diagnostics
{
	/// <include file="../../../../docs/Microsoft.Maui.Controls.Xaml.Diagnostics/VisualTreeChangeEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.Xaml.Diagnostics.VisualTreeChangeEventArgs']/Docs" />
	public class VisualTreeChangeEventArgs : EventArgs
	{
		/// <include file="../../../../docs/Microsoft.Maui.Controls.Xaml.Diagnostics/VisualTreeChangeEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public VisualTreeChangeEventArgs(object parent, object child, int childIndex, VisualTreeChangeType changeType)
		{
			Parent = parent;
			Child = child;
			ChildIndex = childIndex;
			ChangeType = changeType;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.Xaml.Diagnostics/VisualTreeChangeEventArgs.xml" path="//Member[@MemberName='Parent']/Docs" />
		public object Parent { get; }
		/// <include file="../../../../docs/Microsoft.Maui.Controls.Xaml.Diagnostics/VisualTreeChangeEventArgs.xml" path="//Member[@MemberName='Child']/Docs" />
		public object Child { get; }
		/// <include file="../../../../docs/Microsoft.Maui.Controls.Xaml.Diagnostics/VisualTreeChangeEventArgs.xml" path="//Member[@MemberName='ChildIndex']/Docs" />
		public int ChildIndex { get; }
		/// <include file="../../../../docs/Microsoft.Maui.Controls.Xaml.Diagnostics/VisualTreeChangeEventArgs.xml" path="//Member[@MemberName='ChangeType']/Docs" />
		public VisualTreeChangeType ChangeType { get; }
	}
}
