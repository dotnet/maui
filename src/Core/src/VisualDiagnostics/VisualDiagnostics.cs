// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.Media;

namespace Microsoft.Maui
{
	public static class VisualDiagnostics
	{
		static ConditionalWeakTable<object, SourceInfo> sourceInfos = new ConditionalWeakTable<object, SourceInfo>();

		/// <include file="../../docs/Microsoft.Maui/VisualDiagnostics.xml" path="//Member[@MemberName='RegisterSourceInfo']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void RegisterSourceInfo(object target, Uri uri, int lineNumber, int linePosition)
		{
#if !NETSTANDARD2_0
			if (target != null && DebuggerHelper.DebuggerIsAttached)
				sourceInfos.AddOrUpdate(target, new SourceInfo(uri, lineNumber, linePosition));
#else
			if (target != null && DebuggerHelper.DebuggerIsAttached)
			{
				if (sourceInfos.TryGetValue(target, out _))
					sourceInfos.Remove(target);
				sourceInfos.Add(target, new SourceInfo(uri, lineNumber, linePosition));
			}
#endif
		}

		/// <include file="../../docs/Microsoft.Maui/VisualDiagnostics.xml" path="//Member[@MemberName='GetXamlSourceInfo']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static SourceInfo? GetSourceInfo(object obj) =>
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

		public static void OnChildAdded(IVisualTreeElement? parent, IVisualTreeElement child, int newLogicalIndex)
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

		public static event EventHandler<VisualTreeChangeEventArgs>? VisualTreeChanged;

		static void OnVisualTreeChanged(VisualTreeChangeEventArgs e)
		{
			VisualTreeChanged?.Invoke(e.Parent, e);
		}

		public static async Task<byte[]?> CaptureAsPngAsync(IView view)
		{
			var result = await view.CaptureAsync();
			return await ScreenshotResultToArray(result, ScreenshotFormat.Png, 100);
		}

		public static async Task<byte[]?> CaptureAsJpegAsync(IView view, int quality = 80)
		{
			var result = await view.CaptureAsync();
			return await ScreenshotResultToArray(result, ScreenshotFormat.Jpeg, quality);
		}

		public static async Task<byte[]?> CaptureAsPngAsync(IWindow window)
		{
			var result = await window.CaptureAsync();
			return await ScreenshotResultToArray(result, ScreenshotFormat.Png, 100);
		}

		public static async Task<byte[]?> CaptureAsJpegAsync(IWindow window, int quality = 80)
		{
			var result = await window.CaptureAsync();
			return await ScreenshotResultToArray(result, ScreenshotFormat.Jpeg, quality);
		}

		static async Task<byte[]?> ScreenshotResultToArray(IScreenshotResult? result, ScreenshotFormat format, int quality)
		{
			if (result is null)
				return null;

			using var ms = new MemoryStream();
			await result.CopyToAsync(ms, format, quality);

			return ms.ToArray();
		}
	}
}
