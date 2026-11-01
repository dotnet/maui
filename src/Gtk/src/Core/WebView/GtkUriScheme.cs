using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Maui.Gtk.Web
{
	/// <summary>A request to a custom URI scheme (<c>app://</c>), reduced to the data the responders need.</summary>
	internal sealed class GtkUriSchemeRequest
	{
		/// <summary>Creates a request.</summary>
		public GtkUriSchemeRequest(string url, string method, byte[] body, string? pageUri, IReadOnlyCollection<KeyValuePair<string, string>>? headers = null)
		{
			Headers = headers;
			Url = url;
			Method = method;
			Body = body;
			PageUri = pageUri;
		}

		/// <summary>The absolute request URL.</summary>
		public string Url { get; }

		/// <summary>The HTTP method (<c>GET</c> when WebKit reports none).</summary>
		public string Method { get; }

		/// <summary>The request body; empty when there is none.</summary>
		public byte[] Body { get; }

		/// <summary>The request headers, or <see langword="null"/> where the engine exposes none (WebKitGTK).</summary>
		public IReadOnlyCollection<KeyValuePair<string, string>>? Headers { get; }

		/// <summary>The URI of the page that issued the request (the web view's current URI), if known.</summary>
		public string? PageUri { get; }
	}

	/// <summary>The answer to a <see cref="GtkUriSchemeRequest"/>.</summary>
	internal sealed class GtkUriSchemeResponse
	{
		/// <summary>Creates a response.</summary>
		public GtkUriSchemeResponse(int status, string reason, string? contentType, byte[]? body)
		{
			Status = status;
			Reason = reason;
			ContentType = contentType;
			Body = body ?? Array.Empty<byte>();
		}

		/// <summary>The HTTP status code.</summary>
		public int Status { get; }

		/// <summary>The HTTP reason phrase.</summary>
		public string Reason { get; }

		/// <summary>The <c>Content-Type</c>, or <see langword="null"/> for none.</summary>
		public string? ContentType { get; }

		/// <summary>The body bytes.</summary>
		public byte[] Body { get; }

		/// <summary>A <c>200 OK</c> response.</summary>
		public static GtkUriSchemeResponse Ok(string? contentType, byte[] body) => new(200, "OK", contentType, body);

		/// <summary>A <c>404 Not Found</c> response without body.</summary>
		public static GtkUriSchemeResponse NotFound() => new(404, "Not Found", null, null);

		/// <summary>A <c>400 Bad Request</c> response without body.</summary>
		public static GtkUriSchemeResponse BadRequest() => new(400, "Bad Request", null, null);

		/// <summary>A <c>405 Method Not Allowed</c> response without body.</summary>
		public static GtkUriSchemeResponse MethodNotAllowed() => new(405, "Method Not Allowed", null, null);

		/// <summary>A <c>500 Internal Server Error</c> response without body.</summary>
		public static GtkUriSchemeResponse ServerError() => new(500, "Internal Server Error", null, null);
	}

	/// <summary>Answers requests to a custom URI scheme for one web view.</summary>
	/// <param name="request">The request.</param>
	internal delegate Task<GtkUriSchemeResponse> GtkUriSchemeHandler(GtkUriSchemeRequest request);
}
