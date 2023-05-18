using System;
using Tizen.NUI;
using Tizen.NUI.BaseComponents;
using Tizen.NUI.Components;
using Tizen.UIExtensions.NUI;
using NButton = Tizen.UIExtensions.NUI.Button;
using NFontAttributes = Tizen.UIExtensions.Common.FontAttributes;
using NTextAlignment = Tizen.UIExtensions.Common.TextAlignment;
using NView = Tizen.NUI.BaseComponents.View;
using TColor = Tizen.UIExtensions.Common.Color;

namespace Microsoft.Maui.Platform
{
	public class MauiDateTimePicker : Popup<DateTime>
	{
		DateTime _dateTime;
		bool _isTimePicker;

		public MauiDateTimePicker(DateTime dateTime, bool isTimePicker)
		{
			_dateTime = dateTime;
			_isTimePicker = isTimePicker;
		}

		protected override NView CreateContent()
		{
			Layout = new LinearLayout
			{
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center,
			};
			BackgroundColor = new Tizen.NUI.Color(0.1f, 0.1f, 0.1f, 0.5f);
			var margin1 = (ushort)20d.ToPixel();
			var margin2 = (ushort)10d.ToPixel();
			var radius = 8d.ToPixel();

			var isHorizontal = Window.Instance.WindowSize.Width > Window.Instance.WindowSize.Height;
			var content = new NView
			{
				CornerRadius = radius,
				BoxShadow = new Shadow(20d.ToPixel(), TColor.Black.ToNative()),
				Layout = new LinearLayout
				{
					VerticalAlignment = VerticalAlignment.Center,
					HorizontalAlignment = HorizontalAlignment.Center,
					LinearOrientation = LinearLayout.Orientation.Vertical
				},
				SizeWidth = Window.Instance.WindowSize.Width * (isHorizontal ? 0.5f : 0.8f),
				BackgroundColor = Tizen.NUI.Color.White,
			};

			var title = new Label
			{
				Margin = new Extents(margin1, margin1, margin1, margin2),
				HorizontalTextAlignment = NTextAlignment.Start,
				WidthSpecification = LayoutParamPolicies.MatchParent,
				VerticalTextAlignment = NTextAlignment.Center,
				FontAttributes = NFontAttributes.Bold,
				TextColor = TColor.FromHex("#000000"),
				PixelSize = 21d.ToPixel(),
			};
			title.Text = _isTimePicker ? "Set Time" : "Set Date";
			content.Add(title);

			Control dateTimePicker = _isTimePicker ? new TimePicker
			{
				Time = _dateTime
			} : new DatePicker
			{
				Date = _dateTime
			};
			dateTimePicker.Layout = new LinearLayout
			{
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center,
			};
			dateTimePicker.Margin = new Extents(margin1, margin1, 0, 0);
			dateTimePicker.HeightSpecification = LayoutParamPolicies.WrapContent;
			dateTimePicker.SizeWidth = (float)Window.Instance.WindowSize.Width * 0.8f;

			content.Add(dateTimePicker);

			View hlayout = new View
			{
				Layout = new LinearLayout
				{
					VerticalAlignment = VerticalAlignment.Center,
					HorizontalAlignment = HorizontalAlignment.End,
					LinearOrientation = LinearLayout.Orientation.Horizontal
				},
				Margin = new Extents(margin1, margin1, margin2, margin1),
				WidthSpecification = LayoutParamPolicies.MatchParent,
				HeightSpecification = LayoutParamPolicies.WrapContent
			};
			content.Add(hlayout);

			NButton cancelButton = new NButton
			{
				Text = "Cancel",
				TextColor = TColor.Black,
				BackgroundColor = TColor.Transparent.ToNative(),
			};
			cancelButton.TextLabel.PixelSize = 15d.ToPixel();
			cancelButton.SizeWidth = cancelButton.TextLabel.NaturalSize.Width + 15d.ToPixel() * 2;
			cancelButton.Clicked += delegate
			{
				Close();
			};
			hlayout.Add(cancelButton);

			NButton okButton = new NButton
			{
				Text = "OK",
				Margin = new Extents(margin2, 0, 0, 0),
				TextColor = TColor.Black,
				BackgroundColor = TColor.Transparent.ToNative(),
			};
			okButton.TextLabel.PixelSize = 15d.ToPixel();
			okButton.SizeWidth = okButton.TextLabel.NaturalSize.Width + 15d.ToPixel() * 2;
			okButton.Clicked += delegate
			{
				if (dateTimePicker is TimePicker timePicker)
				{
					_dateTime = timePicker.Time;
				}
				else if (dateTimePicker is DatePicker datePicker)
				{
					_dateTime = datePicker.Date;
				}

				SendSubmit(_dateTime);
			};
			hlayout.Add(okButton);

			Relayout += (s, e) =>
			{
				var isHorizontal = Window.Instance.WindowSize.Width > Window.Instance.WindowSize.Height;
				content.SizeWidth = Window.Instance.WindowSize.Width * (isHorizontal ? 0.5f : 0.8f);
			};

			return content;
		}
	}
}
