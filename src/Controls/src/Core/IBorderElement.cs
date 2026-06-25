#nullable disable
using System;
using System.ComponentModel;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IBorderElement
	{
		//note to implementor: implement this property publicly
		Color BorderColor { get; }
		int CornerRadius { get; }

		/// <inheritdoc cref="VisualElement.BackgroundColor"/>
#if NET5_0_OR_GREATER
		[Obsolete("BackgroundColor is obsolete. Use Background instead.",
			DiagnosticId = MauiObsoleteConstants.BackgroundColorObsolete)]
#else
		[Obsolete("BackgroundColor is obsolete. Use Background instead.")]
#endif
		Color BackgroundColor { get; }

		Brush Background { get; }
		double BorderWidth { get; }

		//note to implementor: but implement the methods explicitly
		void OnBorderColorPropertyChanged(Color oldValue, Color newValue);
		bool IsCornerRadiusSet();

		/// <summary>Returns whether <see cref="BackgroundColor"/> has been explicitly set. Use <see cref="IsBackgroundSet"/> instead.</summary>
#if NET5_0_OR_GREATER
		[Obsolete("IsBackgroundColorSet is obsolete. Use IsBackgroundSet instead.",
			DiagnosticId = MauiObsoleteConstants.BackgroundColorObsolete)]
#else
		[Obsolete("IsBackgroundColorSet is obsolete. Use IsBackgroundSet instead.")]
#endif
		bool IsBackgroundColorSet();

		bool IsBackgroundSet();
		bool IsBorderColorSet();
		bool IsBorderWidthSet();
		int CornerRadiusDefaultValue { get; }
		Color BorderColorDefaultValue { get; }
		double BorderWidthDefaultValue { get; }
	}
}