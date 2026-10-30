using System;
using Microsoft.Maui.Gtk.Hosting;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	/// <summary>
	/// GTK 4 registration of <see cref="BlazorWebView"/>.
	/// </summary>
	public static class BlazorWebViewGtkExtensions
	{
		/// <summary>
		/// Makes <see cref="BlazorWebView"/> use <see cref="GtkBlazorWebViewHandler"/> (WebKitGTK 6).
		/// </summary>
		/// <remarks>
		/// <para>Call it on the builder <c>AddMauiBlazorWebView()</c> returns, after the GTK backend was registered
		/// (<c>UseMauiGtk()</c> or <c>UseMauiGtkCore()</c>):</para>
		/// <code>
		/// builder.UseMauiApp&lt;App&gt;().UseMauiGtk();
		/// builder.Services.AddMauiBlazorWebView().UseGtkPlatformHandler();
		/// </code>
		/// <para>It also maps <see cref="BlazorWebViewHandler"/> to <see cref="GtkBlazorWebViewHandler"/> in <see cref="GtkHandlerMap"/>, so a
		/// <c>BlazorWebView</c> registered by another library is rewritten too.</para>
		/// </remarks>
		/// <param name="builder">The builder returned by <c>AddMauiBlazorWebView()</c>.</param>
		/// <returns><paramref name="builder"/>, for chaining.</returns>
		public static IMauiBlazorWebViewBuilder UseGtkPlatformHandler(this IMauiBlazorWebViewBuilder builder)
		{
			ArgumentNullException.ThrowIfNull(builder);

			GtkHandlerMap.Add<BlazorWebViewHandler, GtkBlazorWebViewHandler>();
			return builder.UsePlatformHandler<GtkBlazorWebViewHandler>();
		}
	}
}
