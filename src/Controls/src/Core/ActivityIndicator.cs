using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/ActivityIndicator.xml" path="Type[@FullName='Microsoft.Maui.Controls.ActivityIndicator']/Docs/*" />
	public partial class ActivityIndicator : View, IColorElement, IElementConfiguration<ActivityIndicator>
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/ActivityIndicator.xml" path="//Member[@MemberName='IsRunningProperty']/Docs/*" />
		public static readonly BindableProperty IsRunningProperty = BindableProperty.Create("IsRunning", typeof(bool), typeof(ActivityIndicator), default(bool));

		/// <include file="../../docs/Microsoft.Maui.Controls/ActivityIndicator.xml" path="//Member[@MemberName='ColorProperty']/Docs/*" />
		public static readonly BindableProperty ColorProperty = ColorElement.ColorProperty;

		readonly Lazy<PlatformConfigurationRegistry<ActivityIndicator>> _platformConfigurationRegistry;

		/// <include file="../../docs/Microsoft.Maui.Controls/ActivityIndicator.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public ActivityIndicator()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<ActivityIndicator>>(() => new PlatformConfigurationRegistry<ActivityIndicator>(this));
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ActivityIndicator.xml" path="//Member[@MemberName='Color']/Docs/*" />
		public Color Color
		{
			get { return (Color)GetValue(ColorElement.ColorProperty); }
			set { SetValue(ColorElement.ColorProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ActivityIndicator.xml" path="//Member[@MemberName='IsRunning']/Docs/*" />
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
	}
}