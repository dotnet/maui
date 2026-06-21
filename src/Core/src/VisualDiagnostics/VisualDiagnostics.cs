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
	/// <summary>
	/// Provides APIs for capturing source information, monitoring visual tree changes, and capturing screenshots for XAML and UI diagnostics.
	/// </summary>
	public static class VisualDiagnostics
	{
		static ConditionalWeakTable<object, SourceInfo> sourceInfos = new ConditionalWeakTable<object, SourceInfo>();
		static Lazy<bool> isVisualDiagnosticsEnvVarSet = new Lazy<bool>(() => Environment.GetEnvironmentVariable("ENABLE_XAML_DIAGNOSTICS_SOURCE_INFO") is { } value && value == "1");

		static internal bool IsEnabled => RuntimeFeature.EnableMauiDiagnostics || isVisualDiagnosticsEnvVarSet.Value;

		/// <summary>
		/// Registers source file information (URI, line number, and position) for the specified target object when XAML diagnostics are enabled.
		/// </summary>
		/// <param name="target">The object to associate with source information.</param>
		/// <param name="uri">The URI of the XAML file where the object was defined.</param>
		/// <param name="lineNumber">The line number in the XAML file.</param>
		/// <param name="linePosition">The position within the line in the XAML file.</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void RegisterSourceInfo(object target, Uri uri, int lineNumber, int linePosition)
		{
#if !NETSTANDARD2_0
			if (target != null && VisualDiagnostics.IsEnabled)
				sourceInfos.AddOrUpdate(target, new SourceInfo(uri, lineNumber, linePosition));
#else
			if (target != null && VisualDiagnostics.IsEnabled)
			{
				if (sourceInfos.TryGetValue(target, out _))
					sourceInfos.Remove(target);
				sourceInfos.Add(target, new SourceInfo(uri, lineNumber, linePosition));
			}
#endif
		}

		/// <summary>
		/// Gets the previously registered source information for a specified object.
		/// </summary>
		/// <param name="obj">The object whose source information is requested.</param>
		/// <returns>
		/// A <see cref="SourceInfo"/> instance containing the URI, line number, and position, or <c>null</c> if no information is available.
		/// </returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static SourceInfo? GetSourceInfo(object obj)
		{
			if (obj is null)
				return null;
			return sourceInfos.TryGetValue(obj, out var sourceinfo) ? sourceinfo : null;
		}

		/// <summary>
		/// Called when a child element is added to the visual tree; raises <see cref="VisualTreeChanged"/> event.
		/// </summary>
		/// <param name="parent">The parent visual element.</param>
		/// <param name="child">The child visual element that was added.</param>
		public static void OnChildAdded(IVisualTreeElement parent, IVisualTreeElement child)
		{
			if (!VisualDiagnostics.IsEnabled)
				return;

			if (child is null)
				return;

			var index = parent?.GetVisualChildren().IndexOf(child) ?? -1;

			OnChildAdded(parent, child, index);
		}

		/// <summary>
		/// Called when a child element is added to the visual tree at a specified logical index; raises <see cref="VisualTreeChanged"/> event.
		/// </summary>
		/// <param name="parent">The parent visual element, or null for root.</param>
		/// <param name="child">The child visual element that was added.</param>
		/// <param name="newLogicalIndex">The logical index at which the child was inserted.</param>
		public static void OnChildAdded(IVisualTreeElement? parent, IVisualTreeElement child, int newLogicalIndex)
		{
			if (!VisualDiagnostics.IsEnabled)
				return;

			if (child is null)
				return;

			OnVisualTreeChanged(new VisualTreeChangeEventArgs(parent, child, newLogicalIndex, VisualTreeChangeType.Add));
		}

		/// <summary>
		/// Called when a child element is removed from the visual tree; raises <see cref="VisualTreeChanged"/> event.
		/// </summary>
		/// <param name="parent">The parent visual element.</param>
		/// <param name="child">The child visual element that was removed.</param>
		/// <param name="oldLogicalIndex">The previous logical index of the child.</param>
		public static void OnChildRemoved(IVisualTreeElement parent, IVisualTreeElement child, int oldLogicalIndex)
		{
			if (!VisualDiagnostics.IsEnabled)
				return;

			OnVisualTreeChanged(new VisualTreeChangeEventArgs(parent, child, oldLogicalIndex, VisualTreeChangeType.Remove));
		}

		/// <summary>
		/// Event fired when the visual tree changes (child added or removed).
		/// </summary>
		/// <remarks>Subscribers receive <see cref="VisualTreeChangeEventArgs"/> with change details.</remarks>
		public static event EventHandler<VisualTreeChangeEventArgs>? VisualTreeChanged;

		static void OnVisualTreeChanged(VisualTreeChangeEventArgs e)
		{
			VisualTreeChanged?.Invoke(e.Parent, e);
		}

		/// <summary>
		/// Captures the given view as a PNG image asynchronously.
		/// </summary>
		/// <param name="view">The view to capture.</param>
		/// <returns>A byte array containing the PNG image, or null if capture failed.</returns>
		public static async Task<byte[]?> CaptureAsPngAsync(IView view)
		{
			var result = await view.CaptureAsync();
			return await ScreenshotResultToArray(result, ScreenshotFormat.Png, 100);
		}

		/// <summary>
		/// Captures the given view as a JPEG image asynchronously with specified quality.
		/// </summary>
		/// <param name="view">The view to capture.</param>
		/// <param name="quality">The JPEG quality (0-100).</param>
		/// <returns>A byte array containing the JPEG image, or null if capture failed.</returns>
		public static async Task<byte[]?> CaptureAsJpegAsync(IView view, int quality = 80)
		{
			var result = await view.CaptureAsync();
			return await ScreenshotResultToArray(result, ScreenshotFormat.Jpeg, quality);
		}

		/// <summary>
		/// Captures the given window as a PNG image asynchronously.
		/// </summary>
		/// <param name="window">The window to capture.</param>
		/// <returns>A byte array containing the PNG image, or null if capture failed.</returns>
		public static async Task<byte[]?> CaptureAsPngAsync(IWindow window)
		{
			var result = await window.CaptureAsync();
			return await ScreenshotResultToArray(result, ScreenshotFormat.Png, 100);
		}

		/// <summary>
		/// Captures the given window as a JPEG image asynchronously with specified quality.
		/// </summary>
		/// <param name="window">The window to capture.</param>
		/// <param name="quality">The JPEG quality (0-100).</param>
		/// <returns>A byte array containing the JPEG image, or null if capture failed.</returns>
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
