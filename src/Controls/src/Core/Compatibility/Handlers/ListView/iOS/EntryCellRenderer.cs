#nullable disable
using System;
using System.ComponentModel;
using System.Runtime.Versioning;
using Foundation;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using ObjCRuntime;
using UIKit;
using RectangleF = CoreGraphics.CGRect;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public class EntryCellRenderer : CellRenderer
	{
		static readonly Color DefaultTextColor = Microsoft.Maui.Platform.ColorExtensions.LabelColor.ToColor();

		[Preserve(Conditional = true)]
		public EntryCellRenderer()
		{
		}

		[UnsupportedOSPlatform("ios14.0")]
		[UnsupportedOSPlatform("tvos14.0")]
		public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
		{
			var entryCell = (EntryCell)item;

			var tvc = reusableCell as EntryCellTableViewCell;
			if (tvc == null)
			{
				tvc = new EntryCellTableViewCell(item.GetType().FullName);
			}
			else
			{
				CellPropertyChanged -= HandlePropertyChanged;
				tvc.TextFieldTextChanged -= OnTextFieldTextChanged;
				tvc.KeyboardDoneButtonPressed -= OnKeyBoardDoneButtonPressed;
			}

			SetRealCell(item, tvc);

			tvc.Cell = item;
			CellPropertyChanged += HandlePropertyChanged;
			tvc.TextFieldTextChanged += OnTextFieldTextChanged;
			tvc.KeyboardDoneButtonPressed += OnKeyBoardDoneButtonPressed;

			UpdateBackground(tvc, entryCell);
			UpdateLabel(tvc, entryCell);
			UpdateText(tvc, entryCell);
			UpdateKeyboard(tvc, entryCell);
			UpdatePlaceholder(tvc, entryCell);
			UpdateLabelColor(tvc, entryCell);
			UpdateHorizontalTextAlignment(tvc, entryCell);
			UpdateIsEnabled(tvc, entryCell);

			return tvc;
		}

		[UnsupportedOSPlatform("ios14.0")]
		[UnsupportedOSPlatform("tvos14.0")]
		static void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var entryCell = (EntryCell)sender;
			var realCell = (EntryCellTableViewCell)GetRealCell(entryCell);

			if (e.PropertyName == EntryCell.LabelProperty.PropertyName)
				UpdateLabel(realCell, entryCell);
			else if (e.PropertyName == EntryCell.TextProperty.PropertyName)
				UpdateText(realCell, entryCell);
			else if (e.PropertyName == EntryCell.PlaceholderProperty.PropertyName)
				UpdatePlaceholder(realCell, entryCell);
			else if (e.PropertyName == EntryCell.KeyboardProperty.PropertyName)
				UpdateKeyboard(realCell, entryCell);
			else if (e.PropertyName == EntryCell.LabelColorProperty.PropertyName)
				UpdateLabelColor(realCell, entryCell);
			else if (e.PropertyName == EntryCell.HorizontalTextAlignmentProperty.PropertyName)
				UpdateHorizontalTextAlignment(realCell, entryCell);
			else if (e.PropertyName == Cell.IsEnabledProperty.PropertyName)
				UpdateIsEnabled(realCell, entryCell);
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
				UpdateHorizontalTextAlignment(realCell, entryCell);
		}

		static void OnKeyBoardDoneButtonPressed(object sender, EventArgs e)
		{
			var cell = (EntryCellTableViewCell)sender;
			var model = (IEntryCellController)cell.Cell;

			model.SendCompleted();
		}

		static void OnTextFieldTextChanged(object sender, EventArgs eventArgs)
		{
			var cell = (EntryCellTableViewCell)sender;
			var model = (EntryCell)cell.Cell;

			model
				.SetValue(EntryCell.TextProperty, cell.TextField.Text, specificity: SetterSpecificity.FromHandler);
		}

		static void UpdateHorizontalTextAlignment(EntryCellTableViewCell cell, EntryCell entryCell)
		{
			cell.TextField.UpdateHorizontalTextAlignment(entryCell);
		}

		[UnsupportedOSPlatform("ios14.0")]
		[UnsupportedOSPlatform("tvos14.0")]
		static void UpdateIsEnabled(EntryCellTableViewCell cell, EntryCell entryCell)
		{
			cell.UserInteractionEnabled = entryCell.IsEnabled;
			cell.TextLabel.Enabled = entryCell.IsEnabled;
			cell.DetailTextLabel.Enabled = entryCell.IsEnabled;
			cell.TextField.Enabled = entryCell.IsEnabled;
		}

		static void UpdateKeyboard(EntryCellTableViewCell cell, EntryCell entryCell)
		{
			cell.TextField.ApplyKeyboard(entryCell.Keyboard);
		}

		[UnsupportedOSPlatform("ios14.0")]
		[UnsupportedOSPlatform("tvos14.0")]
		static void UpdateLabel(EntryCellTableViewCell cell, EntryCell entryCell)
		{
			cell.TextLabel.Text = entryCell.Label;
		}

		[UnsupportedOSPlatform("ios14.0")]
		[UnsupportedOSPlatform("tvos14.0")]
		static void UpdateLabelColor(EntryCellTableViewCell cell, EntryCell entryCell)
		{
			cell.TextLabel.TextColor = entryCell.LabelColor?.ToPlatform() ?? DefaultTextColor.ToPlatform();
		}

		static void UpdatePlaceholder(EntryCellTableViewCell cell, EntryCell entryCell)
		{
			cell.TextField.Placeholder = entryCell.Placeholder;
		}

		static void UpdateText(EntryCellTableViewCell cell, EntryCell entryCell)
		{
			if (cell.TextField.Text == entryCell.Text)
				return;
			// double sets side effect on iOS, YAY
			cell.TextField.Text = entryCell.Text;
		}

		public class EntryCellTableViewCell : CellTableViewCell
		{
			public EntryCellTableViewCell(string cellName) : base(UITableViewCellStyle.Value1, cellName)
			{
				TextField = new UITextField(new RectangleF(0, 0, 100, 30)) { BorderStyle = UITextBorderStyle.None };

				TextField.EditingChanged += TextFieldOnEditingChanged;
				TextField.ShouldReturn = OnShouldReturn;

				ContentView.AddSubview(TextField);
			}

			public UITextField TextField { get; }

			public event EventHandler KeyboardDoneButtonPressed;

			public override void LayoutSubviews()
			{
				base.LayoutSubviews();

#pragma warning disable CA1416, CA1422 // TODO:  'UITableViewCell.TextLabel' is unsupported on: 'ios' 14.0 and late
				// simple algorithm to generally line up entries
				var start = (nfloat)Math.Round(Math.Max(Frame.Width * 0.3, TextLabel.Frame.Right + 10));
				TextField.Frame = new RectangleF(start, (Frame.Height - 30) / 2, Frame.Width - TextLabel.Frame.Left - start, 30);
#pragma warning restore CA1416, CA1422
				// Centers TextField Content  (iOS6)
				TextField.VerticalAlignment = UIControlContentVerticalAlignment.Center;
			}

			public event EventHandler TextFieldTextChanged;

			static bool OnShouldReturn(UITextField view)
			{
				var realCell = GetRealCell<EntryCellTableViewCell>(view);
				var handler = realCell?.KeyboardDoneButtonPressed;
				if (handler != null)
					handler(realCell, EventArgs.Empty);

				KeyboardAutoManager.GoToNextResponderOrResign(view);
				return true;
			}

			static void TextFieldOnEditingChanged(object sender, EventArgs eventArgs)
			{
				var realCell = GetRealCell<EntryCellTableViewCell>(sender as UIView);
				var handler = realCell?.TextFieldTextChanged;
				if (handler != null)
					handler(realCell, EventArgs.Empty);
			}

			static T GetRealCell<T>(UIView view) where T : UIView
			{
				T realCell = null;
				while (view.Superview != null && realCell == null)
				{
					view = view.Superview;
					realCell = view as T;
				}
				return realCell;
			}
		}
	}
}