#nullable disable
using System.ComponentModel;
using Android.Content;
using Android.Views;
using Microsoft.Maui.Controls.Internals;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public class TextCellRenderer : CellRenderer
	{
		internal TextCellView View { get; private set; }

#pragma warning disable CS0618 // Type or member is obsolete
		protected override AView GetCellCore(Cell item, AView convertView, ViewGroup parent, Context context)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			if ((View = convertView as TextCellView) == null)
				View = new TextCellView(context, item);

			UpdateMainText();
			UpdateDetailText();
			UpdateHeight();
			UpdateIsEnabled();
			UpdateFlowDirection();
			UpdateAutomationId();
			View.SetImageVisible(false);

			return View;
		}

		protected override void OnCellPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			if (View.IsDisposed())
			{
				return;
			}

#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
			if (args.PropertyName == TextCell.TextProperty.PropertyName || args.PropertyName == TextCell.TextColorProperty.PropertyName)
				UpdateMainText();
			else if (args.PropertyName == TextCell.DetailProperty.PropertyName || args.PropertyName == TextCell.DetailColorProperty.PropertyName)
				UpdateDetailText();
			else if (args.PropertyName == Cell.IsEnabledProperty.PropertyName)
				UpdateIsEnabled();
			else if (args.PropertyName == "RenderHeight")
				UpdateHeight();
			else if (args.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
				UpdateFlowDirection();
			else if (args.PropertyName == VisualElement.AutomationIdProperty.PropertyName)
				UpdateAutomationId();
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
		}

		void UpdateAutomationId()
		{
			View.ContentDescription = Cell.AutomationId;
		}

		void UpdateDetailText()
		{
#pragma warning disable CS0618 // Type or member is obsolete
			var cell = (TextCell)Cell;
#pragma warning restore CS0618 // Type or member is obsolete
			View.DetailText = cell.Detail;
			View.SetDetailTextColor(cell.DetailColor);
		}

		void UpdateHeight()
		{
			View.SetRenderHeight(Cell.RenderHeight);
		}

		void UpdateIsEnabled()
		{
#pragma warning disable CS0618 // Type or member is obsolete
			var cell = (TextCell)Cell;
#pragma warning restore CS0618 // Type or member is obsolete
			View.SetIsEnabled(cell.IsEnabled);
		}

		void UpdateFlowDirection()
		{
			View.UpdateFlowDirection(ParentView);
		}

		void UpdateMainText()
		{
#pragma warning disable CS0618 // Type or member is obsolete
			var cell = (TextCell)Cell;
#pragma warning restore CS0618 // Type or member is obsolete
			View.MainText = cell.Text;

#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
			if (!cell.GetIsGroupHeader<ItemsView<Cell>, Cell>())
				View.SetDefaultMainTextColor(Application.AccentColor);
			else
				View.SetDefaultMainTextColor(null);
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete

			View.SetMainTextColor(cell.TextColor);

			PlatformInterop.RequestLayoutIfNeeded(View);
		}

		// ensure we don't get other people's BaseCellView's
		internal sealed class TextCellView : BaseCellView
		{
#pragma warning disable CS0618 // Type or member is obsolete
			public TextCellView(Context context, Cell cell) : base(context, cell)
#pragma warning restore CS0618 // Type or member is obsolete
			{
			}
		}
	}
}