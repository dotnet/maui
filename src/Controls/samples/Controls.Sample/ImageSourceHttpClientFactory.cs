using System;
using System.Net.Http;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

sealed class ImageSourceHttpClientFactory : IImageSourceHttpClientFactory
{
	readonly IHttpClientFactory _httpClientFactory;

	public ImageSourceHttpClientFactory(IHttpClientFactory httpClientFactory)
	{
		_httpClientFactory = httpClientFactory;
	}

	public bool ShouldDispose => false;

	public HttpClient CreateClient(Uri _)
	{
		// NOTE: possible to add logic depending upon image uri
		return _httpClientFactory.CreateClient(DefaultHttpClientName);
	}

	public static string DefaultHttpClientName => nameof(ImageSourceHttpClientFactory);
}