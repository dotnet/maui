using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiSearchBar : UISearchBar, IUIViewLifeCycleEvents
	{
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Proven safe: delegate is cleared in WillMoveToWindow(null) before view is released")]
		SearchEditorDelegate? _searchEditorDelegate;

		public MauiSearchBar() : this(RectangleF.Empty)
		{
		}

		public MauiSearchBar(NSCoder coder) : base(coder)
		{
		}

		public MauiSearchBar(CGRect frame) : base(frame)
		{
		}

		protected MauiSearchBar(NSObjectFlag t) : base(t)
		{
		}

		protected internal MauiSearchBar(NativeHandle handle) : base(handle)
		{
		}

		// Native Changed doesn't fire when the Text Property is set in code
		// We use this event as a way to fire changes whenever the Text changes
		// via code or user interaction.
		[UnconditionalSuppressMessage("Memory", "MEM0001", Justification = "Proven safe in test: MemoryTests.HandlerDoesNotLeak")]
		public event EventHandler<UISearchBarTextChangedEventArgs>? TextSetOrChanged;

		public override string? Text
		{
			get => base.Text;
			set
			{
				var old = base.Text;

				base.Text = value;

				if (old != value)
				{
					TextSetOrChanged?.Invoke(this, new UISearchBarTextChangedEventArgs(value ?? String.Empty));
				}
			}
		}

		[UnconditionalSuppressMessage("Memory", "MEM0001", Justification = "Proven safe in test: MemoryTests.HandlerDoesNotLeak")]
		internal event EventHandler? OnMovedToWindow;
		[UnconditionalSuppressMessage("Memory", "MEM0001", Justification = "Proven safe in test: MemoryTests.HandlerDoesNotLeak")]
		internal event EventHandler? EditingChanged;
		// Fires whenever the cursor position or text selection changes in the search field,
		// including user-initiated cursor repositioning (tap, long-press) and keyboard navigation.
		// Mirrors the MauiTextField.SelectionChanged pattern for Entry.
		[UnconditionalSuppressMessage("Memory", "MEM0001", Justification = "Proven safe in test: MemoryTests.HandlerDoesNotLeak")]
		internal event EventHandler? SelectionChanged;

		public override void WillMoveToWindow(UIWindow? window)
		{
			var editor = this.GetSearchTextField();

			base.WillMoveToWindow(window);

			if (editor != null)
			{
				editor.EditingChanged -= OnEditingChanged;
				editor.Delegate = null!; // UITextField.Delegate binding is non-nullable, but the underlying ObjC property accepts nil to clear the delegate
				_searchEditorDelegate = null;

				if (window != null)
				{
					editor.EditingChanged += OnEditingChanged;
					// UISearchBar manages its behavior via UISearchBarDelegate (separate hierarchy from
					// UITextFieldDelegate). Setting our own delegate on the internal editor is safe;
					// we only observe DidChangeSelection without interfering with search bar behavior.
					_searchEditorDelegate = new SearchEditorDelegate(this);
					editor.Delegate = _searchEditorDelegate;
				}
			}

			if (window != null)
				OnMovedToWindow?.Invoke(this, EventArgs.Empty);
		}

		[UnconditionalSuppressMessage("Memory", "MEM0003", Justification = "Proven safe in test: MemoryTests.HandlerDoesNotLeak")]
		void OnEditingChanged(object? sender, EventArgs e)
		{
			EditingChanged?.Invoke(this, EventArgs.Empty);
		}

		// Delegate that detects selection/cursor changes on the internal UITextField.
		// UITextFieldDelegate.DidChangeSelection (iOS 13+) fires for tap-to-reposition,
		// long-press selection, keyboard arrow navigation, and text input.
		class SearchEditorDelegate : UITextFieldDelegate
		{
			readonly WeakReference<MauiSearchBar> _owner;

			public SearchEditorDelegate(MauiSearchBar owner)
			{
				_owner = new(owner);
			}

			public override void DidChangeSelection(UITextField textField)
			{
				if (_owner.TryGetTarget(out var owner))
				{
					owner.SelectionChanged?.Invoke(owner, EventArgs.Empty);
				}
			}
		}

		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = IUIViewLifeCycleEvents.UnconditionalSuppressMessage)]
		EventHandler? _movedToWindow;
		event EventHandler IUIViewLifeCycleEvents.MovedToWindow
		{
			add => _movedToWindow += value;
			remove => _movedToWindow -= value;
		}

		public override void MovedToWindow()
		{
			base.MovedToWindow();
			_movedToWindow?.Invoke(this, EventArgs.Empty);
		}
	}
}