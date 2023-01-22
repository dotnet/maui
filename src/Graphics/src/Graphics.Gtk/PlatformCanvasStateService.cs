using System;

namespace Microsoft.Maui.Graphics.Platform.Gtk;

public class PlatformCanvasStateService : ICanvasStateService<PlatformCanvasState> {
	public PlatformCanvasState CreateNew (object context) => new() { };

	public PlatformCanvasState CreateCopy (PlatformCanvasState prototype) => new(prototype);
}