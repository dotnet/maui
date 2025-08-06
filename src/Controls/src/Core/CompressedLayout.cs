#nullable disable
using System;
using System.ComponentModel;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/CompressedLayout.xml" path="Type[@FullName='Microsoft.Maui.Controls.CompressedLayout']/Docs/*" />
	[Obsolete("CompressedLayout does not provide meaningful functionality and may be removed in a future release. Please remove usage of this API.")]
	public static class CompressedLayout
	{
		/// <summary>Bindable property for <c>IsHeadless</c>.</summary>
		[Obsolete("CompressedLayout does not provide meaningful functionality and may be removed in a future release. Please remove usage of this API.")]
		public static readonly BindableProperty IsHeadlessProperty =
			BindableProperty.Create("IsHeadless", typeof(bool), typeof(CompressedLayout), default(bool),
				propertyChanged: OnIsHeadlessPropertyChanged);

		/// <include file="../../docs/Microsoft.Maui.Controls/CompressedLayout.xml" path="//Member[@MemberName='GetIsHeadless']/Docs/*" />
		[Obsolete("CompressedLayout does not provide meaningful functionality and may be removed in a future release. Please remove usage of this API.")]
		public static bool GetIsHeadless(BindableObject bindable)
			=> (bool)bindable.GetValue(IsHeadlessProperty);

		/// <include file="../../docs/Microsoft.Maui.Controls/CompressedLayout.xml" path="//Member[@MemberName='SetIsHeadless']/Docs/*" />
		[Obsolete("CompressedLayout does not provide meaningful functionality and may be removed in a future release. Please remove usage of this API.")]
		public static void SetIsHeadless(BindableObject bindable, bool value)
			=> bindable.SetValue(IsHeadlessProperty, value);

		static void OnIsHeadlessPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var ve = bindable as IVisualElementController;
			if (ve == null)
				return;
			if (ve.IsPlatformEnabled)
				throw new InvalidOperationException("IsHeadless cannot be modified when the view is rendered");
		}

		static readonly BindablePropertyKey HeadlessOffsetPropertyKey =
			BindableProperty.CreateReadOnly("HeadlessOffset", typeof(Point), typeof(CompressedLayout), default(Point));

		/// <summary>Bindable property for <c>HeadlessOffset</c>.</summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("CompressedLayout does not provide meaningful functionality and may be removed in a future release. Please remove usage of this API.")]
		public static readonly BindableProperty HeadlessOffsetProperty = HeadlessOffsetPropertyKey.BindableProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/CompressedLayout.xml" path="//Member[@MemberName='GetHeadlessOffset']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("CompressedLayout does not provide meaningful functionality and may be removed in a future release. Please remove usage of this API.")]
		public static Point GetHeadlessOffset(BindableObject bindable)
			=> (Point)bindable.GetValue(HeadlessOffsetProperty);

		internal static void SetHeadlessOffset(BindableObject bindable, Point value)
			=> bindable.SetValue(HeadlessOffsetPropertyKey, value);
	}
}