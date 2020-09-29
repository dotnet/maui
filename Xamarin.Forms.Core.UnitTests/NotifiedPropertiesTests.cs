using System;
using System.ComponentModel;
using System.Windows.Input;
using NUnit.Framework;
using Xamarin.Forms.Maps;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class NotifiedPropertiesTests : BaseTestFixture
	{
		public abstract class PropertyTestCase
		{
			public string Name { get; set; }
			public Func<INotifyPropertyChanged, object> PropertyGetter { get; set; }
			public Action<INotifyPropertyChanged, object> PropertySetter { get; set; }
			public object ExpectedDefaultValue { get; set; }
			public object TestValue { get; set; }
			public abstract INotifyPropertyChanged CreateView();
			public virtual string DebugName
			{
				get { return Name; }
			}
		}

		public class PropertyTestCase<TView, TProperty> : PropertyTestCase where TView : INotifyPropertyChanged
		{
			Func<TView> init;
			Func<TProperty> expectedValueCreator;

			public PropertyTestCase(string name, Func<TView, TProperty> propertyGetter, Action<TView, TProperty> propertySetter, Func<TProperty> expectedDefaultValue, TProperty testValue, Func<TView> init = null)
			{
				Name = name;
				PropertyGetter = v => propertyGetter((TView)v);
				PropertySetter = (v, o) => propertySetter((TView)v, (TProperty)o);
				expectedValueCreator = expectedDefaultValue;
				TestValue = testValue;
				this.init = init;
			}

			public override INotifyPropertyChanged CreateView()
			{
				ExpectedDefaultValue = expectedValueCreator();
				if (init != null)
					return init();
				if (typeof(TView) == typeof(View))
					return new View();
				return (TView)Activator.CreateInstance(typeof(TView), new object[] { });
			}

			public override string DebugName
			{
				get
				{
					return typeof(TView).Name + "." + Name;
				}
			}
		}

#pragma warning disable 0414
		static PropertyTestCase[] Properties = {
			new PropertyTestCase<View, bool> ("InputTransparent", v => v.InputTransparent, (v, o) => v.InputTransparent = o, () => false, true),
			new PropertyTestCase<View, double> ("Scale", v => v.Scale, (v, o) => v.Scale = o, () => 1d, 2d),
			new PropertyTestCase<View, double> ("Rotation", v => v.Rotation, (v, o) => v.Rotation = o, () => 0d, 90d),
			new PropertyTestCase<View, double> ("RotationX", v => v.RotationX, (v, o) => v.RotationX = o, () => 0d, 90d),
			new PropertyTestCase<View, double> ("RotationY", v => v.RotationY, (v, o) => v.RotationY = o, () => 0d, 90d),
			new PropertyTestCase<View, double> ("AnchorX", v => v.AnchorX, (v, o) => v.AnchorX = o, () => 0.5d, 0d),
			new PropertyTestCase<View, double> ("AnchorY", v => v.AnchorY, (v, o) => v.AnchorY = o, () => 0.5d, 0d),
			new PropertyTestCase<View, double> ("TranslationX", v => v.TranslationX, (v, o) => v.TranslationX = o, () => 0d, 20d),
			new PropertyTestCase<View, double> ("TranslationY", v => v.TranslationY, (v, o) => v.TranslationY = o, () => 0d, 20d),
			new PropertyTestCase<View, double> ("Opacity", v => v.Opacity, (v, o) => v.Opacity = o, () => 1d, 0.5d),
			new PropertyTestCase<View, bool> ("IsEnabled", v => v.IsEnabled, (v, o) => v.IsEnabled = o, () => true, false),
			new PropertyTestCase<View, bool> ("IsVisible", v => v.IsVisible, (v, o) => v.IsVisible = o, () => true, false),
			new PropertyTestCase<View, string> ("ClassId", v => v.ClassId, (v, o) => v.ClassId = o, () => null, "Foo"),
			new PropertyTestCase<ActivityIndicator, bool> ("IsRunning", v => v.IsRunning, (v, o) => v.IsRunning = o, () => false, true),
			new PropertyTestCase<ActivityIndicator, Color> ("Color", v => v.Color, (v, o) => v.Color = o, () => Color.Default, new Color (0, 1, 0)),
			new PropertyTestCase<Button, string> ("Text", v => v.Text, (v, o) => v.Text = o, () => null, "Foo"),
			new PropertyTestCase<Button, Color> ("TextColor", v => v.TextColor, (v, o) => v.TextColor = o, () => Color.Default, new Color (0, 1, 0)),
			new PropertyTestCase<Button, Font> ("Font", v => v.Font, (v, o) => v.Font = o, () => default (Font), Font.SystemFontOfSize (20)),
			new PropertyTestCase<Button, double> ("BorderWidth", v => v.BorderWidth, (v, o) => v.BorderWidth = o, () => -1d, 16d),
			new PropertyTestCase<Button, int> ("BorderRadius", v => v.BorderRadius, (v, o) => v.BorderRadius = o, () => 5, 12),
			new PropertyTestCase<Button, int> ("CornerRadius", v => v.CornerRadius, (v, o) => v.CornerRadius = o, () => -1, 12),
			new PropertyTestCase<Button, Color> ("BorderColor", v => v.BorderColor, (v, o) => v.BorderColor = o, () => Color.Default, new Color (0, 1, 0)),
			new PropertyTestCase<Button, string> ("FontFamily", v => v.FontFamily, (v, o) => v.FontFamily = o, () => null, "TestingFace"),
			new PropertyTestCase<Button, double> ("FontSize", v => v.FontSize, (v, o) => v.FontSize = o, () => Device.GetNamedSize (NamedSize.Default, typeof (Button), true), 123.0),
			new PropertyTestCase<Button, FontAttributes> ("FontAttributes", v => v.FontAttributes, (v, o) => v.FontAttributes = o, () => FontAttributes.None, FontAttributes.Italic),
			new PropertyTestCase<CellTests.TestCell, double> ("Height", v => v.Height, (v, o) => v.Height = o, () => -1, 10),
			new PropertyTestCase<DatePicker, DateTime> ("MinimumDate", v => v.MinimumDate, (v, o) => v.MinimumDate = o, () => new DateTime (1900, 1, 1), new DateTime (2014, 02, 05)),
			new PropertyTestCase<DatePicker, DateTime> ("MaximumDate", v => v.MaximumDate, (v, o) => v.MaximumDate = o, () => new DateTime (2100, 12, 31), new DateTime (2014, 02, 05)),
			new PropertyTestCase<DatePicker, DateTime> ("Date", v => v.Date, (v, o) => v.Date = o, () => DateTime.Now.Date, new DateTime (2008, 5, 5)),
			new PropertyTestCase<DatePicker, string> ("Format", v => v.Format, (v, o) => v.Format = o, () => "d", "D"),
			new PropertyTestCase<Editor, string> ("Text", v => v.Text, (v, o) => v.Text = o, () => null, "Foo"),
			new PropertyTestCase<Entry, string> ("Text", v => v.Text, (v, o) => v.Text = o, () => null, "Foo"),
			new PropertyTestCase<Entry, string> ("Placeholder", v => v.Placeholder, (v, o) => v.Placeholder = o, () => null, "Foo"),
			new PropertyTestCase<Entry, bool> ("IsPassword", v => v.IsPassword, (v, o) => v.IsPassword = o, () => false, true),
			new PropertyTestCase<Entry, Color> ("TextColor", v => v.TextColor, (v, o) => v.TextColor = o, () => Color.Default, new Color (0, 1, 0)),
			new PropertyTestCase<Frame, Color> ("BackgroundColor", v => v.BackgroundColor, (v, o) => v.BackgroundColor = o, () => Color.Default, new Color (0, 1, 0)),
			new PropertyTestCase<Frame, Color> ("OutlineColor", v => v.OutlineColor, (v, o) => v.OutlineColor = o, () => Color.Default, new Color (0, 1, 0)),
			new PropertyTestCase<Frame, bool> ("HasShadow", v => v.HasShadow, (v, o) => v.HasShadow = o, () => true, false),
			new PropertyTestCase<Grid, double> ("RowSpacing", v => v.RowSpacing, (v, o) => v.RowSpacing = o, () => 6, 12),
			new PropertyTestCase<Grid, double> ("ColumnSpacing", v => v.ColumnSpacing, (v, o) => v.ColumnSpacing = o, () => 6, 12),
			new PropertyTestCase<NaiveLayout, Thickness> ("Padding", v => v.Padding, (v, o) => v.Padding = o, () => default(Thickness), new Thickness (20, 20, 10, 10)),
			new PropertyTestCase<Image, ImageSource> ("Source", v => v.Source, (v, o) => v.Source = o, () => null, ImageSource.FromFile("Foo")),
			new PropertyTestCase<Image, Aspect> ("Aspect", v => v.Aspect, (v, o) => v.Aspect = o, () => Aspect.AspectFit, Aspect.AspectFill),
			new PropertyTestCase<Image, bool> ("IsOpaque", v => v.IsOpaque, (v, o) => v.IsOpaque = o, () => false, true),
			new PropertyTestCase<Label, string> ("Text", v => v.Text, (v, o) => v.Text = o, () => null, "Foo"),
			new PropertyTestCase<Label, TextAlignment> ("XAlign", v => v.XAlign, (v, o) => v.XAlign = o, () => TextAlignment.Start, TextAlignment.End),
			new PropertyTestCase<Label, TextAlignment> ("YAlign", v => v.YAlign, (v, o) => v.YAlign = o, () => TextAlignment.Start, TextAlignment.End),
			new PropertyTestCase<Label, Color> ("TextColor", v => v.TextColor, (v, o) => v.TextColor = o, () => Color.Default, new Color (0, 1, 0)),
			new PropertyTestCase<Label, LineBreakMode> ("LineBreakMode", v => v.LineBreakMode, (v, o) => v.LineBreakMode = o, () => LineBreakMode.WordWrap, LineBreakMode.TailTruncation),
			new PropertyTestCase<Label, Font> ("Font", v => v.Font, (v, o) => v.Font = o, () => default (Font), Font.SystemFontOfSize (12)),
			new PropertyTestCase<Label, string> ("FontFamily", v => v.FontFamily, (v, o) => v.FontFamily = o, () => null, "TestingFace"),
			new PropertyTestCase<Label, double> ("FontSize", v => v.FontSize, (v, o) => v.FontSize = o, () => Device.GetNamedSize (NamedSize.Default, typeof (Label), true), 123.0),
			new PropertyTestCase<Label, FontAttributes> ("FontAttributes", v => v.FontAttributes, (v, o) => v.FontAttributes = o, () => FontAttributes.None, FontAttributes.Italic),
			new PropertyTestCase<Label, FormattedString> ("FormattedText", v => v.FormattedText, (v, o) => v.FormattedText = o, () => default (FormattedString), new FormattedString()),
			new PropertyTestCase<Map, MapType> ("MapType", v => v.MapType, (v, o) => v.MapType = o, () => MapType.Street, MapType.Satellite),
			new PropertyTestCase<Map, bool> ("IsShowingUser", v => v.IsShowingUser, (v, o) => v.IsShowingUser = o, () => false, true),
			new PropertyTestCase<Map, bool> ("HasScrollEnabled", v => v.HasScrollEnabled, (v, o) => v.HasScrollEnabled = o, () => true, false),
			new PropertyTestCase<Map, bool> ("HasZoomEnabled", v => v.HasZoomEnabled, (v, o) => v.HasZoomEnabled = o, () => true, false),
			new PropertyTestCase<OpenGLView, bool> ("HasRenderLoop", v => v.HasRenderLoop, (v, o) => v.HasRenderLoop = o, () => false, true),
			new PropertyTestCase<Page, ImageSource> ("BackgroundImageSource", v => v.BackgroundImageSource, (v, o) => v.BackgroundImageSource = o, () => null, "Foo"),
			new PropertyTestCase<Page, Color> ("BackgroundColor", v => v.BackgroundColor, (v, o) => v.BackgroundColor = o, () => default(Color), new Color (0, 1, 0)),
			new PropertyTestCase<Page, string> ("Title", v => v.Title, (v, o) => v.Title = o, () => null, "Foo"),
			new PropertyTestCase<Page, bool> ("IsBusy", v => v.IsBusy, (v, o) => v.IsBusy = o, () => false, true),
			new PropertyTestCase<Page, bool> ("IgnoresContainerArea", v => ((IPageController)v).IgnoresContainerArea, (v, o) => ((IPageController)v).IgnoresContainerArea = o, () => false, true),
			new PropertyTestCase<Page, Thickness> ("Padding", v => v.Padding, (v, o) => v.Padding = o, () => default(Thickness), new Thickness (12)),
			new PropertyTestCase<Picker, string> ("Title", v=>v.Title, (v, o) =>v.Title = o, () => null, "FooBar"),
			new PropertyTestCase<Picker, int> ("SelectedIndex", v=>v.SelectedIndex, (v, o) =>v.SelectedIndex = o, () => -1, 2, ()=>new Picker{Items= {"Foo", "Bar", "Baz", "Qux"}}),
			new PropertyTestCase<ProgressBar, double> ("Progress", v => v.Progress, (v, o) => v.Progress = o, () => 0, .5),
			new PropertyTestCase<ProgressBar, Color> ("ProgressColor", v => v.ProgressColor, (v, o) => v.ProgressColor = o, () => Color.Default, new Color (0, 1, 0)),
			new PropertyTestCase<SearchBar, string> ("Placeholder", v => v.Placeholder, (v, o) => v.Placeholder = o, () => null, "Foo"),
			new PropertyTestCase<SearchBar, string> ("Text", v => v.Text, (v, o) => v.Text = o, () => null, "Foo"),
			new PropertyTestCase<Slider, double> ("Minimum", v => v.Minimum, (v, o) => v.Minimum = o, () => 0, .5),
			new PropertyTestCase<Slider, double> ("Maximum", v => v.Maximum, (v, o) => v.Maximum = o, () => 1, .5),
			new PropertyTestCase<Slider, double> ("Value", v => v.Value, (v, o) => v.Value = o, () => 0, .5),
			new PropertyTestCase<StackLayout, StackOrientation> ("Orientation", v => v.Orientation, (v, o) => v.Orientation = o, () => StackOrientation.Vertical, StackOrientation.Horizontal),
			new PropertyTestCase<StackLayout, double> ("Spacing", v => v.Spacing, (v, o) => v.Spacing = o, () => 6, 12),
			new PropertyTestCase<Stepper, double> ("Minimum", v => v.Minimum, (v, o) => v.Minimum = o, () => 0, 50),
			new PropertyTestCase<Stepper, double> ("Maximum", v => v.Maximum, (v, o) => v.Maximum = o, () => 100, 50),
			new PropertyTestCase<Stepper, double> ("Value", v => v.Value, (v, o) => v.Value = o, () => 0, 50),
			new PropertyTestCase<Stepper, double> ("Increment", v => v.Increment, (v, o) => v.Increment = o, () => 1, 2),
			new PropertyTestCase<TableRoot, string> ("Title", v => v.Title, (v, o) => v.Title = o, () => null, "Foo"),
			new PropertyTestCase<TableView, int> ("RowHeight", v => v.RowHeight, (v, o) => v.RowHeight = o, () => -1, 20),
			new PropertyTestCase<TableView, bool> ("HasUnevenRows", v => v.HasUnevenRows, (v, o) => v.HasUnevenRows = o, () => false, true),
			new PropertyTestCase<TableView, TableIntent> ("Intent", v => v.Intent, (v, o) => v.Intent = o, () => TableIntent.Data, TableIntent.Menu),
			new PropertyTestCase<TextCell, string> ("Text", v => v.Text, (v, o) => v.Text = o, () => null, "Foo"),
			new PropertyTestCase<TextCell, string> ("Detail", v => v.Detail, (v, o) => v.Detail = o, () => null, "Foo"),
			new PropertyTestCase<TextCell, Color> ("TextColor", v => v.TextColor, (v, o) => v.TextColor = o, () => Color.Default, new Color (0, 1, 0)),
			new PropertyTestCase<TextCell, Color> ("DetailColor", v => v.DetailColor, (v, o) => v.DetailColor = o, () => Color.Default, new Color (0, 1, 0)),
			new PropertyTestCase<TimePicker, TimeSpan> ("Time", v => v.Time, (v, o) => v.Time = o, () => default(TimeSpan), new TimeSpan (8, 0, 0)),
			new PropertyTestCase<TimePicker, string> ("Format", v => v.Format, (v, o) => v.Format = o, () => "t", "T"),
			new PropertyTestCase<ViewCell, View> ("View", v => v.View, (v, o) => v.View = o, () => null, new View ()),
			new PropertyTestCase<WebView, WebViewSource> ("Source", v => v.Source, (v, o) => v.Source = o, () => null, new UrlWebViewSource { Url = "Foo" }),
			new PropertyTestCase<TapGestureRecognizer, int> ("NumberOfTapsRequired", t => t.NumberOfTapsRequired, (t, o) => t.NumberOfTapsRequired = o, () => 1, 3),
			new PropertyTestCase<TapGestureRecognizer, object> ("CommandParameter", t => t.CommandParameter, (t, o) => t.CommandParameter = o, () => null, "Test"),
			new PropertyTestCase<TapGestureRecognizer, ICommand> ("Command", t => t.Command, (t, o) => t.Command = o, () => null, new Command(()=>{})),
			new PropertyTestCase<MasterDetailPage, bool> ("IsGestureEnabled", md => md.IsGestureEnabled, (md, v) => md.IsGestureEnabled = v, () => true, false),
			new PropertyTestCase<Entry, bool> ("IsReadOnly", v => v.IsReadOnly, (v, o) => v.IsReadOnly = o, () => false, true),
			new PropertyTestCase<Editor, bool> ("IsReadOnly", v => v.IsReadOnly, (v, o) => v.IsReadOnly = o, () => false, true)
		};
#pragma warning restore 0414

		[SetUp]
		public override void Setup()
		{
			base.Setup();
			Device.PlatformServices = new MockPlatformServices();
		}

		[TearDown]
		public override void TearDown()
		{
			base.TearDown();
			Device.PlatformServices = null;
		}

		[Test, TestCaseSource("Properties")]
		public void DefaultValues(PropertyTestCase property)
		{
			var view = property.CreateView();
			Assert.AreEqual(property.ExpectedDefaultValue, property.PropertyGetter(view), property.DebugName);
		}

		[Test, TestCaseSource("Properties")]
		public void Set(PropertyTestCase property)
		{
			var view = property.CreateView();

			bool changed = false;
			view.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == property.Name)
					changed = true;
			};

			var testvalue = property.TestValue;
			property.PropertySetter(view, testvalue);

			Assert.True(changed, property.DebugName);
			Assert.AreEqual(testvalue, property.PropertyGetter(view), property.DebugName);
		}

		[Test, TestCaseSource("Properties")]
		public void DoubleSet(PropertyTestCase property)
		{
			var view = property.CreateView();

			var testvalue = property.TestValue;
			property.PropertySetter(view, testvalue);

			bool changed = false;
			view.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == property.Name)
					changed = true;
			};

			property.PropertySetter(view, testvalue);

			Assert.False(changed, property.DebugName);
			Assert.AreEqual(testvalue, property.PropertyGetter(view), property.DebugName);
		}
	}
}