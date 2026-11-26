#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Attached properties that present a modal page as a native bottom sheet.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Set <see cref="IsEnabledProperty"/> to <see langword="true"/> on a page and present it with
	/// <see cref="INavigation.PushModalAsync(Page)"/> (or a Shell route) to show it as the platform's native sheet:
	/// a <c>UISheetPresentationController</c> sheet on iOS and Mac Catalyst, and a Material <c>BottomSheetDialog</c> on Android.
	/// Dismiss it with <see cref="INavigation.PopModalAsync()"/>, or let the user dismiss it with the platform gestures.
	/// The page keeps the lifecycle of a modal page.
	/// </para>
	/// <para>
	/// Set the properties on the page that is pushed, for example on the <see cref="NavigationPage"/> when a navigation stack
	/// is shown in the sheet.
	/// </para>
	/// <para>
	/// The presentation adapts to the device like the native sheet does: on iPad and Mac Catalyst the system can show the sheet
	/// centered rather than attached to the bottom edge, and Mac Catalyst ignores detents and the drag handle.
	/// Windows has no native bottom sheet, so the page is shown as a regular modal page and these properties have no effect.
	/// </para>
	/// </remarks>
	public static class BottomSheet
	{
		static readonly IReadOnlyList<BottomSheetDetent> DefaultDetents = new ReadOnlyCollection<BottomSheetDetent>(new[] { BottomSheetDetent.Large });

		/// <summary>Bindable property for attached property <c>IsEnabled</c>.</summary>
		public static readonly BindableProperty IsEnabledProperty =
			BindableProperty.CreateAttached("IsEnabled", typeof(bool), typeof(BottomSheet), false);

		/// <summary>Bindable property for attached property <c>Detents</c>.</summary>
		public static readonly BindableProperty DetentsProperty =
			BindableProperty.CreateAttached("Detents", typeof(IReadOnlyList<BottomSheetDetent>), typeof(BottomSheet), DefaultDetents,
				validateValue: (_, value) => IsValidDetents(value as IReadOnlyList<BottomSheetDetent>),
				coerceValue: (_, value) => Snapshot((IReadOnlyList<BottomSheetDetent>)value));

		/// <summary>Bindable property for attached property <c>SelectedDetent</c>.</summary>
		public static readonly BindableProperty SelectedDetentProperty =
			BindableProperty.CreateAttached("SelectedDetent", typeof(BottomSheetDetent), typeof(BottomSheet), null, BindingMode.TwoWay);

		/// <summary>Bindable property for attached property <c>HasDragHandle</c>.</summary>
		public static readonly BindableProperty HasDragHandleProperty =
			BindableProperty.CreateAttached("HasDragHandle", typeof(bool), typeof(BottomSheet), true);

		/// <summary>Bindable property for attached property <c>IsDismissible</c>.</summary>
		public static readonly BindableProperty IsDismissibleProperty =
			BindableProperty.CreateAttached("IsDismissible", typeof(bool), typeof(BottomSheet), true);

		/// <summary>Bindable property for attached property <c>CornerRadius</c>.</summary>
		public static readonly BindableProperty CornerRadiusProperty =
			BindableProperty.CreateAttached("CornerRadius", typeof(double), typeof(BottomSheet), -1d,
				validateValue: (_, value) => value is double radius && IsValidCornerRadius(radius));

		/// <summary>
		/// Gets whether the page is presented as a native bottom sheet when it is pushed modally.
		/// </summary>
		/// <param name="bindable">The page.</param>
		/// <returns><see langword="true"/> if the page is presented as a bottom sheet; otherwise, <see langword="false"/>.</returns>
		public static bool GetIsEnabled(BindableObject bindable) => (bool)bindable.GetValue(IsEnabledProperty);

		/// <summary>
		/// Sets whether the page is presented as a native bottom sheet when it is pushed modally. The default is <see langword="false"/>.
		/// </summary>
		/// <param name="bindable">The page.</param>
		/// <param name="value"><see langword="true"/> to present the page as a bottom sheet.</param>
		/// <remarks>Set this before the page is pushed. With Shell, a page with this property set is presented modally.</remarks>
		public static void SetIsEnabled(BindableObject bindable, bool value) => bindable.SetValue(IsEnabledProperty, value);

		/// <summary>
		/// Gets the heights at which the sheet can rest.
		/// </summary>
		/// <param name="bindable">The page.</param>
		/// <returns>The detents. The default is a single <see cref="BottomSheetDetent.Large"/> detent.</returns>
		[TypeConverter(typeof(BottomSheetDetentCollectionTypeConverter))]
		public static IReadOnlyList<BottomSheetDetent> GetDetents(BindableObject bindable) => (IReadOnlyList<BottomSheetDetent>)bindable.GetValue(DetentsProperty);

		/// <summary>
		/// Sets the heights at which the sheet can rest.
		/// </summary>
		/// <param name="bindable">The page.</param>
		/// <param name="value">One to three detents. Duplicates are ignored, and the platform orders them by their height.</param>
		/// <exception cref="ArgumentException"><paramref name="value"/> is <see langword="null"/>, empty, contains <see langword="null"/>, or has more than three detents.</exception>
		/// <remarks>
		/// Android supports at most three resting positions, so a sheet can declare at most three detents. In XAML, use a comma-separated
		/// list such as <c>"Medium, Large"</c>. The sheet keeps a copy of the list: to change the detents, including while the sheet is presented,
		/// set a new list.
		/// An invalid value set through XAML or a binding is ignored, and a warning is logged.
		/// </remarks>
		public static void SetDetents(BindableObject bindable, IReadOnlyList<BottomSheetDetent> value)
		{
			if (!IsValidDetents(value))
				throw new ArgumentException("A bottom sheet needs one to three detents, none of which is null.", nameof(value));

			bindable.SetValue(DetentsProperty, value);
		}

		/// <summary>
		/// Gets the detent the sheet rests at.
		/// </summary>
		/// <param name="bindable">The page.</param>
		/// <returns>The selected detent, or <see langword="null"/> when no detent was selected and the sheet uses its smallest detent.</returns>
		public static BottomSheetDetent? GetSelectedDetent(BindableObject bindable) => (BottomSheetDetent?)bindable.GetValue(SelectedDetentProperty);

		/// <summary>
		/// Sets the detent the sheet rests at.
		/// </summary>
		/// <param name="bindable">The page.</param>
		/// <param name="value">One of the sheet's <see cref="GetDetents(BindableObject)">detents</see>, or <see langword="null"/> for the smallest detent.</param>
		/// <remarks>
		/// Setting this while the sheet is presented moves the sheet to that detent. When the user drags the sheet to another detent, the property
		/// is updated, so it binds two-way by default. A detent that is not one of the sheet's detents doesn't move the sheet; when the sheet is presented
		/// with such a selection, it uses its smallest detent.
		/// When several detents resolve to the same height, for example <see cref="BottomSheetDetent.FitContent"/> for content that is taller than the screen
		/// and <see cref="BottomSheetDetent.Large"/>, they share one resting position.
		/// </remarks>
		public static void SetSelectedDetent(BindableObject bindable, BottomSheetDetent? value) => bindable.SetValue(SelectedDetentProperty, value);

		/// <summary>
		/// Gets whether the sheet shows a drag handle (the grabber on iOS).
		/// </summary>
		/// <param name="bindable">The page.</param>
		/// <returns><see langword="true"/> if the sheet shows a drag handle; otherwise, <see langword="false"/>.</returns>
		public static bool GetHasDragHandle(BindableObject bindable) => (bool)bindable.GetValue(HasDragHandleProperty);

		/// <summary>
		/// Sets whether the sheet shows a drag handle (the grabber on iOS). The default is <see langword="true"/>.
		/// </summary>
		/// <param name="bindable">The page.</param>
		/// <param name="value"><see langword="true"/> to show a drag handle.</param>
		/// <remarks>The platform can still hide the handle, for example on iOS when the sheet is full screen in compact height.</remarks>
		public static void SetHasDragHandle(BindableObject bindable, bool value) => bindable.SetValue(HasDragHandleProperty, value);

		/// <summary>
		/// Gets whether the user can dismiss the sheet.
		/// </summary>
		/// <param name="bindable">The page.</param>
		/// <returns><see langword="true"/> if the user can dismiss the sheet; otherwise, <see langword="false"/>.</returns>
		public static bool GetIsDismissible(BindableObject bindable) => (bool)bindable.GetValue(IsDismissibleProperty);

		/// <summary>
		/// Sets whether the user can dismiss the sheet by swiping it down, tapping outside it, or pressing the system back button.
		/// The default is <see langword="true"/>.
		/// </summary>
		/// <param name="bindable">The page.</param>
		/// <param name="value"><see langword="false"/> to prevent the user from dismissing the sheet.</param>
		/// <remarks>
		/// This never prevents <see cref="INavigation.PopModalAsync()"/>. When it is <see langword="false"/>, provide a way to close the sheet.
		/// A <see cref="NavigationPage"/> in the sheet still navigates back on the system back button.
		/// </remarks>
		public static void SetIsDismissible(BindableObject bindable, bool value) => bindable.SetValue(IsDismissibleProperty, value);

		/// <summary>
		/// Gets the radius of the sheet's top corners.
		/// </summary>
		/// <param name="bindable">The page.</param>
		/// <returns>The radius in device-independent units, or -1 for the platform default.</returns>
		public static double GetCornerRadius(BindableObject bindable) => (double)bindable.GetValue(CornerRadiusProperty);

		/// <summary>
		/// Sets the radius of the sheet's top corners. The default is -1, which uses the platform's radius.
		/// </summary>
		/// <param name="bindable">The page.</param>
		/// <param name="value">The radius in device-independent units, or -1 for the platform default.</param>
		/// <exception cref="ArgumentException"><paramref name="value"/> is negative (other than -1) or not finite.</exception>
		public static void SetCornerRadius(BindableObject bindable, double value)
		{
			if (!IsValidCornerRadius(value))
				throw new ArgumentException("The corner radius must be -1 or a finite value of at least 0.", nameof(value));

			bindable.SetValue(CornerRadiusProperty, value);
		}

		/// <summary>Gets whether this platform presents bottom sheets natively.</summary>
		internal static bool IsSupported =>
#if ANDROID || IOS || MACCATALYST
			true;
#else
			false;
#endif

		/// <summary>Gets whether <paramref name="page"/> is a modal page that is presented as a native bottom sheet.</summary>
		internal static bool IsPresentedAsBottomSheet(Page page) =>
			IsSupported && GetIsEnabled(page) && page.RealParent is Window window && window.Page != page;

		// The sheet keeps its own copy, so changing a list after setting it has no effect: set a new list to change the detents.
		static IReadOnlyList<BottomSheetDetent> Snapshot(IReadOnlyList<BottomSheetDetent> detents)
		{
			if (detents is ReadOnlyCollection<BottomSheetDetent> { } snapshot && ReferenceEquals(snapshot, DefaultDetents))
				return snapshot;

			var copy = new BottomSheetDetent[detents.Count];
			for (int i = 0; i < copy.Length; i++)
				copy[i] = detents[i];

			return new ReadOnlyCollection<BottomSheetDetent>(copy);
		}

		static bool IsValidCornerRadius(double radius) =>
			radius == -1d || (radius >= 0 && !double.IsInfinity(radius));

		static bool IsValidDetents(IReadOnlyList<BottomSheetDetent>? detents)
		{
			if (detents is null || detents.Count == 0 || detents.Count > 3)
				return false;

			for (int i = 0; i < detents.Count; i++)
			{
				if (detents[i] is null)
					return false;
			}

			return true;
		}
	}
}
