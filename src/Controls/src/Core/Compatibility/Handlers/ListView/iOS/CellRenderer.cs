using System;
using System.ComponentModel;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
#pragma warning disable CS0618 // Type or member is obsolete
	public class CellRenderer : ElementHandler<Cell, UITableViewCell>, IRegisterable
#pragma warning restore CS0618 // Type or member is obsolete
	{
#pragma warning disable CS0618 // Type or member is obsolete
		static readonly BindableProperty RealCellProperty = BindableProperty.CreateAttached("RealCell", typeof(UITableViewCell), typeof(Cell), null);
#pragma warning restore CS0618 // Type or member is obsolete

		readonly UIColor _defaultCellBgColor = (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsTvOSVersionAtLeast(13)) ? UIColor.Clear : UIColor.White;

#pragma warning disable CS0618 // Type or member is obsolete
		public static PropertyMapper<Cell, CellRenderer> Mapper =
				new PropertyMapper<Cell, CellRenderer>(ElementHandler.ElementMapper);
#pragma warning restore CS0618 // Type or member is obsolete

#pragma warning disable CS0618 // Type or member is obsolete
		public static CommandMapper<Cell, CellRenderer> CommandMapper =
			new CommandMapper<Cell, CellRenderer>(ElementHandler.ElementCommandMapper);
#pragma warning restore CS0618 // Type or member is obsolete
		WeakReference<UITableView>? _tableView;

		private protected event PropertyChangedEventHandler? CellPropertyChanged;

		public CellRenderer() : base(Mapper, CommandMapper)
		{
		}

		protected override UITableViewCell CreatePlatformElement()
		{
			var reusableCell = VirtualView.ReusableCell;
			var tv = VirtualView.TableView;
			VirtualView.ReusableCell = null;
			VirtualView.TableView = null;
			_tableView = new(tv);
			return GetCell(VirtualView, reusableCell, tv);
		}

#pragma warning disable CS0618 // Type or member is obsolete
		public virtual UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			var tvc = reusableCell as CellTableViewCell ?? new CellTableViewCell(UITableViewCellStyle.Default, item.GetType().FullName);

			tvc.Cell = item;

			if (OperatingSystem.IsIOSVersionAtLeast(14))
			{
				var content = tvc.DefaultContentConfiguration;
				content.Text = item.ToString();
			}
			else
			{
				tvc.TextLabel.Text = item.ToString();
			}

			UpdateBackground(tvc, item);

			SetAccessibility(tvc, item);

			return tvc;
		}

#pragma warning disable CS0618 // Type or member is obsolete
		public virtual void SetAccessibility(UITableViewCell tableViewCell, Cell cell)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			if (cell.IsSet(AutomationProperties.IsInAccessibleTreeProperty))
				tableViewCell.IsAccessibilityElement = cell.GetValue(AutomationProperties.IsInAccessibleTreeProperty).Equals(true);
			else
				tableViewCell.IsAccessibilityElement = false;

			if (cell.IsSet(AutomationProperties.ExcludedWithChildrenProperty))
				tableViewCell.AccessibilityElementsHidden = cell.GetValue(AutomationProperties.ExcludedWithChildrenProperty).Equals(true);
			else
				tableViewCell.AccessibilityElementsHidden = false;

#pragma warning disable CS0618 // Type or member is obsolete
			if (cell.IsSet(AutomationProperties.NameProperty))
				tableViewCell.AccessibilityLabel = cell.GetValue(AutomationProperties.NameProperty).ToString();
			else
				tableViewCell.AccessibilityLabel = null;

			if (cell.IsSet(AutomationProperties.HelpTextProperty))
				tableViewCell.AccessibilityHint = cell.GetValue(AutomationProperties.HelpTextProperty).ToString();
			else
				tableViewCell.AccessibilityHint = null;
#pragma warning restore CS0618 // Type or member is obsolete

		}

#pragma warning disable CS0618 // Type or member is obsolete
		public virtual void SetBackgroundColor(UITableViewCell tableViewCell, Cell cell, UIColor color)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			if (OperatingSystem.IsIOSVersionAtLeast(14))
			{
				var content = tableViewCell.DefaultContentConfiguration;
				content.TextProperties.Color = color;
			}
			else
			{
				tableViewCell.TextLabel.BackgroundColor = color;
			}

			tableViewCell.ContentView.BackgroundColor = color;
			tableViewCell.BackgroundColor = color;
		}

#pragma warning disable CS0618 // Type or member is obsolete
		protected void UpdateBackground(UITableViewCell tableViewCell, Cell cell)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			var uiBgColor = UITableView.Appearance.BackgroundColor ?? _defaultCellBgColor;

#if __MOBILE__
#pragma warning disable CS0618 // Type or member is obsolete
			var defaultBgColor = cell.On<PlatformConfiguration.iOS>().DefaultBackgroundColor();
#pragma warning restore CS0618 // Type or member is obsolete
#else
			var defaultBgColor = cell.On<PlatformConfiguration.macOS>().DefaultBackgroundColor();
#endif
			if (defaultBgColor != null)
			{
				uiBgColor = defaultBgColor.ToPlatform();
			}
			else
			{
#pragma warning disable CS0618 // Type or member is obsolete
				if (cell.GetIsGroupHeader<ItemsView<Cell>, Cell>())
				{
					uiBgColor = Microsoft.Maui.Platform.ColorExtensions.GroupedBackground;
				}
				else
				{
					if (cell.RealParent is VisualElement element && element.BackgroundColor != null)
						uiBgColor = element.BackgroundColor.ToPlatform();
				}
#pragma warning restore CS0618 // Type or member is obsolete
			}

			SetBackgroundColor(tableViewCell, cell, uiBgColor);
		}

		[Obsolete("The ForceUpdateSizeRequested event is now managed by the command mapper, so it's not necessary to invoke this event manually.")]
		protected void WireUpForceUpdateSizeRequested(ICellController cell, UITableViewCell platformCell, UITableView tableView)
		{

		}

		void Ncp_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine($"{e.PropertyName}");
		}

		internal static UITableViewCell GetRealCell(BindableObject cell)
		{
			return (UITableViewCell)cell.GetValue(RealCellProperty);
		}

		internal static void SetRealCell(BindableObject cell, UITableViewCell renderer)
		{
			cell.SetValue(RealCellProperty, renderer);
		}

		public override void UpdateValue(string property)
		{
			base.UpdateValue(property);
			var args = new PropertyChangedEventArgs(property);
			if (VirtualView is BindableObject bindableObject &&
				GetRealCell(bindableObject) is CellTableViewCell ctv)
			{
				ctv.HandlePropertyChanged(bindableObject, args);
			}

			CellPropertyChanged?.Invoke(VirtualView, args);
		}

		public override void Invoke(string command, object? args)
		{
			base.Invoke(command, args);

			if (command == "ForceUpdateSizeRequested" &&
				VirtualView is BindableObject bindableObject &&
				GetRealCell(bindableObject) is UITableViewCell ctv &&
				_tableView is not null &&
				_tableView.TryGetTarget(out var tableView))
			{
				var index = tableView.IndexPathForCell(ctv);
#pragma warning disable CS0618 // Type or member is obsolete
				if (index == null && VirtualView is Cell c)
				{
					index = Controls.Compatibility.Platform.iOS.CellExtensions.GetIndexPath(c);
				}
#pragma warning restore CS0618 // Type or member is obsolete
				if (index != null)
					tableView.ReloadRows(new[] { index }, UITableViewRowAnimation.None);
			}
		}
	}
}