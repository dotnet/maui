namespace Microsoft.Maui.Controls.ControlGallery.iOS
{
	public partial class AppDelegate : MauiUIApplicationDelegate
	{
		void AddNativeControls(NestedNativeControlGalleryPage page)
		{
			if (page.NativeControlsAdded)
			{
				return;
			}

			StackLayout sl = page.Layout;

			// Create and add a native UILabel
			var originalText = "I am a native UILabel";
			var longerText =
				"I am a native UILabel with considerably more text. Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";

#pragma warning disable CA1416 // TODO: UILabel.MinimumFontSize has [UnsupportedOSPlatform("ios6.0")]
#pragma warning disable CA1422 // Validate platform compatibility
			var uilabel = new UILabel
			{
				MinimumFontSize = 14f,
				Text = originalText,
				Lines = 0,
				LineBreakMode = UILineBreakMode.WordWrap,
				Font = UIFont.FromName("Helvetica", 24f)
			};

			sl?.Add(uilabel);

			// Create and add a native Button 
			var uibutton = new UIButton(UIButtonType.System);
			uibutton.SetTitle("Toggle Text Amount", UIControlState.Normal);
			uibutton.TitleLabel.Font = UIFont.FromName("Helvetica", 14f);


			uibutton.TouchUpInside += (sender, args) =>
			{
				uilabel.Text = uilabel.Text == originalText ? longerText : originalText;
				uilabel.SizeToFit();
			};

			sl?.Children.Add(uibutton.ToView());

			// Create some control which we know don't behave correctly with regard to measurement
			var difficultControl0 = new BrokenNativeControl
			{
				MinimumFontSize = 14f,
				Font = UIFont.FromName("Helvetica", 14f),
				Lines = 0,
				LineBreakMode = UILineBreakMode.WordWrap,
				Text = "Doesn't play nice with sizing. That's why there's a big gap around it."
			};

			var difficultControl1 = new BrokenNativeControl
			{
				MinimumFontSize = 14f,
				Font = UIFont.FromName("Helvetica", 14f),
				Lines = 0,
				LineBreakMode = UILineBreakMode.WordWrap,
				Text = "Custom size fix specified. No gaps."
			};

			var explanation0 = new UILabel
			{
				MinimumFontSize = 14f,
				Text = "The next control is a customized label with a bad SizeThatFits implementation.",
				Lines = 0,
				LineBreakMode = UILineBreakMode.WordWrap,
				Font = UIFont.FromName("Helvetica", 24f)
			};

			var explanation1 = new UILabel
			{
				MinimumFontSize = 14f,
				Text = "The next control is the same broken class as above, but we pass in an override to the GetDesiredSize method.",
				Lines = 0,
				LineBreakMode = UILineBreakMode.WordWrap,
				Font = UIFont.FromName("Helvetica", 24f)
			};
#pragma warning restore CA1422 // Validate platform compatibility
#pragma warning restore CA1416

			// Add a misbehaving control
			sl?.Children.Add(explanation0);
			sl?.Children.Add(difficultControl0);

			// Add the misbehaving control with a custom delegate for FixSize
			sl?.Children.Add(explanation1);
			sl?.Children.Add(difficultControl1, FixSize);

			page.NativeControlsAdded = true;
		}

		SizeRequest? FixSize(NativeViewWrapperRenderer renderer, double width, double height)
		{
			var uiView = renderer.Control;
			var view = renderer.Element;

			if (uiView == null || view == null)
			{
				return null;
			}

			var constraint = new CGSize(width, height);

			// Let the BrokenNativeControl determine its size (which we know will be wrong)
			var badRect = uiView.SizeThatFits(constraint);

			// And we'll use the width (which is fine) and substitute our own height
			return new SizeRequest(new Size(badRect.Width, 20));
		}

		void AddNativeBindings(NativeBindingGalleryPage page)
		{
			if (page.NativeControlsAdded)
				return;

			StackLayout sl = page.Layout;

			int width = (int)sl.Width;
			int heightCustomLabelView = 100;

#pragma warning disable CA1422 // Validate platform compatibility
			var uilabel = new UILabel(new CGRect(0, 0, width, heightCustomLabelView))
			{
#pragma warning disable CA1416 // TODO: UILabel.MinimumFontSize has [UnsupportedOSPlatform("ios6.0")]
				MinimumFontSize = 14f,
#pragma warning restore CA1416
				Lines = 0,
				LineBreakMode = UILineBreakMode.WordWrap,
				Font = UIFont.FromName("Helvetica", 24f),
				Text = "DefaultText"
			};
#pragma warning restore CA1422 // Validate platform compatibility

			var uibuttonColor = new UIButton(UIButtonType.System);
			uibuttonColor.SetTitle("Toggle Text Color Binding", UIControlState.Normal);
			uibuttonColor.TitleLabel.Font = UIFont.FromName("Helvetica", 14f);
			uibuttonColor.TouchUpInside += (sender, args) => uilabel.TextColor = UIColor.Blue;

			var nativeColorConverter = new ColorConverter();

			uilabel.SetBinding("Text", new Binding("NativeLabel"));
			uilabel.SetBinding(nameof(uilabel.TextColor), new Binding("NativeLabelColor", converter: nativeColorConverter));

			var kvoSlider = new KVOUISlider();
			kvoSlider.MaxValue = 100;
			kvoSlider.MinValue = 0;
			kvoSlider.SetBinding(nameof(kvoSlider.KVOValue), new Binding("Age", BindingMode.TwoWay));
			sl?.Children.Add(kvoSlider);

			var uiView = new UIView(new CGRect(0, 0, width, heightCustomLabelView));
			uiView.Add(uilabel);
			sl?.Children.Add(uiView);
			sl?.Children.Add(uibuttonColor.ToView());
			// TODO: Replace with a new plugin or API
			//var colorPicker = new AdvancedColorPicker.ColorPickerView(new CGRect(0, 0, width, 300));
			//colorPicker.SetBinding("SelectedColor", new Binding("NativeLabelColor", BindingMode.TwoWay, nativeColorConverter), "ColorPicked");
			//sl?.Children.Add(colorPicker);
			page.NativeControlsAdded = true;
		}
	}
}
