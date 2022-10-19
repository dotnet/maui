using System;
using System.ComponentModel;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/BoxView.xml" path="Type[@FullName='Microsoft.Maui.Controls.BoxView']/Docs/*" />
	public partial class BoxView : View, IColorElement, ICornerElement, IElementConfiguration<BoxView>
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/BoxView.xml" path="//Member[@MemberName='ColorProperty']/Docs/*" />
		public static readonly BindableProperty ColorProperty = ColorElement.ColorProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/BoxView.xml" path="//Member[@MemberName='CornerRadiusProperty']/Docs/*" />
		public static readonly BindableProperty CornerRadiusProperty = CornerElement.CornerRadiusProperty;

		readonly Lazy<PlatformConfigurationRegistry<BoxView>> _platformConfigurationRegistry;

		/// <include file="../../docs/Microsoft.Maui.Controls/BoxView.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public BoxView()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<BoxView>>(() => new PlatformConfigurationRegistry<BoxView>(this));
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/BoxView.xml" path="//Member[@MemberName='Color']/Docs/*" />
		public Color Color
		{
			get => (Color)GetValue(ColorElement.ColorProperty);
			set => SetValue(ColorElement.ColorProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/BoxView.xml" path="//Member[@MemberName='CornerRadius']/Docs/*" />
		public CornerRadius CornerRadius
		{
			get => (CornerRadius)GetValue(CornerElement.CornerRadiusProperty);
			set => SetValue(CornerElement.CornerRadiusProperty, value);
		}

		/// <inheritdoc/>
		public IPlatformElementConfiguration<T, BoxView> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
			return new SizeRequest(new Size(40, 40));
		}
	}
}