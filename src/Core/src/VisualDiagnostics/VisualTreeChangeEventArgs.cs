// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Maui
{
	/// <summary>
	/// Provides data for changes in the visual tree, such as when a child is added or removed.
	/// </summary>
	public class VisualTreeChangeEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of <see cref="VisualTreeChangeEventArgs"/>.
		/// </summary>
		/// <param name="parent">The parent visual element, or null for the root.</param>
		/// <param name="child">The child visual element involved in the change.</param>
		/// <param name="childIndex">The logical index of the child within its parent at the time of change.</param>
		/// <param name="changeType">The type of change that occurred.</param>
		public VisualTreeChangeEventArgs(object? parent, object child, int childIndex, VisualTreeChangeType changeType)
		{
			Parent = parent;
			Child = child;
			ChildIndex = childIndex;
			ChangeType = changeType;
		}

		/// <summary>
		/// Gets the parent visual element involved in the change, or null if this is the root.
		/// </summary>
		public object? Parent { get; }

		/// <summary>
		/// Gets the child visual element involved in the change.
		/// </summary>
		public object Child { get; }

		/// <summary>
		/// Gets the logical index of the child within its parent at the time of change.
		/// </summary>
		public int ChildIndex { get; }

		/// <summary>
		/// Gets the type of visual tree change that occurred (Add or Remove).
		/// </summary>
		public VisualTreeChangeType ChangeType { get; }
	}
}
