#nullable disable
using System;
using System.ComponentModel;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>Contains attached properties for omitting redundant renderers.</summary>
	[Obsolete("CompressedLayout does not provide meaningful functionality and may be removed in a future release. Please remove usage of this API.")]
	public static class CompressedLayout
	{
		/// <summary>Bindable property for <c>IsHeadless</c>.</summary>
		[Obsolete("CompressedLayout does not provide meaningful functionality and may be removed in a future release. Please remove usage of this API.")]
		public static readonly BindableProperty IsHeadlessProperty =
			BindableProperty.Create("IsHeadless", typeof(bool), typeof(CompressedLayout), default(bool),
				propertyChanged: OnIsHeadlessPropertyChanged);

		/// <summary>Gets a Boolean value that tells whether layout compression is enabled for the specified bindable object.</summary>
		/// <param name="bindable">The <see cref="BindableObject" /> whose status to check.</param>
		/// <returns><see langword="true" /> if layout compression is enabled for <paramref name="bindable" />. Otherwise, returns <see langword="false" />.</returns>
		[Obsolete("CompressedLayout does not provide meaningful functionality and may be removed in a future release. Please remove usage of this API.")]
		public static bool GetIsHeadless(BindableObject bindable)
			=> (bool)bindable.GetValue(IsHeadlessProperty);

		/// <summary>Turns layout compression on or off for the specified bindable object.</summary>
		/// <param name="bindable">The <see cref="BindableObject" /> on which to enable or disable layout compression</param>
		/// <param name="value">The new layout compression value. <see langword="true" /> to enable layout compression</param>
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

		/// <summary>For internal use by the Microsoft.Maui.Controls platform.</summary>
		/// <param name="bindable">For internal use by the Microsoft.Maui.Controls platform.</param>
		/// <returns>For internal use by the Microsoft.Maui.Controls platform.</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("CompressedLayout does not provide meaningful functionality and may be removed in a future release. Please remove usage of this API.")]
		public static Point GetHeadlessOffset(BindableObject bindable)
			=> (Point)bindable.GetValue(HeadlessOffsetProperty);

		internal static void SetHeadlessOffset(BindableObject bindable, Point value)
			=> bindable.SetValue(HeadlessOffsetPropertyKey, value);
	}
}