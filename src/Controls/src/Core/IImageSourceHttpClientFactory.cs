#nullable enable

using System;
using System.Net.Http;

namespace Microsoft.Maui.Controls;

public interface IImageSourceHttpClientFactory
{
	bool ShouldDispose { get; }
	HttpClient CreateClient(Uri imageUri);
}