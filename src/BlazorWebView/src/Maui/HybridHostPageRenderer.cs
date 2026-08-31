using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.HtmlRendering.Infrastructure;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

// BL0006: this type intentionally derives from StaticHtmlRenderer and overrides the render-mode
// resolution hook — the same extension point the framework's own renderers use — to host a full
// document in a WebView. These APIs are stable enough for this controlled use.
#pragma warning disable BL0006

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	/// <summary>
	/// Statically renders a full-document host component (an <c>App.razor</c> equivalent) to an HTML
	/// string that can be served as the <see cref="BlazorWebView"/> host page, and collects the
	/// interactive components declared with a render mode so they can be attached to the live
	/// document after load.
	/// </summary>
	/// <remarks>
	/// Blazor Hybrid has no server endpoint, so it cannot render an interactive island inside the
	/// document like a Blazor Web App does. Instead, an interactive <c>@rendermode</c> boundary is
	/// converted into a mount element plus a selector-based attach registration:
	/// <list type="bullet">
	/// <item><description><c>HeadOutlet</c> is attached at <c>head::after</c>.</description></item>
	/// <item><description>Any other interactive component is attached at <c>#app</c>.</description></item>
	/// </list>
	/// The render mode value itself is documentary; all interactive modes are treated identically.
	/// </remarks>
	internal sealed class HybridHostPageRenderer : StaticHtmlRenderer
	{
		internal const string AppElementId = "app";
		internal const string HeadOutletSelector = "head::after";

		private readonly List<HybridRootComponentRegistration> _registrations = new();
		private readonly ResourceAssetCollection _assets;
		private int _mountElementCount;

		private HybridHostPageRenderer(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ResourceAssetCollection assets)
			: base(serviceProvider, loggerFactory)
		{
			_assets = assets;
		}

		/// <summary>
		/// Supplies the fingerprint-aware asset collection so that <c>@Assets["logical"]</c> resolves to
		/// the fingerprinted URL during the static host render. The base <c>Renderer.Assets</c> property is
		/// <c>protected internal virtual</c>, which makes this override the supported extension point
		/// (declared as <c>protected</c> here because the override is in a different assembly).
		/// </summary>
		protected override ResourceAssetCollection Assets => _assets;

		/// <summary>
		/// Asynchronously renders <paramref name="appComponentType"/> to a full HTML document and returns
		/// the markup together with the interactive components that must be attached to the live document.
		/// The render runs on the renderer's own dispatcher (a dedicated, thread-pool-serialized context -
		/// deliberately NOT the MAUI UI thread, since driving this static render on the UI thread deadlocks)
		/// and is awaited rather than blocked. The caller marshals the post-render work back to the MAUI
		/// dispatcher.
		/// </summary>
		/// <param name="services">The application service provider.</param>
		/// <param name="appComponentType">The host component type to render (the <c>App.razor</c> equivalent).</param>
		/// <param name="assets">The fingerprint-aware asset collection, or <c>null</c> to disable <c>@Assets</c> fingerprinting.</param>
		/// <returns>The rendered document markup and the collected interactive attach registrations.</returns>
		public static Task<HybridHostPageResult> RenderAsync(
			IServiceProvider services,
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type appComponentType,
			ResourceAssetCollection? assets = null)
		{
			ArgumentNullException.ThrowIfNull(services);
			ArgumentNullException.ThrowIfNull(appComponentType);

			var loggerFactory = services.GetService<ILoggerFactory>() ?? NullLoggerFactory.Instance;
			var resolvedAssets = assets ?? ResourceAssetCollection.Empty;

			var renderer = new HybridHostPageRenderer(services, loggerFactory, resolvedAssets);
			return renderer.Dispatcher.InvokeAsync(async () =>
			{
				try
				{
					var rootComponent = renderer.BeginRenderingComponent(appComponentType, ParameterView.Empty);

					// Wait for the component tree to finish rendering - including any asynchronous
					// initialization (for example OnInitializedAsync) - before serializing, otherwise
					// async host content (and its render-mode registrations) could be omitted from the
					// document. This mirrors how the framework's own static HTML rendering awaits quiescence.
					await rootComponent.QuiescenceTask.ConfigureAwait(false);

					var html = rootComponent.ToHtmlString();
					return new HybridHostPageResult(html, renderer._registrations);
				}
				finally
				{
					// Dispose on the renderer's dispatcher (required by Renderer) to release the
					// component tree and avoid leaking the renderer per host render.
					renderer.Dispose();
				}
			});
		}

		/// <summary>
		/// Synchronous fallback used when the host document is rendered outside the handler's async startup
		/// path (for example a direct <c>CreateFileProvider</c> call). Blocks on <see cref="RenderAsync"/>.
		/// </summary>
		public static HybridHostPageResult Render(
			IServiceProvider services,
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type appComponentType,
			ResourceAssetCollection? assets = null)
			=> Task.Run(() => RenderAsync(services, appComponentType, assets)).GetAwaiter().GetResult();

		/// <inheritdoc />
		protected override IComponent ResolveComponentForRenderMode(
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type componentType,
			int? parentComponentId,
			IComponentActivator componentActivator,
			IComponentRenderMode renderMode)
		{
			// The mode value is documentary — all interactive modes are intercepted identically.
			if (renderMode is InteractiveServerRenderMode or InteractiveWebAssemblyRenderMode or InteractiveAutoRenderMode)
			{
				if (componentType == typeof(HeadOutlet))
				{
					// HeadOutlet appends to the live <head>; nothing is emitted into the static document.
					_registrations.Add(new HybridRootComponentRegistration(HeadOutletSelector, componentType));
					return new HybridEmptyPlaceholder();
				}

				// Any other interactive root becomes a mount element that the live component attaches to.
				// Each root gets a unique element id (app, app-1, app-2, ...) so a host document that
				// declares more than one interactive boundary does not emit duplicate ids that collide
				// on a single querySelector('#app') target.
				var elementId = _mountElementCount == 0 ? AppElementId : $"{AppElementId}-{_mountElementCount}";
				_mountElementCount++;
				_registrations.Add(new HybridRootComponentRegistration("#" + elementId, componentType));
				return new HybridMountPlaceholder(elementId, componentType);
			}

			return base.ResolveComponentForRenderMode(componentType, parentComponentId, componentActivator, renderMode);
		}

		/// <summary>
		/// Statically renders a <c>&lt;div id="..."&gt;&lt;/div&gt;</c> mount element in place of an
		/// interactive component.
		/// </summary>
		private sealed class HybridMountPlaceholder : IComponent
		{
			private readonly string _elementId;
			private readonly Type _componentType;
			private RenderHandle _renderHandle;

			public HybridMountPlaceholder(string elementId, Type componentType)
			{
				_elementId = elementId;
				_componentType = componentType;
			}

			public void Attach(RenderHandle renderHandle) => _renderHandle = renderHandle;

			public Task SetParametersAsync(ParameterView parameters)
			{
				// A hybrid interactive boundary becomes a bare mount element plus a selector attach; there
				// is no channel to carry statically-authored parameters to the freshly-attached live
				// component, so silently dropping them would mis-render. Fail fast with a clear diagnostic
				// instead. (@rendermode itself is not a parameter and does not appear here.)
				foreach (var parameter in parameters)
				{
					throw new NotSupportedException(
						$"The interactive component '{_componentType.FullName}' declared in the {nameof(BlazorWebView.AppType)} host document has parameters (for example '{parameter.Name}'), which are not supported on a hybrid interactive boundary. Remove the parameters, or move them onto a child component rendered inside it.");
				}

				_renderHandle.Render(builder =>
				{
					builder.OpenElement(0, "div");
					builder.AddAttribute(1, "id", _elementId);
					builder.CloseElement();
				});
				return Task.CompletedTask;
			}
		}

		/// <summary>
		/// Renders nothing; used to remove an interactive component from the static document while
		/// still recording its attach registration.
		/// </summary>
		private sealed class HybridEmptyPlaceholder : IComponent
		{
			private RenderHandle _renderHandle;

			public void Attach(RenderHandle renderHandle) => _renderHandle = renderHandle;

			public Task SetParametersAsync(ParameterView parameters)
			{
				_renderHandle.Render(static _ => { });
				return Task.CompletedTask;
			}
		}
	}

	/// <summary>
	/// The result of rendering a host component: the document markup plus the interactive components
	/// to attach to the live document.
	/// </summary>
	internal readonly struct HybridHostPageResult
	{
		public HybridHostPageResult(string html, IReadOnlyList<HybridRootComponentRegistration> registrations)
		{
			Html = html;
			Registrations = registrations;
		}

		/// <summary>Gets the rendered full-document HTML markup.</summary>
		public string Html { get; }

		/// <summary>Gets the interactive components to attach after the document loads.</summary>
		public IReadOnlyList<HybridRootComponentRegistration> Registrations { get; }
	}

	/// <summary>
	/// Describes an interactive component to attach to the live document at a given CSS selector.
	/// </summary>
	internal readonly struct HybridRootComponentRegistration
	{
		public HybridRootComponentRegistration(string selector, Type componentType)
		{
			Selector = selector;
			ComponentType = componentType;
		}

		/// <summary>Gets the CSS selector the component attaches to (for example <c>#app</c>).</summary>
		public string Selector { get; }

		/// <summary>Gets the interactive component type.</summary>
		public Type ComponentType { get; }
	}
}
#pragma warning restore BL0006
