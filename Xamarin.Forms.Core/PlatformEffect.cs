using System.ComponentModel;

namespace Xamarin.Forms
{
	public abstract class PlatformEffect<TContainer, TControl> : Effect where TContainer : class where TControl : class
	{
		public TContainer Container { get; internal set; }

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SetContainer(TContainer container) => Container = container;

		public TControl Control { get; internal set; }

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SetControl(TControl control) => Control = control;

		protected virtual void OnElementPropertyChanged(PropertyChangedEventArgs args)
		{
		}

		internal override void SendDetached()
		{
			base.SendDetached();
			Container = null;
			Control = null;
		}

		internal override void SendOnElementPropertyChanged(PropertyChangedEventArgs args)
		{
			if (IsAttached)
				OnElementPropertyChanged(args);
		}
	}
}