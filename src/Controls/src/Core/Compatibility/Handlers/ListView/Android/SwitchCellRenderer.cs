#nullable disable
using System.ComponentModel;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using ASwitch = Android.Widget.Switch;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public class SwitchCellRenderer : CellRenderer
	{
		SwitchCellView _view;
		Drawable _defaultTrackDrawable;

#pragma warning disable CS0618 // Type or member is obsolete
		protected override AView GetCellCore(Cell item, AView convertView, ViewGroup parent, Context context)
#pragma warning restore CS0618 // Type or member is obsolete
		{
#pragma warning disable CS0618 // Type or member is obsolete
			var cell = (SwitchCell)Cell;
#pragma warning restore CS0618 // Type or member is obsolete

			if ((_view = convertView as SwitchCellView) == null)
				_view = new SwitchCellView(context, item);

			_view.Cell = cell;

			var aSwitch = _view.AccessoryView as ASwitch;
			if (aSwitch != null)
				_defaultTrackDrawable = aSwitch.TrackDrawable;

			UpdateText();
			UpdateChecked();
			UpdateHeight();
			UpdateIsEnabled(_view, cell);
			UpdateFlowDirection();
			UpdateOnColor(_view, cell);

			return _view;
		}

		protected override void OnCellPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
			if (args.PropertyName == SwitchCell.TextProperty.PropertyName)
				UpdateText();
			else if (args.PropertyName == SwitchCell.OnProperty.PropertyName)
			{
				UpdateChecked();
#pragma warning disable CS0618 // Type or member is obsolete
				UpdateOnColor(_view, (SwitchCell)sender);
#pragma warning restore CS0618 // Type or member is obsolete
			}
			else if (args.PropertyName == "RenderHeight")
				UpdateHeight();
			else if (args.PropertyName == Cell.IsEnabledProperty.PropertyName)
#pragma warning disable CS0618 // Type or member is obsolete
				UpdateIsEnabled(_view, (SwitchCell)sender);
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
			else if (args.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
				UpdateFlowDirection();
			else if (args.PropertyName == SwitchCell.OnColorProperty.PropertyName)
#pragma warning disable CS0618 // Type or member is obsolete
				UpdateOnColor(_view, (SwitchCell)sender);
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
		}

		void UpdateChecked()
		{
#pragma warning disable CS0618 // Type or member is obsolete
			((ASwitch)_view.AccessoryView).Checked = ((SwitchCell)Cell).On;
#pragma warning restore CS0618 // Type or member is obsolete
		}

#pragma warning disable CS0618 // Type or member is obsolete
		void UpdateIsEnabled(SwitchCellView cell, SwitchCell switchCell)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			cell.Enabled = switchCell.IsEnabled;
			var aSwitch = cell.AccessoryView as ASwitch;
			if (aSwitch != null)
				aSwitch.Enabled = switchCell.IsEnabled;
		}

		void UpdateFlowDirection()
		{
			_view.UpdateFlowDirection(ParentView);
		}

		void UpdateHeight()
		{
			_view.SetRenderHeight(Cell.RenderHeight);
		}

		void UpdateText()
		{
#pragma warning disable CS0618 // Type or member is obsolete
			_view.MainText = ((SwitchCell)Cell).Text;
#pragma warning restore CS0618 // Type or member is obsolete
		}

#pragma warning disable CS0618 // Type or member is obsolete
		void UpdateOnColor(SwitchCellView cell, SwitchCell switchCell)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			var aSwitch = cell.AccessoryView as ASwitch;
			if (aSwitch != null)
			{
				if (switchCell.On)
				{
					if (switchCell.OnColor == null)
					{
						aSwitch.TrackDrawable = _defaultTrackDrawable;
					}
					else
					{
						aSwitch.TrackDrawable.SetColorFilter(switchCell.OnColor, FilterMode.Multiply);
					}
				}
				else
				{
					aSwitch.TrackDrawable.ClearColorFilter();
				}
			}
		}
	}
}