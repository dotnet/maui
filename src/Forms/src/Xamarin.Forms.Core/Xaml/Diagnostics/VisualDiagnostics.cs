// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Xaml.Diagnostics
{
	public class VisualDiagnostics
	{
		static ConditionalWeakTable<object, XamlSourceInfo> sourceInfos = new ConditionalWeakTable<object, XamlSourceInfo>();

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void RegisterSourceInfo(object target, Uri uri, int lineNumber, int linePosition)
		{
			if (target != null && DebuggerHelper.DebuggerIsAttached && !sourceInfos.TryGetValue(target, out _))
				sourceInfos.Add(target, new XamlSourceInfo(uri, lineNumber, linePosition));
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("use OnChildAdded/Removed")]
		internal static void SendVisualTreeChanged(object parent, object child)
		{
			if (!DebuggerHelper.DebuggerIsAttached)
				return;

			VisualTreeChanged?.Invoke(parent, new VisualTreeChangeEventArgs(parent, child, -1, child != null ? VisualTreeChangeType.Add : VisualTreeChangeType.Remove));
		}

		internal static void OnChildAdded(Element parent, Element child)
		{
			if (!DebuggerHelper.DebuggerIsAttached)
				return;

			if (child is null)
				return;

			var index = parent?.AllChildren.IndexOf(child) ?? -1;
			VisualTreeChanged?.Invoke(parent, new VisualTreeChangeEventArgs(parent, child, index, VisualTreeChangeType.Add));
		}

		internal static void OnChildRemoved(Element parent, Element child, int oldLogicalIndex)
		{
			if (!DebuggerHelper.DebuggerIsAttached)
				return;

			VisualTreeChanged?.Invoke(parent, new VisualTreeChangeEventArgs(parent, child, oldLogicalIndex, VisualTreeChangeType.Remove));
		}

		public static event EventHandler<VisualTreeChangeEventArgs> VisualTreeChanged;
		public static XamlSourceInfo GetXamlSourceInfo(object obj) => sourceInfos.TryGetValue(obj, out var sourceinfo) ? sourceinfo : null;
	}

	public enum VisualTreeChangeType
	{
		Add = 0,
		Remove = 1
	}
}