#nullable disable
using System.ComponentModel;
using Android.Content;
using Android.Text;
using Android.Text.Method;
using Android.Views;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public class EntryCellRenderer : CellRenderer
	{
		EntryCellView _view;

#pragma warning disable CS0618 // Type or member is obsolete
		protected override global::Android.Views.View GetCellCore(Cell item, global::Android.Views.View convertView, ViewGroup parent, Context context)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			Disconnect();
			if ((_view = convertView as EntryCellView) == null)
			{
				_view = new EntryCellView(context, item);
			}

			UpdateLabel();
			UpdateLabelColor();
			UpdatePlaceholder();
			UpdateKeyboard();
			UpdateHorizontalTextAlignment();
			UpdateVerticalTextAlignment();
			UpdateText();
			UpdateIsEnabled();
			UpdateHeight();
			UpdateFlowDirection();

			_view.TextChanged = OnTextChanged;
			_view.EditingCompleted = OnEditingCompleted;

			return _view;
		}

		protected override void OnCellPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (!_view.IsAlive())
			{
				return;
			}

			base.OnCellPropertyChanged(sender, e);

#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
			if (e.PropertyName == EntryCell.LabelProperty.PropertyName)
				UpdateLabel();
#pragma warning disable CS0618 // Type or member is obsolete
			else if (e.PropertyName == EntryCell.TextProperty.PropertyName)
				UpdateText();
#pragma warning disable CS0618 // Type or member is obsolete
			else if (e.PropertyName == EntryCell.PlaceholderProperty.PropertyName)
				UpdatePlaceholder();
#pragma warning disable CS0618 // Type or member is obsolete
			else if (e.PropertyName == EntryCell.KeyboardProperty.PropertyName)
				UpdateKeyboard();
#pragma warning disable CS0618 // Type or member is obsolete
			else if (e.PropertyName == EntryCell.LabelColorProperty.PropertyName)
				UpdateLabelColor();
#pragma warning disable CS0618 // Type or member is obsolete
			else if (e.PropertyName == EntryCell.HorizontalTextAlignmentProperty.PropertyName)
				UpdateHorizontalTextAlignment();
			else if (e.PropertyName == EntryCell.VerticalTextAlignmentProperty.PropertyName)
				UpdateVerticalTextAlignment();
			else if (e.PropertyName == Cell.IsEnabledProperty.PropertyName)
				UpdateIsEnabled();
			else if (e.PropertyName == "RenderHeight")
				UpdateHeight();
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
			{
				UpdateFlowDirection();
				UpdateHorizontalTextAlignment();
			}
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
		}

		protected virtual NumberKeyListener GetDigitsKeyListener(InputTypes inputTypes)
		{
			// Override this in a custom renderer to use a different NumberKeyListener 
			// or to filter out input types you don't want to allow 
			// (e.g., inputTypes &= ~InputTypes.NumberFlagSigned to disallow the sign)
			return LocalizedDigitsKeyListener.Create(inputTypes);
		}

		void OnEditingCompleted()
		{
			var entryCell = (IEntryCellController)Cell;
			entryCell.SendCompleted();
		}

		void OnTextChanged(string text)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			var entryCell = (EntryCell)Cell;
#pragma warning restore CS0618 // Type or member is obsolete

#pragma warning disable CS0618 // Type or member is obsolete
			entryCell
				.SetValue(EntryCell.TextProperty, text, specificity: SetterSpecificity.FromHandler);
#pragma warning restore CS0618 // Type or member is obsolete
		}

		void UpdateHeight()
		{
			_view.SetRenderHeight(Cell.RenderHeight);
		}

		void UpdateHorizontalTextAlignment()
		{
#pragma warning disable CS0618 // Type or member is obsolete
			var entryCell = (EntryCell)Cell;
#pragma warning restore CS0618 // Type or member is obsolete
			_view.EditText.UpdateHorizontalAlignment(entryCell.HorizontalTextAlignment);
		}

		void UpdateVerticalTextAlignment()
		{
#pragma warning disable CS0618 // Type or member is obsolete
			var entryCell = (EntryCell)Cell;
#pragma warning restore CS0618 // Type or member is obsolete
			_view.EditText.UpdateVerticalAlignment(entryCell.VerticalTextAlignment);
		}

		void UpdateIsEnabled()
		{
#pragma warning disable CS0618 // Type or member is obsolete
			var entryCell = (EntryCell)Cell;
#pragma warning restore CS0618 // Type or member is obsolete
			_view.EditText.Enabled = entryCell.IsEnabled;
		}

		void UpdateKeyboard()
		{
#pragma warning disable CS0618 // Type or member is obsolete
			var entryCell = (EntryCell)Cell;
#pragma warning restore CS0618 // Type or member is obsolete
			var keyboard = entryCell.Keyboard;
			_view.EditText.InputType = keyboard.ToInputType();

			if (keyboard == Keyboard.Numeric)
			{
				_view.EditText.KeyListener = GetDigitsKeyListener(_view.EditText.InputType);
			}
		}

		void UpdateLabel()
		{
#pragma warning disable CS0618 // Type or member is obsolete
			_view.LabelText = ((EntryCell)Cell).Label;
#pragma warning restore CS0618 // Type or member is obsolete
		}

		void UpdateLabelColor()
		{
#pragma warning disable CS0618 // Type or member is obsolete
			_view.SetLabelTextColor(((EntryCell)Cell).LabelColor, global::Android.Resource.Attribute.TextColor);
#pragma warning restore CS0618 // Type or member is obsolete
		}

		void UpdateFlowDirection()
		{
			_view.UpdateFlowDirection(ParentView);
		}

		void UpdatePlaceholder()
		{
#pragma warning disable CS0618 // Type or member is obsolete
			var entryCell = (EntryCell)Cell;
#pragma warning restore CS0618 // Type or member is obsolete
			_view.EditText.Hint = entryCell.Placeholder;
		}

		void UpdateText()
		{
#pragma warning disable CS0618 // Type or member is obsolete
			var entryCell = (EntryCell)Cell;
#pragma warning restore CS0618 // Type or member is obsolete
			if (_view.EditText.Text == entryCell.Text)
				return;

			_view.EditText.Text = entryCell.Text;
		}

		void Disconnect()
		{
			if (_view is null)
				return;

			_view.TextChanged = null;
			_view.FocusChanged = null;
			_view.EditingCompleted = null;
		}
	}
}
