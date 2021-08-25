// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Controls.Xaml.Diagnostics
{
	public static class VisualDiagnostics
	{
		static ConditionalWeakTable<object, XamlSourceInfo> sourceInfos = new ConditionalWeakTable<object, XamlSourceInfo>();

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void RegisterSourceInfo(object target, Uri uri, int lineNumber, int linePosition)
		{
			if (target != null && DebuggerHelper.DebuggerIsAttached && !sourceInfos.TryGetValue(target, out _))
				sourceInfos.Add(target, new XamlSourceInfo(uri, lineNumber, linePosition));
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static XamlSourceInfo GetXamlSourceInfo(object obj) =>
			sourceInfos.TryGetValue(obj, out var sourceinfo) ? sourceinfo : null;

		public static void OnChildAdded(IVisualTreeElement parent, IVisualTreeElement child)
		{
			if (!DebuggerHelper.DebuggerIsAttached)
				return;

			if (child is null)
				return;

			var index = parent?.GetVisualChildren().IndexOf(child) ?? -1;

			OnChildAdded(parent, child, index);
		}

		public static void OnChildAdded(IVisualTreeElement parent, IVisualTreeElement child, int newLogicalIndex)
		{
			if (!DebuggerHelper.DebuggerIsAttached)
				return;

			if (child is null)
				return;

			OnVisualTreeChanged(new VisualTreeChangeEventArgs(parent, child, newLogicalIndex, VisualTreeChangeType.Add));
		}

		public static void OnChildRemoved(IVisualTreeElement parent, IVisualTreeElement child, int oldLogicalIndex)
		{
			if (!DebuggerHelper.DebuggerIsAttached)
				return;

			OnVisualTreeChanged(new VisualTreeChangeEventArgs(parent, child, oldLogicalIndex, VisualTreeChangeType.Remove));
		}

		public static event EventHandler<VisualTreeChangeEventArgs> VisualTreeChanged;

		static void OnVisualTreeChanged(VisualTreeChangeEventArgs e)
		{
			VisualTreeChanged?.Invoke(e.Parent, e);
		}
	}

	public enum VisualTreeChangeType
	{
		Add = 0,
		Remove = 1
	}
}