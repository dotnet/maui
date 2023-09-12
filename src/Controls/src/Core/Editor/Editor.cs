#nullable disable
using System;
using System.ComponentModel;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/Editor.xml" path="Type[@FullName='Microsoft.Maui.Controls.Editor']/Docs/*" />
	public partial class Editor : InputView, IEditorController, ITextAlignmentElement, IElementConfiguration<Editor>, IEditor
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/Editor.xml" path="//Member[@MemberName='TextProperty']/Docs/*" />
		public new static readonly BindableProperty TextProperty = InputView.TextProperty;

		/// <inheritdoc cref="InputView.FontFamilyProperty"/>
		public new static readonly BindableProperty FontFamilyProperty = InputView.FontFamilyProperty;

		/// <inheritdoc cref="InputView.FontSizeProperty"/>
		public new static readonly BindableProperty FontSizeProperty = InputView.FontSizeProperty;

		/// <inheritdoc cref="InputView.FontAttributesProperty"/>
		public new static readonly BindableProperty FontAttributesProperty = InputView.FontAttributesProperty;

		/// <inheritdoc cref="InputView.FontAutoScalingEnabledProperty"/>
		public new static readonly BindableProperty FontAutoScalingEnabledProperty = InputView.FontAutoScalingEnabledProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Editor.xml" path="//Member[@MemberName='TextColorProperty']/Docs/*" />
		public new static readonly BindableProperty TextColorProperty = InputView.TextColorProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Editor.xml" path="//Member[@MemberName='CharacterSpacingProperty']/Docs/*" />
		public new static readonly BindableProperty CharacterSpacingProperty = InputView.CharacterSpacingProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Editor.xml" path="//Member[@MemberName='PlaceholderProperty']/Docs/*" />
		public new static readonly BindableProperty PlaceholderProperty = InputView.PlaceholderProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Editor.xml" path="//Member[@MemberName='PlaceholderColorProperty']/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/Editor.xml" path="//Member[@MemberName='AutoSize']/Docs/*" />
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
		Rect _previousBounds;

		/// <include file="../../docs/Microsoft.Maui.Controls/Editor.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public Editor()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<Editor>>(() => new PlatformConfigurationRegistry<Editor>(this));
		}

		/// <inheritdoc/>
		public IPlatformElementConfiguration<T, Editor> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Editor.xml" path="//Member[@MemberName='SendCompleted']/Docs/*" />
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
			if (AutoSize == EditorAutoSizeOption.Disabled &&
				Width > 0 &&
				Height > 0)
			{
				if (TheSame(_previousHeightConstraint, heightConstraint) &&
					TheSame(_previousWidthConstraint, widthConstraint))
				{
					return new Size(Width, Height);
				}
				else if (TheSame(_previousHeightConstraint, _previousBounds.Height) &&
					TheSame(_previousWidthConstraint, _previousBounds.Width))
				{
					return new Size(Width, Height);
				}
			}

			_previousWidthConstraint = widthConstraint;
			_previousHeightConstraint = heightConstraint;
			return base.MeasureOverride(widthConstraint, heightConstraint);

			bool TheSame(double width, double otherWidth)
			{
				return width == otherWidth ||
					(width - otherWidth) < double.Epsilon;
			}
		}
	}
}
