using System;
using System.Linq;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload;

[Collection("XamlHotReloadTests")]
public class Maui40733Tests
{
	const string PageSource = """
		using System.Collections.Generic;
		using System.Collections.ObjectModel;
		using Microsoft.Maui.Controls;

		namespace Test40733;

		public partial class MainPage : ContentPage
		{
			public MainPage() => InitializeComponent();
		}

		[ContentProperty(nameof(Rows))]
		public class FormSection : ContentView
		{
			public ObservableCollection<View> Rows { get; } = [];
		}

		[ContentProperty(nameof(Rows))]
		public class SettableFormSection : ContentView
		{
			public IList<View> Rows { get; set; } = new List<View>();
		}

		[ContentProperty(nameof(Body))]
		public class ReadOnlyBody : ContentView
		{
			public View Body { get; } = new Label();
		}

		[ContentProperty(nameof(Body))]
		public class BodyBase : ContentView
		{
			public virtual View Body { get; set; }
		}

		public class GetterOverride : BodyBase
		{
			public override View Body => base.Body;
		}

		public interface IRow { }

		public class RowLabel : Label, IRow { }

		public class RowCollection : IEnumerable<View>
		{
			readonly List<View> _items = [];
			public void Add(View view) => _items.Add(view);
			public void Add(IRow row) => _items.Add((View)row);
			public IEnumerator<View> GetEnumerator() => _items.GetEnumerator();
			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
		}

		[ContentProperty(nameof(Rows))]
		public class RowSection : ContentView
		{
			public RowCollection Rows { get; } = new();
		}

		public class ElementCollection : ICollection<View>
		{
			readonly List<View> _items = [];
			public void Add(Element element) => _items.Add((View)element);
			void ICollection<View>.Add(View view) => throw new System.InvalidOperationException();
			public int Count => _items.Count;
			public bool IsReadOnly => false;
			public void Clear() => _items.Clear();
			public bool Contains(View item) => _items.Contains(item);
			public void CopyTo(View[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);
			public bool Remove(View item) => _items.Remove(item);
			public IEnumerator<View> GetEnumerator() => _items.GetEnumerator();
			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
		}

		[ContentProperty(nameof(Rows))]
		public class ElementSection : ContentView
		{
			public ElementCollection Rows { get; } = new();
		}
		""";

	static string Page(string sections) => $$"""
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             xmlns:local="clr-namespace:Test40733"
		             x:Class="Test40733.MainPage">
		    <VerticalStackLayout>
		        <local:FormSection>
		            <Label Text="Row 1" />
		        </local:FormSection>
		{{sections}}
		    </VerticalStackLayout>
		</ContentPage>
		""";

	const string AddedSection = """
		        <local:FormSection>
		            <Label Text="A" />
		            <Label Text="B" />
		        </local:FormSection>
		""";

	[Fact]
	public void AddingElementWithGetterOnlyCollectionContentProperty_AddsEveryChild()
	{
		using var harness = new XamlHotReloadTestHarness(
			nameof(AddingElementWithGetterOnlyCollectionContentProperty_AddsEveryChild), "Test40733.MainPage", PageSource);
		var generation = harness.Generate(Page(""), Page(AddedSection));

		var update = generation[1].UpdateComponentSource;
		Assert.NotNull(update);
		Assert.DoesNotContain(".Rows =", update, StringComparison.Ordinal);
		Assert.Contains("Text = \"A\"", update, StringComparison.Ordinal);
		Assert.Contains("Text = \"B\"", update, StringComparison.Ordinal);
		harness.Compile(generation[1]);
	}

	[Fact]
	public void AddingElementWithSettableCollectionContentProperty_AddsEveryChild()
	{
		using var harness = new XamlHotReloadTestHarness(
			nameof(AddingElementWithSettableCollectionContentProperty_AddsEveryChild), "Test40733.MainPage", PageSource);
		var generation = harness.Generate(Page(""), Page(AddedSection.Replace("local:FormSection", "local:SettableFormSection", StringComparison.Ordinal)));

		var update = generation[1].UpdateComponentSource;
		Assert.NotNull(update);
		Assert.DoesNotContain(".Rows =", update, StringComparison.Ordinal);
		Assert.Contains("Text = \"B\"", update, StringComparison.Ordinal);
		harness.Compile(generation[1]);
	}

