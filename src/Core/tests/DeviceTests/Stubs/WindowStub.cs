namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class WindowStub : IWindow
	{
		public IElementHandler Handler { get; set; }

		public IElement Parent { get; set; }

		public IView Content { get; set; }

		public IVisualDiagnosticsOverlay VisualDiagnosticsOverlay { get; }

		public string Title { get; set; }
		public bool IsCreated { get; set; }
		public bool IsActivated { get; set; }
		public bool IsDeactivated { get; set; }
		public bool IsDestroyed { get; set; }
		public bool IsResumed { get; set; }
		public bool IsStopped { get; set; }

		public void Activated()
		{
			IsActivated = true;
			IsDeactivated = false;
		}

		public void Created()
		{
			IsCreated = true;
		}

		public void Deactivated()
		{
			IsActivated = false;
			IsDeactivated = false;
		}

		public void Destroying()
		{
			IsDestroyed = true;
			IsCreated = false;
		}

		public void Resumed()
		{
			IsResumed = true;
			IsStopped = false;
		}

		public void Stopped()
		{
			IsStopped = true;
			IsResumed = false;
		}

		public void Backgrounding(IPersistedState state)
		{
		}

		public bool BackButtonClicked() => true;
	}
}