#nullable enable

using System;
using System.ComponentModel;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public class CellRenderer : ElementHandler<Cell, UITableViewCell>, IRegisterable
	{
		static readonly BindableProperty RealCellProperty = BindableProperty.CreateAttached("RealCell", typeof(UITableViewCell), typeof(Cell), null);

		EventHandler? _onForceUpdateSizeRequested;
		PropertyChangedEventHandler? _onPropertyChangedEventHandler;
		readonly UIColor _defaultCellBgColor = (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsTvOSVersionAtLeast(13)) ? UIColor.Clear : UIColor.White;

		public static PropertyMapper<Cell, CellRenderer> Mapper =
				new PropertyMapper<Cell, CellRenderer>(ElementHandler.ElementMapper);

		public static CommandMapper<Cell, CellRenderer> CommandMapper =
			new CommandMapper<Cell, CellRenderer>(ElementHandler.ElementCommandMapper);
		UITableView? _tableView;

		public CellRenderer() : base(Mapper, CommandMapper)
		{
		}

		protected override UITableViewCell CreatePlatformElement()
		{
			var reusableCell = VirtualView.ReusableCell;
			var tv = VirtualView.TableView;
			VirtualView.ReusableCell = null;
			VirtualView.TableView = null;
			return GetCell(VirtualView, reusableCell, tv);
		}

		public virtual UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
		{
			_tableView = tv;
			Performance.Start(out string reference);

			var tvc = reusableCell as CellTableViewCell ?? new CellTableViewCell(UITableViewCellStyle.Default, item.GetType().FullName);

			tvc.Cell = item;

			WireUpForceUpdateSizeRequested(item, tvc, tv);

			if(OperatingSystem.IsIOSVersionAtLeast(14))
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

			Performance.Stop(reference);
			return tvc;
		}

		public virtual void SetAccessibility(UITableViewCell tableViewCell, Cell cell)
		{
			if (cell.IsSet(AutomationProperties.IsInAccessibleTreeProperty))
				tableViewCell.IsAccessibilityElement = cell.GetValue(AutomationProperties.IsInAccessibleTreeProperty).Equals(true);
			else
				tableViewCell.IsAccessibilityElement = false;

			if (cell.IsSet(AutomationProperties.ExcludedWithChildrenProperty))
				tableViewCell.AccessibilityElementsHidden = cell.GetValue(AutomationProperties.ExcludedWithChildrenProperty).Equals(true);
			else
				tableViewCell.AccessibilityElementsHidden = false;

			if (cell.IsSet(AutomationProperties.NameProperty))
				tableViewCell.AccessibilityLabel = cell.GetValue(AutomationProperties.NameProperty).ToString();
			else
				tableViewCell.AccessibilityLabel = null;

			if (cell.IsSet(AutomationProperties.HelpTextProperty))
				tableViewCell.AccessibilityHint = cell.GetValue(AutomationProperties.HelpTextProperty).ToString();
			else
				tableViewCell.AccessibilityHint = null;
		}

		public virtual void SetBackgroundColor(UITableViewCell tableViewCell, Cell cell, UIColor color)
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

		protected void UpdateBackground(UITableViewCell tableViewCell, Cell cell)
		{
			var uiBgColor = UITableView.Appearance.BackgroundColor ?? _defaultCellBgColor;

#if __MOBILE__
			var defaultBgColor = cell.On<PlatformConfiguration.iOS>().DefaultBackgroundColor();
#else
			var defaultBgColor = cell.On<PlatformConfiguration.macOS>().DefaultBackgroundColor();
#endif
			if (defaultBgColor != null)
			{
				uiBgColor = defaultBgColor.ToPlatform();
			}
			else
			{
				if (cell.GetIsGroupHeader<ItemsView<Cell>, Cell>())
				{
					uiBgColor = Microsoft.Maui.Platform.ColorExtensions.GroupedBackground;
				}
				else
				{
					if (cell.RealParent is VisualElement element && element.BackgroundColor != null)
						uiBgColor = element.BackgroundColor.ToPlatform();
				}
			}

			SetBackgroundColor(tableViewCell, cell, uiBgColor);
		}

		protected void WireUpForceUpdateSizeRequested(ICellController cell, UITableViewCell platformCell, UITableView tableView)
		{
			var inpc = cell as INotifyPropertyChanged;
			cell.ForceUpdateSizeRequested -= _onForceUpdateSizeRequested;

			if (inpc != null)
				inpc.PropertyChanged -= _onPropertyChangedEventHandler;

			_onForceUpdateSizeRequested = (sender, e) =>
			{
				var index = tableView?.IndexPathForCell(platformCell);
				if (index == null && sender is Cell c)
				{
					index = Controls.Compatibility.Platform.iOS.CellExtensions.GetIndexPath(c);
				}

				if (index != null)
					tableView?.ReloadRows(new[] { index }, UITableViewRowAnimation.None);
			};

			_onPropertyChangedEventHandler = (sender, e) =>
			{
				if (e.PropertyName == "RealCell" && sender is BindableObject bo && GetRealCell(bo) == null)
				{
					if (sender is ICellController icc)
						icc.ForceUpdateSizeRequested -= _onForceUpdateSizeRequested;

					if (sender is INotifyPropertyChanged notifyPropertyChanged)
						notifyPropertyChanged.PropertyChanged -= _onPropertyChangedEventHandler;

					_onForceUpdateSizeRequested = null;
					_onPropertyChangedEventHandler = null;
				}
			};

			cell.ForceUpdateSizeRequested += _onForceUpdateSizeRequested;
			if (inpc != null)
				inpc.PropertyChanged += _onPropertyChangedEventHandler;

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
	}
}
