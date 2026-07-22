// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload.AiAssisted;

[Collection("XamlHotReloadTests")]
public partial class BindingAndMarkupHotReloadTests
{
	const string PageClass = "TestAiAssisted.MainPage";

	const string PageStub = """
		namespace TestAiAssisted;

		public partial class MainPage : global::Microsoft.Maui.Controls.ContentPage
		{
			private partial void InitializeComponent();

			public MainPage()
			{
				InitializeComponent();
			}
		}
		""";

	const string BindingAndMarkupSource = """
		using System;
		using System.ComponentModel;
		using System.Globalization;
		using Microsoft.Maui.Controls;
		using Microsoft.Maui.Controls.Xaml;

		namespace TestAiAssisted;

		public sealed class TestViewModel
		{
			public string Text { get; set; } = "VM";
		}

		public sealed class Row : INotifyPropertyChanged
		{
			string _a = string.Empty;
			string _b = string.Empty;

			public string A
			{
				get => _a;
				set
				{
					if (_a == value)
						return;
					_a = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(A)));
				}
			}

			public string B
			{
				get => _b;
				set
				{
					if (_b == value)
						return;
					_b = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(B)));
				}
			}

			public event PropertyChangedEventHandler? PropertyChanged;
		}

		public sealed class JoinConverter : IMultiValueConverter
		{
			public int ConvertCount { get; private set; }

			public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
			{
				ConvertCount++;
				var result = $"{values.Length}:";
				for (var index = 0; index < values.Length; index++)
				{
					if (index > 0)
						result += "|";
					result += values[index]?.ToString() ?? "<null>";
				}
				return result;
			}

			public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
				new object[targetTypes.Length];
		}

		public sealed class ShoutExtension : IMarkupExtension<string>
		{
			public string Text { get; set; } = string.Empty;

			public string ProvideValue(IServiceProvider serviceProvider) => Text.ToUpperInvariant();

			object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);
		}
		""";

	static XamlHotReloadTestHarness CreateHarness([CallerMemberName] string scenarioName = "") =>
		new(scenarioName, PageClass, PageStub, BindingAndMarkupSource);

	static TextRegistration GetTextRegistration(Label label)
	{
		var propertiesField = typeof(BindableObject).GetField("_properties", BindingFlags.Instance | BindingFlags.NonPublic);
		Assert.NotNull(propertiesField);

		var properties = Assert.IsAssignableFrom<IDictionary>(propertiesField!.GetValue(label));
		foreach (DictionaryEntry entry in properties)
		{
			var context = entry.Value;
			if (context is null)
				continue;

			var contextType = context.GetType();
			var property = contextType.GetField("Property")?.GetValue(context);
			if (!ReferenceEquals(property, Label.TextProperty))
				continue;

			var attributes = contextType.GetField("Attributes")?.GetValue(context)?.ToString() ?? string.Empty;
			var bindings = contextType.GetField("Bindings")?.GetValue(context);
			Assert.NotNull(bindings);

			var bindingCount = (int)bindings!.GetType().GetProperty("Count")!.GetValue(bindings)!;
			var binding = bindings.GetType().GetMethod("GetValue")!.Invoke(bindings, null) as BindingBase;
			return new TextRegistration(binding, bindingCount, attributes.Contains("IsDynamicResource", StringComparison.Ordinal));
		}

		throw new Xunit.Sdk.XunitException("The Label.Text property had no bindable-property context.");
	}

	readonly record struct TextRegistration(BindingBase? Binding, int BindingCount, bool IsDynamicResource);
}
