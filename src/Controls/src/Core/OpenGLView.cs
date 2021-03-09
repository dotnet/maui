using System;
using System.ComponentModel;

namespace Microsoft.Maui.Controls
{
	public sealed class OpenGLView : View, IOpenGlViewController, IElementConfiguration<OpenGLView>
	{
		#region Statics

		public static readonly BindableProperty HasRenderLoopProperty = BindableProperty.Create("HasRenderLoop", typeof(bool), typeof(OpenGLView), default(bool));

		readonly Lazy<PlatformConfigurationRegistry<OpenGLView>> _platformConfigurationRegistry;

		#endregion

		public bool HasRenderLoop
		{
			get { return (bool)GetValue(HasRenderLoopProperty); }
			set { SetValue(HasRenderLoopProperty, value); }
		}

		public Action<Rectangle> OnDisplay { get; set; }

		public void Display()
			=> DisplayRequested?.Invoke(this, EventArgs.Empty);

		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler DisplayRequested;

		public OpenGLView()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<OpenGLView>>(() => new PlatformConfigurationRegistry<OpenGLView>(this));
		}

		public IPlatformElementConfiguration<T, OpenGLView> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}
	}
}