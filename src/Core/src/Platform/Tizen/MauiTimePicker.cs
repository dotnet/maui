using System;
using Tizen.NUI;
using Tizen.NUI.BaseComponents;
using Tizen.NUI.Components;
using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.NUI;
using NButton = Tizen.UIExtensions.NUI.Button;
using NView = Tizen.NUI.BaseComponents.View;
using NFontAttributes = Tizen.UIExtensions.Common.FontAttributes;
using NTextAlignment = Tizen.UIExtensions.Common.TextAlignment;

namespace Microsoft.Maui.Platform
{
	public class MauiTimePicker : Popup<TimeSpan>
	{
		DateTime _time;

		public MauiTimePicker(TimeSpan time)
		{
			_time = new DateTime() + time;
		}

		protected override NView CreateContent()
		{
			Layout = new LinearLayout
			{
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center,
			};
			BackgroundColor = new Tizen.NUI.Color(0.1f, 0.1f, 0.1f, 0.5f);

			var content = new NView
			{
				Layout = new LinearLayout
				{
					VerticalAlignment = VerticalAlignment.Center,
					HorizontalAlignment = HorizontalAlignment.Center,
					LinearOrientation = LinearLayout.Orientation.Vertical
				},
				SizeWidth = (float)Window.Instance.WindowSize.Width * 0.8f,
				BackgroundColor = Tizen.NUI.Color.White,
			};

			var title = new Label
			{
				Text = "Set Time",
				WidthSpecification = LayoutParamPolicies.MatchParent,
				HeightSpecification = LayoutParamPolicies.WrapContent,
				Padding = new Extents(10, 10, 10, 10),
				HorizontalTextAlignment = NTextAlignment.Center,
				VerticalTextAlignment = NTextAlignment.Center,
				FontAttributes = NFontAttributes.Bold,
				TextColor = Tizen.UIExtensions.Common.Color.White,
				FontSize = 6 * DeviceInfo.ScalingFactor,
				BackgroundColor = new Tizen.NUI.Color("#344955"),
			};
			title.UpdateBackgroundColor(Tizen.UIExtensions.Common.Color.FromHex("#344955"));
			content.Add(title);

			var timePicker = new TimePicker
			{
				Layout = new LinearLayout
				{
					VerticalAlignment = VerticalAlignment.Center,
					HorizontalAlignment = HorizontalAlignment.Center,
				},
				HeightSpecification = LayoutParamPolicies.WrapContent,
				SizeWidth = (float)Window.Instance.WindowSize.Width * 0.8f,
				Time = _time,
			};

			content.Add(timePicker);

			View hlayout = new View
			{
				Layout = new LinearLayout
				{
					VerticalAlignment = VerticalAlignment.Center,
					HorizontalAlignment = HorizontalAlignment.End,
					LinearOrientation = LinearLayout.Orientation.Horizontal
				},
				WidthSpecification = LayoutParamPolicies.MatchParent,
				HeightSpecification = LayoutParamPolicies.WrapContent
			};
			content.Add(hlayout);

			NButton cancelBtn = new NButton
			{
				Margin = new Extents(20, 20, 10, 10),
				Text = "cancel",
				SizeWidth = content.SizeWidth * 0.4f,
				HeightSpecification = LayoutParamPolicies.WrapContent,
			};
			cancelBtn.Clicked += delegate
			{
				Close();
			};
			hlayout.Add(cancelBtn);

			NButton okBtn = new NButton
			{
				Margin = new Extents(20, 20, 10, 10),
				Text = "ok",
				HeightSpecification = LayoutParamPolicies.WrapContent,
				SizeWidth = content.SizeWidth * 0.4f,
			};
			okBtn.Clicked += delegate
			{
				SendSubmit(TimeSpan.FromTicks(timePicker.Time.Ticks));
			};
			hlayout.Add(okBtn);

			return content;
		}
	}
}
