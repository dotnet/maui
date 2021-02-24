using System;
using System.ComponentModel;
using UIKit;
using Microsoft.Maui.Controls.Compatibility.Internals;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	public class CellRenderer : IRegisterable
	{
		static readonly BindableProperty RealCellProperty = BindableProperty.CreateAttached("RealCell", typeof(UITableViewCell), typeof(Cell), null);

		EventHandler _onForceUpdateSizeRequested;
		PropertyChangedEventHandler _onPropertyChangedEventHandler;
		readonly UIColor _defaultCellBgColor = Forms.IsiOS13OrNewer ? UIColor.Clear : UIColor.White;

		[Preserve(Conditional = true)]
		public CellRenderer()
		{
		}

		public virtual UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
		{
			Performance.Start(out string reference);

			var tvc = reusableCell as CellTableViewCell ?? new CellTableViewCell(UITableViewCellStyle.Default, item.GetType().FullName);

			tvc.Cell = item;

			WireUpForceUpdateSizeRequested(item, tvc, tv);

			tvc.TextLabel.Text = item.ToString();

			UpdateBackground(tvc, item);

			SetAccessibility (tvc, item);

			Performance.Stop(reference);
			return tvc;
		}

		public virtual void SetAccessibility (UITableViewCell tableViewCell, Cell cell)
		{
			if (cell.IsSet (AutomationProperties.IsInAccessibleTreeProperty))
				tableViewCell.IsAccessibilityElement = cell.GetValue (AutomationProperties.IsInAccessibleTreeProperty).Equals (true);
			else
				tableViewCell.IsAccessibilityElement = false;

			if (cell.IsSet (AutomationProperties.NameProperty))
				tableViewCell.AccessibilityLabel = cell.GetValue (AutomationProperties.NameProperty).ToString ();
			else
				tableViewCell.AccessibilityLabel = null;

			if (cell.IsSet (AutomationProperties.HelpTextProperty))
				tableViewCell.AccessibilityHint = cell.GetValue (AutomationProperties.HelpTextProperty).ToString ();
			else
				tableViewCell.AccessibilityHint = null;
		}

		public virtual void SetBackgroundColor(UITableViewCell tableViewCell, Cell cell, UIColor color)
		{
			tableViewCell.TextLabel.BackgroundColor = color;
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
			if (defaultBgColor != Color.Default)
			{
				uiBgColor = defaultBgColor.ToUIColor();
			}
			else
			{
				if (cell.GetIsGroupHeader<ItemsView<Cell>, Cell>())
				{
					if (!UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
						return;

					uiBgColor = ColorExtensions.GroupedBackground;
				}
				else
				{
					if (cell.RealParent is VisualElement element && element.BackgroundColor != Color.Default)
						uiBgColor = element.BackgroundColor.ToUIColor();
				}
			}

			SetBackgroundColor(tableViewCell, cell, uiBgColor);
		}

		protected void WireUpForceUpdateSizeRequested(ICellController cell, UITableViewCell nativeCell, UITableView tableView)
		{
			var inpc = cell as INotifyPropertyChanged;
			cell.ForceUpdateSizeRequested -= _onForceUpdateSizeRequested;

			if (inpc != null)
				inpc.PropertyChanged -= _onPropertyChangedEventHandler;

			_onForceUpdateSizeRequested = (sender, e) =>
			{
				var index = tableView?.IndexPathForCell(nativeCell) ?? (sender as Cell)?.GetIndexPath();
				if (index != null)
					tableView.ReloadRows(new[] { index }, UITableViewRowAnimation.None);
			};

			_onPropertyChangedEventHandler = (sender, e) =>
			{
				if(e.PropertyName == "RealCell" && sender is BindableObject bo && GetRealCell(bo) == null)
				{
					if(sender is ICellController icc)
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
