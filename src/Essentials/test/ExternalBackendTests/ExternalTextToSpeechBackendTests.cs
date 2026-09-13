#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Media;
using Xunit;

namespace Microsoft.Maui.Essentials.ExternalBackend.UnitTests
{
	/// <summary>
	/// Stands in for an out-of-tree platform backend (for example a community Tizen
	/// backend). It is compiled into an assembly that has no <c>InternalsVisibleTo</c>
	/// access to Microsoft.Maui.Essentials, so it can only use the public API surface.
	/// </summary>
	sealed class ExternalTextToSpeech : ITextToSpeech
	{
		public SpeechOptions? LastOptions { get; private set; }

		public Task<IEnumerable<Locale>> GetLocalesAsync() =>
			Task.FromResult<IEnumerable<Locale>>(new[]
			{
				new Locale("en", "US", "English (United States)", "external:en-US"),
				new Locale("ko", "KR", "Korean (Korea)", "external:ko-KR"),

				// A backend that can only report a language uses null for the rest.
				new Locale("fr", null, null, null),
			});

		public Task SpeakAsync(string text, SpeechOptions? options = default, CancellationToken cancelToken = default)
		{
			LastOptions = options;
			return Task.CompletedTask;
		}
	}

	public class ExternalTextToSpeechBackendTests
	{
		[Fact]
		public void LocaleConstructorIsAccessibleOutsideEssentials()
		{
			var locale = new Locale("en", "US", "English (United States)", "external:en-US");

			Assert.Equal("en", locale.Language);
			Assert.Equal("US", locale.Country);
			Assert.Equal("English (United States)", locale.Name);
			Assert.Equal("external:en-US", locale.Id);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		public void NullAndEmptyValuesNormalizeToEmptyString(string? value)
		{
			var locale = new Locale(value, value, value, value);

			Assert.Equal(string.Empty, locale.Language);
			Assert.Equal(string.Empty, locale.Country);
			Assert.Equal(string.Empty, locale.Name);
			Assert.Equal(string.Empty, locale.Id);
		}

		[Fact]
		public void ValuesAreNotTrimmedOrCaseNormalized()
		{
			var locale = new Locale("EN", "us", " Display ", "Id");

			Assert.Equal("EN", locale.Language);
			Assert.Equal("us", locale.Country);
			Assert.Equal(" Display ", locale.Name);
			Assert.Equal("Id", locale.Id);
		}

		[Fact]
		public async Task ExternalBackendCanReturnLocalesFromGetLocalesAsync()
		{
			ITextToSpeech tts = new ExternalTextToSpeech();

			var locales = (await tts.GetLocalesAsync()).ToList();

			Assert.Equal(3, locales.Count);
			Assert.Contains(locales, l => l.Language == "en" && l.Country == "US");
			Assert.Contains(locales, l => l.Language == "ko" && l.Id == "external:ko-KR");

			// Language-only locales still expose non-null property values.
			var languageOnly = Assert.Single(locales, l => l.Language == "fr");
			Assert.Equal(string.Empty, languageOnly.Country);
			Assert.Equal(string.Empty, languageOnly.Name);
			Assert.Equal(string.Empty, languageOnly.Id);
		}

		[Fact]
		public async Task ExternalBackendCanRoundTripLocaleThroughSpeechOptions()
		{
			var backend = new ExternalTextToSpeech();
			ITextToSpeech tts = backend;

			var locale = (await tts.GetLocalesAsync()).First();

			await tts.SpeakAsync("hello", new SpeechOptions { Locale = locale, Rate = 1.0f });

			Assert.NotNull(backend.LastOptions);
			Assert.Same(locale, backend.LastOptions!.Locale);
			Assert.Equal("en", backend.LastOptions.Locale!.Language);
		}
	}
}
