#nullable disable
using System;
using System.Diagnostics;

using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// A visual control used to indicate that something is ongoing.
	/// </summary>
	/// <remarks>
	/// This control gives a visual clue to the user that something is happening, without information about its progress.
	/// </remarks>
	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	[ElementHandler<ActivityIndicatorHandler>]
	public partial class ActivityIndicator : View, IColorElement, IElementConfiguration<ActivityIndicator>, IActivityIndicator
	{
		/// <summary>Bindable property for <see cref="IsRunning"/>.</summary>
		public static readonly BindableProperty IsRunningProperty = BindableProperty.Create(nameof(IsRunning), typeof(bool), typeof(ActivityIndicator), default(bool));

		/// <summary>Bindable property for <see cref="Color"/>.</summary>
		public static readonly BindableProperty ColorProperty = ColorElement.ColorProperty;

		readonly Lazy<PlatformConfigurationRegistry<ActivityIndicator>> _platformConfigurationRegistry;

		/// <summary>
		/// Initializes a new instance of the ActivityIndicator class.
		/// </summary>
		public ActivityIndicator()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<ActivityIndicator>>(() => new PlatformConfigurationRegistry<ActivityIndicator>(this));
		}

		/// <summary>
		/// Gets or sets the color of the ActivityIndicator. This is a bindable property.
		/// </summary>
		/// <value>A <see cref="Graphics.Color"/> used to display the ActivityIndicator.</value>
		public Color Color
		{
			get { return (Color)GetValue(ColorElement.ColorProperty); }
			set { SetValue(ColorElement.ColorProperty, value); }
		}

		/// <summary>
		/// Gets or sets a value indicating whether the ActivityIndicator is running (animating). This is a bindable property.
		/// </summary>
		/// <value><see langword="true"/> if the indicator is running; otherwise, <see langword="false"/>. The default is <see langword="false"/>.</value>
		public bool IsRunning
		{
			get { return (bool)GetValue(IsRunningProperty); }
			set { SetValue(IsRunningProperty, value); }
		}

		/// <inheritdoc/>
		public IPlatformElementConfiguration<T, ActivityIndicator> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		private protected override string GetDebuggerDisplay()
		{
			var debugText = DebuggerDisplayHelpers.GetDebugText(nameof(IsRunning), IsRunning);
			return $"{base.GetDebuggerDisplay()}, {debugText}";
		}
	}
}