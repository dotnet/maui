using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1588, "[WPF] Stacklayout WidthRequest adds unwanted margin", PlatformAffected.WPF)]
	public partial class Issue1588 : TestContentPage
	{
#if APP
		public Issue1588()
		{
			InitializeComponent();
		}
#endif

		protected override void Init()
		{
		}
	}

	public enum EntryOrientation
	{
		Vertical,
		Horizontal
	}

	class LabledEntry : ContentView
	{
		StackLayout _stk;
		Label _label;
		Entry _entry;

		public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
			"FontSize",
			typeof(double),
			typeof(LabledEntry),
			Device.GetNamedSize(NamedSize.Small, typeof(Entry)),
			BindingMode.TwoWay,
			propertyChanged: (bindable, oldvalue, newvalue) =>
			{
				var labledEntry = ((LabledEntry)bindable);
				var fontSize = (double)newvalue;
				labledEntry._entry.FontSize = fontSize;
				labledEntry._label.FontSize = fontSize;
			});

		public double FontSize
		{
			set { SetValue(FontSizeProperty, value); }
			get { return (double)GetValue(FontSizeProperty); }
		}

		public static readonly BindableProperty OrientationProperty = BindableProperty.Create(
			"Orientation",
			typeof(EntryOrientation),
			typeof(LabledEntry),
			EntryOrientation.Vertical,
			BindingMode.TwoWay,
			propertyChanged: (bindable, oldvalue, newvalue) =>
			{
				var labledEntry = ((LabledEntry)bindable);
				var orientation = (EntryOrientation)newvalue;
				labledEntry._stk.Orientation = orientation == EntryOrientation.Vertical ? StackOrientation.Vertical : StackOrientation.Horizontal;
				labledEntry.Orientation = orientation;
			});

		public EntryOrientation Orientation
		{
			set { SetValue(OrientationProperty, value); }
			get { return (EntryOrientation)GetValue(OrientationProperty); }
		}

		public static readonly BindableProperty KeyboardProperty = BindableProperty.Create(
			"Keyboard",
			typeof(Keyboard),
			typeof(LabledEntry),
			Keyboard.Default,
			BindingMode.TwoWay,
			propertyChanged: (bindable, oldvalue, newvalue) =>
			{
				var labledEntry = ((LabledEntry)bindable);
				var keyboard = (Keyboard)newvalue;
				labledEntry._entry.Keyboard = keyboard;
				labledEntry.Keyboard = keyboard;
			});

		public Keyboard Keyboard
		{
			set { SetValue(KeyboardProperty, value); }
			get { return (Keyboard)GetValue(KeyboardProperty); }
		}

		public static readonly BindableProperty LabelTextProperty = BindableProperty.Create(
			"LabelText",
			typeof(string),
			typeof(LabledEntry),
			null,
			BindingMode.TwoWay,
			propertyChanged: (bindable, oldvalue, newvalue) =>
			{
				var labledEntry = ((LabledEntry)bindable);
				labledEntry._label.Text = (string)newvalue;
				labledEntry.LabelText = (string)newvalue;
			});

		public string LabelText
		{
			set { SetValue(LabelTextProperty, value); }
			get { return (string)GetValue(LabelTextProperty); }
		}

		public static readonly BindableProperty EntryTextProperty = BindableProperty.Create(
			"EntryText",
			typeof(string),
			typeof(LabledEntry),
			null,
			BindingMode.TwoWay,
			propertyChanged: (bindable, oldvalue, newvalue) =>
			{
				var labledEntry = ((LabledEntry)bindable);
				labledEntry._entry.Text = (string)newvalue;
				labledEntry.EntryText = (string)newvalue;
			});

		public string EntryText
		{
			set { SetValue(EntryTextProperty, value); }
			get { return (string)GetValue(EntryTextProperty); }
		}

		public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
			"Placeholder",
			typeof(string),
			typeof(LabledEntry),
			null,
			BindingMode.TwoWay,
			propertyChanged: (bindable, oldvalue, newvalue) =>
			{
				var labledEntry = ((LabledEntry)bindable);
				labledEntry._entry.Placeholder = (string)newvalue;
				labledEntry.Placeholder = (string)newvalue;
			});

		public string Placeholder
		{
			set { SetValue(PlaceholderProperty, value); }
			get { return (string)GetValue(PlaceholderProperty); }
		}

		public static readonly BindableProperty LabelTextColorProperty = BindableProperty.Create(
			"LabelTextColor",
			typeof(Color),
			typeof(LabledEntry),
			Color.Default,
			BindingMode.TwoWay,
			propertyChanged: (bindable, oldvalue, newvalue) =>
			{
				var labledEntry = ((LabledEntry)bindable);
				labledEntry._label.TextColor = (Color)newvalue;
				labledEntry.LabelTextColor = (Color)newvalue;
			});

		public Color LabelTextColor
		{
			set { SetValue(LabelTextColorProperty, value); }
			get { return (Color)GetValue(LabelTextColorProperty); }
		}

		public static readonly BindableProperty EntryTextColorProperty = BindableProperty.Create(
			"EntryTextColor",
			typeof(Color),
			typeof(LabledEntry),
			Color.Default,
			BindingMode.TwoWay,
			propertyChanged: (bindable, oldvalue, newvalue) =>
			{
				var labledEntry = ((LabledEntry)bindable);
				labledEntry._entry.TextColor = (Color)newvalue;
				labledEntry.EntryTextColor = (Color)newvalue;
			});

		public Color EntryTextColor
		{
			set { SetValue(EntryTextColorProperty, value); }
			get { return (Color)GetValue(EntryTextColorProperty); }
		}

		public static readonly BindableProperty PlaceholderColorProperty = BindableProperty.Create(
			"PlaceholderColor",
			typeof(Color),
			typeof(LabledEntry),
			Color.Default,
			BindingMode.TwoWay,
			propertyChanged: (bindable, oldvalue, newvalue) =>
			{
				var labledEntry = ((LabledEntry)bindable);
				labledEntry._entry.PlaceholderColor = (Color)newvalue;
				labledEntry.PlaceholderColor = (Color)newvalue;
			});

		public Color PlaceholderColor
		{
			set { SetValue(PlaceholderColorProperty, value); }
			get { return (Color)GetValue(PlaceholderColorProperty); }
		}

		public static readonly BindableProperty MaxLengthProperty = BindableProperty.Create(
			"MaxLength",
			typeof(int),
			typeof(LabledEntry),
			30,
			BindingMode.TwoWay,
			propertyChanged: (bindable, oldvalue, newvalue) =>
			{
				var labledEntry = ((LabledEntry)bindable);

			});

		public int MaxLength
		{
			set { SetValue(MaxLengthProperty, value); }
			get { return (int)GetValue(MaxLengthProperty); }
		}

		public LabledEntry()
		{
			_stk = new StackLayout();
			_entry = new Entry();
			_entry.FontSize = Device.GetNamedSize(NamedSize.Small, _entry);
			_label = new Label();
			_entry.FontSize = Device.GetNamedSize(NamedSize.Small, _label);
			_entry.HorizontalOptions = LayoutOptions.FillAndExpand;
			_label.VerticalOptions = LayoutOptions.Center;
			_stk.Children.Add(_label);
			_stk.Children.Add(_entry);
			Content = _stk;
		}
	}

	public enum DividerOrientation
	{
		Vertical,
		Horizontal
	}

	class Divider : ContentView
	{
		BoxView _line;

		public static readonly BindableProperty OrientationProperty = BindableProperty.Create(
			"Orientation",
			typeof(DividerOrientation),
			typeof(Divider),
			DividerOrientation.Vertical,
			BindingMode.TwoWay,
			propertyChanged: (bindable, oldvalue, newvalue) =>
			{
				var divider = ((Divider)bindable);
				var orientation = (DividerOrientation)newvalue;
				if (orientation == DividerOrientation.Vertical)
				{
					divider._line.WidthRequest = divider.Thickness;
					divider._line.HeightRequest = -1;
				}
				else
				{
					divider._line.HeightRequest = divider.Thickness;
					divider._line.WidthRequest = -1;
				}
				divider.Orientation = orientation;
			});

		public DividerOrientation Orientation
		{
			set { SetValue(OrientationProperty, value); }
			get { return (DividerOrientation)GetValue(OrientationProperty); }
		}

		public static readonly BindableProperty ColorProperty = BindableProperty.Create(
			"Color",
			typeof(Color),
			typeof(Divider),
			Color.Black,
			BindingMode.TwoWay,
			propertyChanged: (bindable, oldvalue, newvalue) =>
			{
				var divider = ((Divider)bindable);
				divider._line.Color = (Color)newvalue;
				divider.Color = (Color)newvalue;
			});

		public Color Color
		{
			set { SetValue(ColorProperty, value); }
			get { return (Color)GetValue(ColorProperty); }
		}

		public static readonly BindableProperty ThicknessProperty = BindableProperty.Create(
		   "Thickness",
		   typeof(double),
		   typeof(Divider),
		   1.0,
		   BindingMode.TwoWay,
		   propertyChanged: (bindable, oldvalue, newvalue) =>
		   {
			   var divider = ((Divider)bindable);
			   var thickness = (double)newvalue;
			   if (divider.Orientation == DividerOrientation.Vertical)
			   {
				   divider._line.WidthRequest = thickness;
			   }
			   else
			   {
				   divider._line.HeightRequest = thickness;
			   }
			   divider.Thickness = thickness;
		   });

		public double Thickness
		{
			set { SetValue(ThicknessProperty, value); }
			get { return (double)GetValue(ThicknessProperty); }
		}

		public Divider()
		{
			_line = new BoxView();
			Content = _line;
		}
	}
}