#nullable disable
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Graphics;
using static Microsoft.Maui.Primitives.Dimension;

namespace Microsoft.Maui.Controls
{
<<<<<<< HEAD
	/// <include file="../../docs/Microsoft.Maui.Controls/Editor.xml" path="Type[@FullName='Microsoft.Maui.Controls.Editor']/Docs/*" />
	[ElementHandler<EditorHandler>]
||||||| 3f26a592b2
	/// <include file="../../docs/Microsoft.Maui.Controls/Editor.xml" path="Type[@FullName='Microsoft.Maui.Controls.Editor']/Docs/*" />
=======
	/// <summary>A control that can edit multiple lines of text.</summary>
>>>>>>> 485b400ee4a317af11647f3e64085d7d8d4d5f17
	public partial class Editor : InputView, IEditorController, ITextAlignmentElement, IElementConfiguration<Editor>, IEditor
	{
		/// <summary>Identifies the Text bindable property.</summary>
		public new static readonly BindableProperty TextProperty = InputView.TextProperty;

		/// <inheritdoc cref="InputView.FontFamilyProperty"/>
		public new static readonly BindableProperty FontFamilyProperty = InputView.FontFamilyProperty;

		/// <inheritdoc cref="InputView.FontSizeProperty"/>
		public new static readonly BindableProperty FontSizeProperty = InputView.FontSizeProperty;

		/// <inheritdoc cref="InputView.FontAttributesProperty"/>
		public new static readonly BindableProperty FontAttributesProperty = InputView.FontAttributesProperty;

		/// <inheritdoc cref="InputView.FontAutoScalingEnabledProperty"/>
		public new static readonly BindableProperty FontAutoScalingEnabledProperty = InputView.FontAutoScalingEnabledProperty;

		/// <summary>Backing store for the <see cref="Microsoft.Maui.Controls.InputView.TextColor"/> property.</summary>
		public new static readonly BindableProperty TextColorProperty = InputView.TextColorProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Editor.xml" path="//Member[@MemberName='CharacterSpacingProperty']/Docs/*" />
		public new static readonly BindableProperty CharacterSpacingProperty = InputView.CharacterSpacingProperty;

		/// <summary>Backing store for the <see cref="Microsoft.Maui.Controls.InputView.Placeholder"/> property.</summary>
		public new static readonly BindableProperty PlaceholderProperty = InputView.PlaceholderProperty;

		/// <summary>Backing store for the <see cref="Microsoft.Maui.Controls.InputView.PlaceholderColor"/> property.</summary>
		public new static readonly BindableProperty PlaceholderColorProperty = InputView.PlaceholderColorProperty;

		/// <inheritdoc cref="InputView.IsTextPredictionEnabledProperty"/>
		public new static readonly BindableProperty IsTextPredictionEnabledProperty = InputView.IsTextPredictionEnabledProperty;

		/// <inheritdoc cref="InputView.CursorPositionProperty"/>
		public new static readonly BindableProperty CursorPositionProperty = InputView.CursorPositionProperty;

		/// <inheritdoc cref="InputView.SelectionLengthProperty"/>
		public new static readonly BindableProperty SelectionLengthProperty = InputView.SelectionLengthProperty;

		/// <summary>Bindable property for <see cref="AutoSize"/>.</summary>
		public static readonly BindableProperty AutoSizeProperty = BindableProperty.Create(nameof(AutoSize), typeof(EditorAutoSizeOption), typeof(Editor), defaultValue: EditorAutoSizeOption.Disabled, propertyChanged: (bindable, oldValue, newValue)
			=> ((Editor)bindable)?.UpdateAutoSizeOption());

		/// <summary>Bindable property for <see cref="HorizontalTextAlignment"/>.</summary>
		public static readonly BindableProperty HorizontalTextAlignmentProperty = TextAlignmentElement.HorizontalTextAlignmentProperty;

		/// <summary>Bindable property for <see cref="VerticalTextAlignment"/>.</summary>
		public static readonly BindableProperty VerticalTextAlignmentProperty = BindableProperty.Create(nameof(VerticalTextAlignment), typeof(TextAlignment), typeof(Editor), TextAlignment.Start);

		readonly Lazy<PlatformConfigurationRegistry<Editor>> _platformConfigurationRegistry;