	[Fact]
	public void AddingElementWithGetterOnlyNonCollectionContentProperty_CompilesWithoutAssigningIt()
	{
		using var harness = new XamlHotReloadTestHarness(
			nameof(AddingElementWithGetterOnlyNonCollectionContentProperty_CompilesWithoutAssigningIt), "Test40733.MainPage", PageSource);
		var generation = harness.Generate(Page(""), Page("""
			        <local:ReadOnlyBody>
			            <Label Text="A" />
			        </local:ReadOnlyBody>
			"""));

		AssertPatchDiscarded(generation[1].UpdateComponentSource);
		harness.Compile(generation[1]);
	}

	[Theory]
	[InlineData("RowSection", "View")]
	[InlineData("ElementSection", "Element")]
	public void AddingChildMatchingSeveralAddOverloads_BindsTheOverloadInitializeComponentUses(string section, string parameterType)
	{
		using var harness = new XamlHotReloadTestHarness(
			nameof(AddingChildMatchingSeveralAddOverloads_BindsTheOverloadInitializeComponentUses), "Test40733.MainPage", PageSource);
		var generation = harness.Generate(Page(""), Page($$"""
			        <local:{{section}}>
			            <local:RowLabel Text="A" />
			        </local:{{section}}>
			"""));

		Assert.Contains($".Rows.Add((global::Microsoft.Maui.Controls.{parameterType})", generation[1].UpdateComponentSource, StringComparison.Ordinal);
		harness.Compile(generation[1]);
	}

	[Theory]
	[InlineData("""<Label Text="Row 1" />""", """<Button Text="Row 1" />""")]
	[InlineData("""<Label Text="Row 1" />""", """<Label Text="Row 1" /><Label Text="Row 2" />""")]
	[InlineData("""<Label Text="Row 1" /><Label Text="Row 2" />""", """<Label Text="Row 1" />""")]
	[InlineData("""<Label Text="Row 1" />""", "")]
	public void ChangingChildrenOfExistingSettableCollectionContentProperty_DiscardsPatch(string before, string after)
	{
		static string Xaml(string rows) => $$"""
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             xmlns:local="clr-namespace:Test40733"
			             x:Class="Test40733.MainPage">
			    <local:SettableFormSection>{{rows}}</local:SettableFormSection>
			</ContentPage>
			""";
		using var harness = new XamlHotReloadTestHarness(
			nameof(ChangingChildrenOfExistingSettableCollectionContentProperty_DiscardsPatch), "Test40733.MainPage", PageSource);
		var generation = harness.Generate(Xaml(before), Xaml(after));

		AssertPatchDiscarded(generation[1].UpdateComponentSource);
		harness.Compile(generation[1]);
	}

	[Fact]
	public void AddingElementWithGetterOnlyOverrideOfSettableContentProperty_AssignsInheritedSetter()
	{
		using var harness = new XamlHotReloadTestHarness(
			nameof(AddingElementWithGetterOnlyOverrideOfSettableContentProperty_AssignsInheritedSetter), "Test40733.MainPage", PageSource);
		var generation = harness.Generate(Page(""), Page("""
			        <local:GetterOverride>
			            <Label Text="A" />
			        </local:GetterOverride>
			"""));

		Assert.Contains(".Body = ", generation[1].UpdateComponentSource, StringComparison.Ordinal);
		harness.Compile(generation[1]);
	}

	// A patch that can't be represented falls back to a structural reset: an empty UpdateComponent().
	static void AssertPatchDiscarded(string? update)
	{
		Assert.NotNull(update);
		Assert.Contains("Fingerprint() => \"\";", update, StringComparison.Ordinal);
	}

	[MetadataUpdateFact]
	public void AddingElementWithGetterOnlyCollectionContentProperty_UpdatesLivePage()
	{
		using var harness = new XamlHotReloadTestHarness(
			nameof(AddingElementWithGetterOnlyCollectionContentProperty_UpdatesLivePage), "Test40733.MainPage", PageSource);
		var generation = harness.Generate(Page(""), Page(AddedSection));

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var layout = Assert.IsType<VerticalStackLayout>(page.Content);
			var existing = Assert.Single(layout.Children);

			live.ApplyUpdate<ContentPage>(1);

			Assert.Equal(2, layout.Children.Count);
			Assert.Same(existing, layout.Children[0]);
			var rows = (System.Collections.Generic.IEnumerable<View>)layout.Children[1].GetType().GetProperty("Rows")!.GetValue(layout.Children[1])!;
			Assert.Equal(["A", "B"], rows.Cast<Label>().Select(label => label.Text));
		});
	}
}
