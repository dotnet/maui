namespace Microsoft.Maui.Graphics.Platform
{
	public class PlatformCanvasState : CanvasState
	{
		private bool _shadowed;

		public PlatformCanvasState() : base()
		{
		}

		public PlatformCanvasState(PlatformCanvasState prototype) : base(prototype)
		{
			_shadowed = prototype._shadowed;
		}

		public bool Shadowed
		{
			get => _shadowed;
			set => _shadowed = value;
		}
	}
}
