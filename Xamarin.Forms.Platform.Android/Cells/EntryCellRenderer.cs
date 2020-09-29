using System.ComponentModel;
using Android.Content;
using Android.Text;
using Android.Text.Method;
using Android.Views;

namespace Xamarin.Forms.Platform.Android
{
	public class EntryCellRenderer : CellRenderer
	{
		EntryCellView _view;

		protected override global::Android.Views.View GetCellCore(Cell item, global::Android.Views.View convertView, ViewGroup parent, Context context)
		{
			if ((_view = convertView as EntryCellView) == null)
				_view = new EntryCellView(context, item);
			else
			{
				_view.TextChanged = null;
				_view.FocusChanged = null;
				_view.EditingCompleted = null;
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

			if (e.PropertyName == EntryCell.LabelProperty.PropertyName)
				UpdateLabel();
			else if (e.PropertyName == EntryCell.TextProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == EntryCell.PlaceholderProperty.PropertyName)
				UpdatePlaceholder();
			else if (e.PropertyName == EntryCell.KeyboardProperty.PropertyName)
				UpdateKeyboard();
			else if (e.PropertyName == EntryCell.LabelColorProperty.PropertyName)
				UpdateLabelColor();
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
			var entryCell = (EntryCell)Cell;
			entryCell.Text = text;
		}

		void UpdateHeight()
		{
			_view.SetRenderHeight(Cell.RenderHeight);
		}

		void UpdateHorizontalTextAlignment()
		{
			var entryCell = (EntryCell)Cell;
			_view.EditText.UpdateHorizontalAlignment(entryCell.HorizontalTextAlignment, _view.Context.HasRtlSupport());
		}

		void UpdateVerticalTextAlignment()
		{
			var entryCell = (EntryCell)Cell;
			_view.EditText.UpdateVerticalAlignment(entryCell.VerticalTextAlignment);
		}

		void UpdateIsEnabled()
		{
			var entryCell = (EntryCell)Cell;
			_view.EditText.Enabled = entryCell.IsEnabled;
		}

		void UpdateKeyboard()
		{
			var entryCell = (EntryCell)Cell;
			var keyboard = entryCell.Keyboard;
			_view.EditText.InputType = keyboard.ToInputType();

			if (keyboard == Keyboard.Numeric)
			{
				_view.EditText.KeyListener = GetDigitsKeyListener(_view.EditText.InputType);
			}
		}

		void UpdateLabel()
		{
			_view.LabelText = ((EntryCell)Cell).Label;
		}

		void UpdateLabelColor()
		{
			_view.SetLabelTextColor(((EntryCell)Cell).LabelColor, global::Android.Resource.Attribute.TextColor);
		}

		void UpdateFlowDirection()
		{
			_view.UpdateFlowDirection(ParentView);
		}

		void UpdatePlaceholder()
		{
			var entryCell = (EntryCell)Cell;
			_view.EditText.Hint = entryCell.Placeholder;
		}

		void UpdateText()
		{
			var entryCell = (EntryCell)Cell;
			if (_view.EditText.Text == entryCell.Text)
				return;

			_view.EditText.Text = entryCell.Text;
		}
	}
}