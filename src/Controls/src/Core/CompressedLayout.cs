using System;
using System.ComponentModel;

namespace Microsoft.Maui.Controls
{
	public static class CompressedLayout
	{
		public static readonly BindableProperty IsHeadlessProperty =
			BindableProperty.Create("IsHeadless", typeof(bool), typeof(CompressedLayout), default(bool),
				propertyChanged: OnIsHeadlessPropertyChanged);

		public static bool GetIsHeadless(BindableObject bindable)
			=> (bool)bindable.GetValue(IsHeadlessProperty);

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

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static readonly BindableProperty HeadlessOffsetProperty = HeadlessOffsetPropertyKey.BindableProperty;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static Point GetHeadlessOffset(BindableObject bindable)
			=> (Point)bindable.GetValue(HeadlessOffsetProperty);

		internal static void SetHeadlessOffset(BindableObject bindable, Point value)
			=> bindable.SetValue(HeadlessOffsetPropertyKey, value);
	}
}