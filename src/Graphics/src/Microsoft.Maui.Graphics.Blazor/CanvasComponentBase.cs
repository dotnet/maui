using System;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Microsoft.Maui.Graphics.Blazor
{
	public class CanvasComponentBase : ComponentBase
	{
		private BlazorCanvas _blazorCanvas;

		[Parameter]
#pragma warning disable BL0004 // Component parameter should be public.
		protected long width { get; set; }
#pragma warning restore BL0004 // Component parameter should be public.

		[Parameter]
#pragma warning disable BL0004 // Component parameter should be public.
		protected long height { get; set; }
#pragma warning restore BL0004 // Component parameter should be public.

		[Inject]
		public IJSRuntime JSRuntime { get; set; }

		protected readonly string id = Guid.NewGuid().ToString();

		protected ElementReference canvas;

		internal ElementReference CanvasReference => canvas;

		public string Id => id;

		private bool _initialized;
		private float _displayScale = 1;

		public BlazorCanvas BlazorCanvas
		{
			get
			{
				if (_blazorCanvas == null)
				{
					_blazorCanvas = new BlazorCanvas
					{
						Context = new Canvas2D.CanvasRenderingContext2D(this),
						DisplayScale = _displayScale
					};
				}

				return _blazorCanvas;
			}
		}

		protected override async void OnAfterRender(bool firstRender)
		{
			if (!_initialized)
			{
				_initialized = true;
				_displayScale = await JSRuntime.SetupCanvas(Id);
			}
		}
	}
}
