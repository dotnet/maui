// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Controls.Xaml.Diagnostics
{
	/// <include file="../../../../docs/Microsoft.Maui.Controls.Xaml.Diagnostics/VisualDiagnostics.xml" path="Type[@FullName='Microsoft.Maui.Controls.Xaml.Diagnostics.VisualDiagnostics']/Docs" />
	public static class VisualDiagnostics
	{
		static ConditionalWeakTable<object, XamlSourceInfo> sourceInfos = new ConditionalWeakTable<object, XamlSourceInfo>();

		/// <include file="../../../../docs/Microsoft.Maui.Controls.Xaml.Diagnostics/VisualDiagnostics.xml" path="//Member[@MemberName='RegisterSourceInfo']/Docs" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void RegisterSourceInfo(object target, Uri uri, int lineNumber, int linePosition)
		{
			if (target != null && DebuggerHelper.DebuggerIsAttached && !sourceInfos.TryGetValue(target, out _))
				sourceInfos.Add(target, new XamlSourceInfo(uri, lineNumber, linePosition));
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.Xaml.Diagnostics/VisualDiagnostics.xml" path="//Member[@MemberName='GetXamlSourceInfo']/Docs" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static XamlSourceInfo GetXamlSourceInfo(object obj) =>
			sourceInfos.TryGetValue(obj, out var sourceinfo) ? sourceinfo : null;

		/// <include file="../../../../docs/Microsoft.Maui.Controls.Xaml.Diagnostics/VisualDiagnostics.xml" path="//Member[@MemberName='OnChildAdded']/Docs" />
		public static void OnChildAdded(IVisualTreeElement parent, IVisualTreeElement child)
		{
			if (!DebuggerHelper.DebuggerIsAttached)
				return;

			if (child is null)
				return;

			var index = parent?.GetVisualChildren().IndexOf(child) ?? -1;

			OnChildAdded(parent, child, index);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.Xaml.Diagnostics/VisualDiagnostics.xml" path="//Member[@MemberName='OnChildAdded']/Docs" />
		public static void OnChildAdded(IVisualTreeElement parent, IVisualTreeElement child, int newLogicalIndex)
		{
			if (!DebuggerHelper.DebuggerIsAttached)
				return;

			if (child is null)
				return;

			OnVisualTreeChanged(new VisualTreeChangeEventArgs(parent, child, newLogicalIndex, VisualTreeChangeType.Add));
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.Xaml.Diagnostics/VisualDiagnostics.xml" path="//Member[@MemberName='OnChildRemoved']/Docs" />
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

	/// <include file="../../../../docs/Microsoft.Maui.Controls.Xaml.Diagnostics/VisualTreeChangeType.xml" path="Type[@FullName='Microsoft.Maui.Controls.Xaml.Diagnostics.VisualTreeChangeType']/Docs" />
	public enum VisualTreeChangeType
	{
		/// <include file="../../../../docs/Microsoft.Maui.Controls.Xaml.Diagnostics/VisualTreeChangeType.xml" path="//Member[@MemberName='Add']/Docs" />
		Add = 0,
		/// <include file="../../../../docs/Microsoft.Maui.Controls.Xaml.Diagnostics/VisualTreeChangeType.xml" path="//Member[@MemberName='Remove']/Docs" />
		Remove = 1
	}
}