		/// <summary>Gets or sets a value that controls whether the editor will change size to accommodate input as the user enters it.</summary>
		/// <remarks>Automatic resizing is turned off by default.</remarks>
		public EditorAutoSizeOption AutoSize
		{
			get { return (EditorAutoSizeOption)GetValue(AutoSizeProperty); }
			set { SetValue(AutoSizeProperty, value); }
		}

		public TextAlignment HorizontalTextAlignment
		{
			get { return (TextAlignment)GetValue(HorizontalTextAlignmentProperty); }
			set { SetValue(HorizontalTextAlignmentProperty, value); }
		}

		public TextAlignment VerticalTextAlignment
		{
			get { return (TextAlignment)GetValue(VerticalTextAlignmentProperty); }
			set { SetValue(VerticalTextAlignmentProperty, value); }
		}


		void UpdateAutoSizeOption()
		{
			if (AutoSize == EditorAutoSizeOption.TextChanges && this.IsShimmed())
				InvalidateMeasure();
		}

		public event EventHandler Completed;
		double _previousWidthConstraint;
		double _previousHeightConstraint;
		double _previousWidthRequest;
		double _previousHeightRequest;
		Thickness _previousMargin;

		Rect _previousBounds;

		/// <summary>Initializes a new instance of the Editor class.</summary>
		/// <remarks>The following example creates a Editor with a Chat keyboard that fills the available space.</remarks>
		public Editor()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<Editor>>(() => new PlatformConfigurationRegistry<Editor>(this));
		}

		/// <inheritdoc/>
		public IPlatformElementConfiguration<T, Editor> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		/// <summary>Internal API for Microsoft.Maui.Controls platform use.</summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendCompleted()
			=> Completed?.Invoke(this, EventArgs.Empty);

		protected override void OnTextChanged(string oldValue, string newValue)
		{
			base.OnTextChanged(oldValue, newValue);

			if (AutoSize == EditorAutoSizeOption.TextChanges)
			{
				InvalidateMeasure();
			}
		}

		public void OnHorizontalTextAlignmentPropertyChanged(TextAlignment oldValue, TextAlignment newValue)
		{
		}

		void IEditor.Completed()
		{
			(this as IEditorController).SendCompleted();
		}

		protected override Size ArrangeOverride(Rect bounds)
		{
			_previousBounds = bounds;
			return base.ArrangeOverride(bounds);
		}

		protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
		{
			if (AutoSize == EditorAutoSizeOption.TextChanges)
			{
				return base.MeasureOverride(widthConstraint, heightConstraint);
			}

			IView view = this;
			if (Width > 0 &&
				Height > 0 &&
				// If the user hasn't changed any of the WxH constraints then we don't want to remeasure
				// We want the Editor to remain the same size
				_previousWidthRequest == WidthRequest &&
				_previousHeightRequest == HeightRequest &&
				_previousMargin == Margin &&
				// If the user has explicitly set the width and height we don't need to special case around AutoSize
				// The explicitly set values will already stop any resizing from happening
				(!IsExplicitSet(view.Width) || !IsExplicitSet(view.Height))
				)
			{
				// check if the min/max constraints have changed and are no longer valid
				if (IsExplicitSet(view.MinimumWidth) && Width < view.MinimumWidth ||
					IsExplicitSet(view.MaximumWidth) && Width > view.MaximumWidth ||
					IsExplicitSet(view.MinimumHeight) && Height < view.MinimumHeight ||
					IsExplicitSet(view.MaximumHeight) && Height > view.MaximumHeight)
				{
					_previousWidthConstraint = -1;
					_previousHeightConstraint = -1;
				}
				else if ((TheSame(_previousHeightConstraint, heightConstraint) &&
					TheSame(_previousWidthConstraint, widthConstraint)) ||
					(TheSame(_previousHeightConstraint, _previousBounds.Height) &&
					TheSame(_previousWidthConstraint, _previousBounds.Width)))
				{
					// Just return previously established desired size
					return DesiredSize;
				}
			}

			_previousWidthRequest = WidthRequest;
			_previousHeightRequest = HeightRequest;
			_previousMargin = Margin;
			_previousWidthConstraint = widthConstraint;
			_previousHeightConstraint = heightConstraint;

			return base.MeasureOverride(widthConstraint, heightConstraint);

			bool TheSame(double width, double otherWidth)
			{
				return width == otherWidth ||
					(width - otherWidth) < double.Epsilon;
			}
		}

		internal override bool TrySetValue(string text)
		{
			Text = text;
			return true;
		}
	}
}
