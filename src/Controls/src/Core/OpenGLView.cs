using System;
using System.ComponentModel;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/OpenGLView.xml" path="Type[@FullName='Microsoft.Maui.Controls.OpenGLView']/Docs" />
	public sealed class OpenGLView : View, IOpenGlViewController, IElementConfiguration<OpenGLView>
	{
		#region Statics

		/// <include file="../../docs/Microsoft.Maui.Controls/OpenGLView.xml" path="//Member[@MemberName='HasRenderLoopProperty']/Docs" />
		public static readonly BindableProperty HasRenderLoopProperty = BindableProperty.Create("HasRenderLoop", typeof(bool), typeof(OpenGLView), default(bool));

		readonly Lazy<PlatformConfigurationRegistry<OpenGLView>> _platformConfigurationRegistry;

		#endregion

		/// <include file="../../docs/Microsoft.Maui.Controls/OpenGLView.xml" path="//Member[@MemberName='HasRenderLoop']/Docs" />
		public bool HasRenderLoop
		{
			get { return (bool)GetValue(HasRenderLoopProperty); }
			set { SetValue(HasRenderLoopProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/OpenGLView.xml" path="//Member[@MemberName='OnDisplay']/Docs" />
		public Action<Rectangle> OnDisplay { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/OpenGLView.xml" path="//Member[@MemberName='Display']/Docs" />
		public void Display()
			=> DisplayRequested?.Invoke(this, EventArgs.Empty);

		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler DisplayRequested;

		/// <include file="../../docs/Microsoft.Maui.Controls/OpenGLView.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public OpenGLView()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<OpenGLView>>(() => new PlatformConfigurationRegistry<OpenGLView>(this));
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/OpenGLView.xml" path="//Member[@MemberName='On']/Docs" />
		public IPlatformElementConfiguration<T, OpenGLView> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}
	}
}