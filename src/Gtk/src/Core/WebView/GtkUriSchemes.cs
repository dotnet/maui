using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Maui.Gtk.Platform;
using WebKitContext = WebKit.WebContext;

namespace Microsoft.Maui.Gtk.Web
{
	/// <summary>
	/// Serves custom URI schemes (<c>app://</c>) to WebKitGTK web views.
	/// </summary>
	/// <remarks>
	/// <para>WebKitGTK registers a scheme once per <c>WebContext</c> and not per view, so the scheme is registered once on the default context
	/// and the request is routed to the handler registered for the requesting view (<c>URISchemeRequest.WebView</c>).</para>
	/// <para>The scheme is registered as secure and CORS-enabled: WebKit refuses <c>fetch()</c> to a custom scheme otherwise (HybridWebView
	/// and Blazor both fetch).</para>
	/// </remarks>
	internal static class GtkUriSchemes
	{
		const int ReadChunk = 16 * 1024;

		static readonly object s_gate = new();
		static readonly Dictionary<string, Dictionary<IntPtr, GtkUriSchemeHandler>> s_handlers = new(StringComparer.Ordinal);
		static readonly List<WebKit.URISchemeRequestCallback> s_callbacks = new();

		/// <summary>Routes requests to <paramref name="scheme"/> from <paramref name="view"/> to <paramref name="handler"/>.</summary>
		/// <param name="view">The web view.</param>
		/// <param name="scheme">The scheme, without <c>://</c>.</param>
		/// <param name="handler">The responder.</param>
		/// <returns>A token that removes the route.</returns>
		internal static IDisposable Register(global::WebKit.WebView view, string scheme, GtkUriSchemeHandler handler)
		{
			if (view is null)
				throw new ArgumentNullException(nameof(view));
			if (string.IsNullOrEmpty(scheme))
				throw new ArgumentException("The scheme cannot be empty.", nameof(scheme));
			if (handler is null)
				throw new ArgumentNullException(nameof(handler));

			var handle = view.Handle;
			bool first;

			lock (s_gate)
			{
				first = !s_handlers.TryGetValue(scheme, out var routes);
				if (first)
				{
					routes = new Dictionary<IntPtr, GtkUriSchemeHandler>();
					s_handlers[scheme] = routes;
				}

				routes![handle] = handler;
			}

			if (first)
			{
				var context = WebKitContext.Default;
				var security = context.SecurityManager;
				security.RegisterUriSchemeAsSecure(scheme);
				security.RegisterUriSchemeAsCorsEnabled(scheme);

				WebKit.URISchemeRequestCallback callback = request => OnRequest(scheme, request);
				lock (s_gate)
					s_callbacks.Add(callback);

				context.RegisterUriScheme(scheme, callback);
			}

			return new Route(scheme, handle);
		}

		static GtkUriSchemeHandler? Find(string scheme, IntPtr view)
		{
			lock (s_gate)
				return s_handlers.TryGetValue(scheme, out var routes) && routes.TryGetValue(view, out var handler) ? handler : null;
		}

		static async void OnRequest(string scheme, WebKit.URISchemeRequest request)
		{
			GtkUriSchemeResponse response;

			try
			{
				var view = request.WebView;
				var handler = view is null ? null : Find(scheme, view.Handle);

				response = handler is null
					? GtkUriSchemeResponse.NotFound()
					: await handler(ReadRequest(request, view!)).ConfigureAwait(true);
			}
			catch (Exception)
			{
				response = GtkUriSchemeResponse.ServerError();
			}

			// GTK objects may only be touched on the GTK thread; the handler may have completed elsewhere.
			GtkRuntime.Post(() => Finish(request, response));
		}

		static GtkUriSchemeRequest ReadRequest(WebKit.URISchemeRequest request, global::WebKit.WebView view)
		{
			var method = request.HttpMethod;
			return new GtkUriSchemeRequest(
				request.Uri ?? string.Empty,
				string.IsNullOrEmpty(method) ? "GET" : method,
				ReadBody(request),
				view.Uri);
		}

		static byte[] ReadBody(WebKit.URISchemeRequest request)
		{
			try
			{
				var stream = request.HttpBody;
				if (stream is null)
					return Array.Empty<byte>();

				using var buffer = new MemoryStream();
				var chunk = new byte[ReadChunk];

				while (true)
				{
					var read = stream.Read(chunk, (ulong)chunk.Length, null);
					if (read <= 0)
						break;

					buffer.Write(chunk, 0, (int)read);
				}

				stream.Close(null);
				return buffer.ToArray();
			}
			catch (EntryPointNotFoundException)
			{
				// WebKitGTK older than 2.40 cannot give the request body.
				return Array.Empty<byte>();
			}
		}

		static void Finish(WebKit.URISchemeRequest request, GtkUriSchemeResponse response)
		{
			var stream = response.Body.Length == 0
				? new GLib.MemoryInputStream()
				: new GLib.MemoryInputStream(new GLib.Bytes(response.Body));

			var webResponse = new WebKit.URISchemeResponse(stream, response.Body.Length);
			webResponse.SetStatus((uint)response.Status, response.Reason);
			if (response.ContentType is not null)
				webResponse.ContentType = response.ContentType;

			request.FinishWithResponse(webResponse);
		}

		sealed class Route : IDisposable
		{
			readonly string _scheme;
			readonly IntPtr _view;

			public Route(string scheme, IntPtr view)
			{
				_scheme = scheme;
				_view = view;
			}

			public void Dispose()
			{
				lock (s_gate)
				{
					if (s_handlers.TryGetValue(_scheme, out var routes))
						routes.Remove(_view);
				}
			}
		}
	}
}
