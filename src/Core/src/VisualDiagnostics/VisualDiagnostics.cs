// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui
{
	/// <include file="../../docs/Microsoft.Maui/VisualDiagnostics.xml" path="Type[@FullName='Microsoft.Maui.VisualDiagnostics']/Docs" />
	public static class VisualDiagnostics
	{
		static ConditionalWeakTable<object, SourceInfo> sourceInfos = new ConditionalWeakTable<object, SourceInfo>();

		/// <include file="../../docs/Microsoft.Maui/VisualDiagnostics.xml" path="//Member[@MemberName='RegisterSourceInfo']/Docs" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void RegisterSourceInfo(object target, Uri uri, int lineNumber, int linePosition)
		{
			if (target != null && DebuggerHelper.DebuggerIsAttached && !sourceInfos.TryGetValue(target, out _))
				sourceInfos.Add(target, new SourceInfo(uri, lineNumber, linePosition));
		}

		/// <include file="../../docs/Microsoft.Maui/VisualDiagnostics.xml" path="//Member[@MemberName='GetXamlSourceInfo']/Docs" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static SourceInfo? GetSourceInfo(object obj) =>
			sourceInfos.TryGetValue(obj, out var sourceinfo) ? sourceinfo : null;

		/// <include file="../../docs/Microsoft.Maui/VisualDiagnostics.xml" path="//Member[@MemberName='OnChildAdded'][1]/Docs" />
		public static void OnChildAdded(IVisualTreeElement parent, IVisualTreeElement child)
		{
			if (!DebuggerHelper.DebuggerIsAttached)
				return;

			if (child is null)
				return;

			var index = parent?.GetVisualChildren().IndexOf(child) ?? -1;

			OnChildAdded(parent, child, index);
		}

		/// <include file="../../docs/Microsoft.Maui/VisualDiagnostics.xml" path="//Member[@MemberName='OnChildAdded'][2]/Docs" />
		public static void OnChildAdded(IVisualTreeElement? parent, IVisualTreeElement child, int newLogicalIndex)
		{
			if (!DebuggerHelper.DebuggerIsAttached)
				return;

			if (child is null)
				return;

			OnVisualTreeChanged(new VisualTreeChangeEventArgs(parent, child, newLogicalIndex, VisualTreeChangeType.Add));
		}

		/// <include file="../../docs/Microsoft.Maui/VisualDiagnostics.xml" path="//Member[@MemberName='OnChildRemoved']/Docs" />
		public static void OnChildRemoved(IVisualTreeElement parent, IVisualTreeElement child, int oldLogicalIndex)
		{
			if (!DebuggerHelper.DebuggerIsAttached)
				return;

			OnVisualTreeChanged(new VisualTreeChangeEventArgs(parent, child, oldLogicalIndex, VisualTreeChangeType.Remove));
		}

		public static event EventHandler<VisualTreeChangeEventArgs>? VisualTreeChanged;

		static void OnVisualTreeChanged(VisualTreeChangeEventArgs e)
		{
			VisualTreeChanged?.Invoke(e.Parent, e);
		}
	}
}
