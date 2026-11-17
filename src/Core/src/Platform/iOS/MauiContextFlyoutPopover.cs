#if IOS || MACCATALYST
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	[SupportedOSPlatform("maccatalyst16.0")]
	[SupportedOSPlatform("ios16.0")]
	sealed class MauiContextFlyoutPopover : NSObject, IUIInteraction
	{
		readonly WeakReference<IViewHandler> _handler;
		WeakReference<UIView>? _view;
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Presentation resources are released on dismissal, flyout changes, unload, or disconnect.")]
		UINavigationController? _navigation;
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "UIKit holds the delegate weakly; it is released on dismissal.")]
		PopoverDelegate? _delegate;
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "The unload observer is disposed when the interaction is removed.")]
		IDisposable? _unloaded;
		bool _closing;

		MauiContextFlyoutPopover(IViewHandler handler) => _handler = new(handler);

		public UIView? View => _view is not null && _view.TryGetTarget(out var view) ? view : null;

		// Only the protocol implementation exports "view". UIKit permits null when detached,
		// despite the managed protocol's non-nullable return type.
		UIView IUIInteraction.View => View!;

		[Export("willMoveToView:")]
		public void WillMoveToView(UIView? view)
		{
		}

		[Export("didMoveToView:")]
		public void DidMoveToView(UIView? view)
		{
			_view = view is null ? null : new(view);
			if (view is null)
				Close();
		}

		internal static void Show(IViewHandler handler, IView view)
		{
			if (!NSThread.IsMain)
				throw new InvalidOperationException("Context flyouts must be presented on the UI thread.");

			if (handler.VirtualView != view || handler.PlatformView is not UIView target ||
				target.Window is null || target.Hidden || !view.IsEnabled ||
				view.InputTransparent || view.Visibility != Visibility.Visible)
				throw new InvalidOperationException("The context-flyout target must be loaded, visible, enabled, and accept input.");

			if (view is not IContextFlyoutElement { ContextFlyout: IMenuFlyout { Count: > 0 } flyout })
				throw new InvalidOperationException("A nonempty menu flyout must be attached to the target.");

			foreach (var interaction in target.Interactions)
			{
				if (interaction is MauiContextFlyoutPopover)
					return;
			}

			var context = handler.MauiContext
				?? throw new InvalidOperationException("The target handler must have a MauiContext.");
			var controller = target.Window.RootViewController
				?? throw new InvalidOperationException("The target window must have a root view controller.");
			while (controller.PresentedViewController is { } presented)
				controller = presented;

			if (controller.IsBeingPresented || controller.IsBeingDismissed || controller.ModalPresentationStyle == UIModalPresentationStyle.Popover)
				throw new InvalidOperationException("A context flyout cannot be presented while another popover or presentation transition is active.");

			var sections = CreateSections(flyout, context);
			if (sections.Count == 0)
				throw new InvalidOperationException("The attached context flyout must contain an action, not only separators.");

			var presenter = new MauiContextFlyoutPopover(handler);
			var root = new MenuController(presenter, sections, null);
			var navigation = new UINavigationController(root)
			{
				ModalPresentationStyle = UIModalPresentationStyle.Popover,
				PreferredContentSize = new CGSize(300, 480)
			};
			navigation.NavigationBar.Translucent = false;
			using (var appearance = new UINavigationBarAppearance())
			{
				appearance.ConfigureWithOpaqueBackground();
				appearance.BackgroundColor = UIColor.SystemGroupedBackground;
				appearance.ShadowColor = UIColor.Clear;
				navigation.NavigationBar.StandardAppearance = appearance;
				navigation.NavigationBar.ScrollEdgeAppearance = appearance;
				navigation.NavigationBar.CompactAppearance = appearance;
				navigation.NavigationBar.CompactScrollEdgeAppearance = appearance;
			}
			var popover = navigation.PopoverPresentationController
				?? throw new InvalidOperationException("UIKit could not create the context-flyout popover.");
			presenter._navigation = navigation;
			presenter._delegate = new PopoverDelegate(presenter);
			popover.Delegate = presenter._delegate;
			popover.BackgroundColor = UIColor.SystemGroupedBackground;
			popover.SourceView = target;
			popover.SourceRect = target.Bounds;
			popover.PermittedArrowDirections = UIPopoverArrowDirection.Any;
			// The button requesting presentation must not also receive an outside-dismissal click.
			popover.PassthroughViews = Array.Empty<UIView>();
			target.AddInteraction(presenter);
			presenter._unloaded = view.OnUnloaded(presenter.Remove);

			controller.PresentViewController(navigation, false, null);
		}

		static List<List<MenuEntry>> CreateSections(IList<IMenuElement> elements, IMauiContext context)
		{
			var sections = new List<List<MenuEntry>>();
			List<MenuEntry>? section = null;
			for (int i = 0; i < elements.Count; i++)
			{
				var element = elements[i];
				if (element is IMenuFlyoutSeparator)
				{
					section = null;
					continue;
				}

				if (section is null)
				{
					section = new List<MenuEntry>();
					sections.Add(section);
				}

				section.Add(new MenuEntry(element, element.Source.GetPlatformMenuImage(context),
					element is IMenuFlyoutSubItem submenu ? CreateSections(submenu, context) : null));
			}
			return sections;
		}

		internal static void Remove(UIView view)
		{
			foreach (var interaction in view.Interactions)
			{
				if (interaction is MauiContextFlyoutPopover presenter)
					presenter.Remove();
			}
		}

		void Remove() => Close();

		void Close(Action? selected = null, bool dismiss = true)
		{
			if (_closing)
				return;

			_closing = true;
			_unloaded?.Dispose();
			_unloaded = null;
			var target = _view;
			var navigation = _navigation;
			_navigation = null;
			if (navigation?.PopoverPresentationController is { } popover)
				popover.Delegate = null;
			_delegate?.Dispose();
			_delegate = null;
			View?.RemoveInteraction(this);

			void Complete()
			{
				if (navigation is not null)
				{
					if (navigation.ViewControllers is { } pages)
					{
						foreach (var page in pages)
							page.Dispose();
					}
					navigation.Dispose();
				}
				selected?.Invoke();
				if (target is not null && target.TryGetTarget(out var view) && view.Window is not null)
					UIAccessibility.PostNotification(UIAccessibilityPostNotification.ScreenChanged, view);
			}

			if (dismiss && navigation?.PresentingViewController is not null)
				navigation.DismissViewController(false, Complete);
			else
				Complete();
			Dispose();
		}

		void Activate(MenuEntry entry, MenuController page)
		{
			if (_closing || !_handler.TryGetTarget(out var handler) ||
				handler.PlatformView != View ||
				handler.VirtualView is not IView { IsEnabled: true, InputTransparent: false, Visibility: Visibility.Visible } ||
				View?.Window is null || !entry.IsEnabled)
				return;

			if (entry.Children is { } children)
			{
				page.NavigationController?.PushViewController(new MenuController(this, children, entry.Text), false);
			}
			else
			{
				var target = View;
				var virtualView = handler.VirtualView;
				Close(() =>
				{
					if (handler.PlatformView == target && handler.VirtualView == virtualView &&
						target?.Window is not null &&
						virtualView is IView { IsEnabled: true, InputTransparent: false, Visibility: Visibility.Visible } &&
						entry.IsEnabled)
						entry.Element.Clicked();
				});
			}

		}

		sealed class MenuEntry
		{
			internal readonly IMenuElement Element;
			internal readonly string Text;
			internal readonly UIImage? Image;
			internal readonly List<List<MenuEntry>>? Children;
			readonly bool _enabled;

			internal MenuEntry(IMenuElement element, UIImage? image, List<List<MenuEntry>>? children)
			{
				Element = element;
				Text = element.Text ?? string.Empty;
				Image = image;
				Children = children;
				_enabled = element.IsEnabled && (children is null || children.Count > 0);
			}

			internal bool IsEnabled => _enabled && Element.IsEnabled;
		}

		sealed class PopoverDelegate : UIPopoverPresentationControllerDelegate
		{
			readonly WeakReference<MauiContextFlyoutPopover> _owner;

			internal PopoverDelegate(MauiContextFlyoutPopover owner) => _owner = new(owner);

			public override UIModalPresentationStyle GetAdaptivePresentationStyle(UIPresentationController controller) =>
				UIModalPresentationStyle.None;

			public override UIModalPresentationStyle GetAdaptivePresentationStyle(UIPresentationController controller, UITraitCollection traitCollection) =>
				UIModalPresentationStyle.None;

			public override void DidDismissPopover(UIPopoverPresentationController popoverPresentationController)
			{
				if (_owner.TryGetTarget(out var owner))
					owner.Close(dismiss: false);
			}

			public override void DidDismiss(UIPresentationController presentationController)
			{
				if (_owner.TryGetTarget(out var owner))
					owner.Close(dismiss: false);
			}
		}

		sealed class MenuController : UITableViewController
		{
			readonly WeakReference<MauiContextFlyoutPopover> _owner;
			readonly List<List<MenuEntry>> _sections;
			bool _needsSizeUpdate = true;
			double _measuredWidth;
			double _measuredBarHeight;
			[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Key commands are disposed with the menu controller.")]
			UIKeyCommand[]? _commands;

			internal MenuController(MauiContextFlyoutPopover owner, List<List<MenuEntry>> sections, string? title)
				: base(UITableViewStyle.InsetGrouped)
			{
				_owner = new(owner);
				_sections = sections;
				Title = title;
				EdgesForExtendedLayout = UIRectEdge.None;
				var weakOwner = _owner;
				NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Close,
					(_, _) => { if (weakOwner.TryGetTarget(out var presenter)) presenter.Close(); });
			}

			public override bool CanBecomeFirstResponder => true;

			public override void ViewDidLoad()
			{
				base.ViewDidLoad();
				TableView.RowHeight = UITableView.AutomaticDimension;
				TableView.EstimatedRowHeight = 44;
				TableView.BackgroundColor = UIColor.SystemGroupedBackground;
				TableView.ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Automatic;
			}

			public override UIKeyCommand[] KeyCommands
			{
				get
				{
					if (_commands is not null)
						return _commands;

					_commands = new[]
					{
						UIKeyCommand.Create(UIKeyCommand.UpArrow, 0, new Selector("contextFlyoutUp")),
						UIKeyCommand.Create(UIKeyCommand.DownArrow, 0, new Selector("contextFlyoutDown")),
						UIKeyCommand.Create(UIKeyCommand.RightArrow, 0, new Selector("contextFlyoutOpen")),
						UIKeyCommand.Create(UIKeyCommand.LeftArrow, 0, new Selector("contextFlyoutBack")),
						UIKeyCommand.Create(UIKeyCommand.Escape, 0, new Selector("contextFlyoutBack")),
						UIKeyCommand.Create((NSString)"\r", 0, new Selector("contextFlyoutActivate"))
					};
					foreach (var command in _commands)
						command.WantsPriorityOverSystemBehavior = true;
					return _commands;
				}
			}

			public override void ViewWillAppear(bool animated)
			{
				base.ViewWillAppear(animated);
				_needsSizeUpdate = true;
				View?.SetNeedsLayout();
			}

			public override void ViewDidLayoutSubviews()
			{
				base.ViewDidLayoutSubviews();
				if (NavigationController is not { } navigation ||
					navigation.TopViewController != this || TableView.Bounds.Width <= 0)
					return;

				var width = (double)TableView.Bounds.Width;
				var barHeight = (double)navigation.NavigationBar.Bounds.Height;
				if (!_needsSizeUpdate && Math.Abs(width - _measuredWidth) < 0.5 &&
					Math.Abs(barHeight - _measuredBarHeight) < 0.5)
					return;

				// Self-sizing estimates change during scrolling. They must not continuously resize the popover.
				_needsSizeUpdate = false;
				_measuredWidth = width;
				_measuredBarHeight = barHeight;
				var contentHeight = Math.Ceiling((double)TableView.ContentSize.Height +
					(double)TableView.AdjustedContentInset.Top + (double)TableView.AdjustedContentInset.Bottom);
				var height = Math.Min(contentHeight + barHeight, 480);
				var popoverSize = new CGSize(300, height);
				// Only the navigation controller owns the requested size; child-size propagation can feed back into layout.
				if (navigation.PreferredContentSize != popoverSize)
					navigation.PreferredContentSize = popoverSize;
			}

			public override void TraitCollectionDidChange(UITraitCollection? previousTraitCollection)
			{
#pragma warning disable CA1422 // Trait-change fallback also supports iOS/Mac Catalyst 16.
				base.TraitCollectionDidChange(previousTraitCollection);
#pragma warning restore CA1422
				if (previousTraitCollection is not null &&
					previousTraitCollection.PreferredContentSizeCategory != TraitCollection.PreferredContentSizeCategory)
				{
					_needsSizeUpdate = true;
					View?.SetNeedsLayout();
				}
			}

			public override void ViewDidAppear(bool animated)
			{
				base.ViewDidAppear(animated);
				BecomeFirstResponder();
			}

			public override nint NumberOfSections(UITableView tableView) => _sections.Count;

			public override nint RowsInSection(UITableView tableView, nint section) => _sections[(int)section].Count;

			MenuEntry GetEntry(NSIndexPath path) => _sections[(int)path.Section][(int)path.Row];

			public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
			{
				var entry = GetEntry(indexPath);
				var cell = tableView.DequeueReusableCell("ContextFlyoutItem")
					?? new UITableViewCell(UITableViewCellStyle.Default, "ContextFlyoutItem");
				var content = UIListContentConfiguration.CellConfiguration;
				content.Text = entry.Text;
				content.Image = entry.Image;
				// Full-resolution file images must not determine self-sizing menu row dimensions.
				content.ImageProperties.MaximumSize = new CGSize(24, 24);
				content.ImageProperties.ReservedLayoutSize = new CGSize(24, 24);
				content.TextProperties.Color = entry.IsEnabled ? UIColor.Label : UIColor.SecondaryLabel;
				content.TextProperties.NumberOfLines = 0;
				cell.ContentConfiguration = content;
				cell.Accessory = entry.Children is null ? UITableViewCellAccessory.None : UITableViewCellAccessory.DisclosureIndicator;
				cell.UserInteractionEnabled = entry.IsEnabled;
				cell.AccessibilityLabel = entry.Text;
				cell.AccessibilityTraits = UIAccessibilityTrait.Button | (entry.IsEnabled ? 0 : UIAccessibilityTrait.NotEnabled);
				return cell;
			}

			public override bool CanFocusRow(UITableView tableView, NSIndexPath indexPath) => GetEntry(indexPath).IsEnabled;

			public override bool ShouldHighlightRow(UITableView tableView, NSIndexPath rowIndexPath) => GetEntry(rowIndexPath).IsEnabled;

			public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
			{
				tableView.DeselectRow(indexPath, false);
				if (_owner.TryGetTarget(out var owner))
					owner.Activate(GetEntry(indexPath), this);
			}

			[Export("contextFlyoutUp")]
			void MoveUp() => MoveSelection(-1);

			[Export("contextFlyoutDown")]
			void MoveDown() => MoveSelection(1);

			void MoveSelection(int direction)
			{
				var current = TableView.IndexPathForSelectedRow;
				int section = current is null ? (direction > 0 ? 0 : _sections.Count - 1) : (int)current.Section;
				int row = current is null ? (direction > 0 ? -1 : _sections[section].Count) : (int)current.Row;
				while (section >= 0 && section < _sections.Count)
				{
					row += direction;
					if (row < 0 || row >= _sections[section].Count)
					{
						section += direction;
						if (section >= 0 && section < _sections.Count)
							row = direction > 0 ? -1 : _sections[section].Count;
						continue;
					}
					if (_sections[section][row].IsEnabled)
					{
						TableView.SelectRow(NSIndexPath.FromRowSection(row, section), false, UITableViewScrollPosition.Middle);
						return;
					}
				}
			}

			[Export("contextFlyoutActivate")]
			void ActivateSelected()
			{
				if (TableView.IndexPathForSelectedRow is { } path && _owner.TryGetTarget(out var owner))
					owner.Activate(GetEntry(path), this);
			}

			[Export("contextFlyoutOpen")]
			void OpenSelectedSubmenu()
			{
				if (TableView.IndexPathForSelectedRow is { } path && GetEntry(path).Children is not null)
					ActivateSelected();
			}

			[Export("contextFlyoutBack")]
			void GoBack()
			{
				if (NavigationController is { ViewControllers.Length: > 1 } navigation)
					navigation.PopViewController(false);
				else if (_owner.TryGetTarget(out var owner))
					owner.Close();
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					if (_commands is not null)
					{
						foreach (var command in _commands)
							command.Dispose();
						_commands = null;
					}
					NavigationItem.RightBarButtonItem?.Dispose();
				}
				base.Dispose(disposing);
			}
		}
	}
}
#endif
