using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Essentials.Gtk;
using Microsoft.Maui.Media;
using Xunit;

namespace Microsoft.Maui.Gtk.UnitTests.Essentials;

public class SpeechScaleTests
{
	[Theory]
	[InlineData(0.1f, 100, -100)]
	[InlineData(0.55f, 100, -50)]
	[InlineData(1f, 100, 0)]
	[InlineData(1.5f, 100, 50)]
	[InlineData(2f, 100, 100)]
	[InlineData(0.1f, 10, -10)]
	[InlineData(1f, 10, 0)]
	[InlineData(2f, 10, 10)]
	public void RateMapsOneToZeroAndTheEndsToTheEngineRange(float rate, int max, int expected) =>
		Assert.Equal(expected, SpeechScale.Rate(rate, max));

	[Theory]
	[InlineData(0f, 100, -100)]
	[InlineData(1f, 100, 0)]
	[InlineData(2f, 100, 100)]
	[InlineData(0.5f, 10, -5)]
	[InlineData(2f, 10, 10)]
	public void PitchIsLinearAroundOne(float pitch, int max, int expected) =>
		Assert.Equal(expected, SpeechScale.Pitch(pitch, max));

	[Theory]
	[InlineData(0f, -100)]
	[InlineData(0.5f, 0)]
	[InlineData(1f, 100)]
	public void SsipVolumeCoversMinusHundredToHundred(float volume, int expected) =>
		Assert.Equal(expected, SpeechScale.SsipVolume(volume));

	[Theory]
	[InlineData(0f, 0)]
	[InlineData(0.25f, 25)]
	[InlineData(1f, 100)]
	public void SapiVolumeIsAPercentage(float volume, int expected) =>
		Assert.Equal(expected, SpeechScale.SapiVolume(volume));

	static readonly IReadOnlyList<SpeechVoice> Voices = new[]
	{
		new SpeechVoice("it-IT", "Ada", "ada"),
		new SpeechVoice("en-US", "Bob", "bob"),
		new SpeechVoice("en-GB", "Cy", "cy"),
	};

	[Fact]
	public void MatchPrefersTheIdOverTheLanguage() =>
		Assert.Equal("cy", SpeechScale.Match(Voices, "CY", "it-IT")!.Value.Id);

	[Theory]
	[InlineData("en-GB", "cy")]
	[InlineData("en_gb", "cy")]
	[InlineData("it", "ada")]
	[InlineData("en", "bob")]
	[InlineData("en-AU", "bob")]
	public void MatchFallsBackFromTheFullTagToThePrimaryLanguage(string language, string expectedId) =>
		Assert.Equal(expectedId, SpeechScale.Match(Voices, null, language)!.Value.Id);

	[Theory]
	[InlineData(null, null)]
	[InlineData("", "")]
	[InlineData("fr-FR", "nope")]
	public void MatchReturnsNullWhenNothingFits(string? language, string? id) =>
		Assert.Null(SpeechScale.Match(Voices, id, language));
}

public class TextToSpeechTests
{
	sealed class FakeSpeech : ISpeechBackend
	{
		public List<SpeechVoice> Voices { get; } = new()
		{
			new SpeechVoice("en-GB", "Alice", "alice"),
			new SpeechVoice("it-IT", "Bruno", "bruno"),
		};

		public List<SpeechRequest> Requests { get; } = new();

		public int Active, MaxActive;

		/// <summary>When set, utterances run until this is completed (or cancelled).</summary>
		public TaskCompletionSource? Gate { get; set; }

		public bool Cancelled { get; private set; }

		public IReadOnlyList<SpeechVoice> GetVoices() => Voices;

		public async Task SpeakAsync(SpeechRequest request, CancellationToken cancellationToken)
		{
			lock (Requests)
			{
				Requests.Add(request);
				MaxActive = Math.Max(MaxActive, ++Active);
			}

			try
			{
				if (Gate is { } gate)
				{
					using var reg = cancellationToken.Register(() =>
					{
						Cancelled = true;
						gate.TrySetCanceled(cancellationToken);
					});
					await gate.Task;
				}
			}
			finally
			{
				lock (Requests)
					Active--;
			}
		}
	}

	static GtkTextToSpeech Create(FakeSpeech fake) => new(() => fake);

	[Fact]
	public async Task LocalesComeFromTheEngineVoices()
	{
		var locales = (await Create(new FakeSpeech()).GetLocalesAsync()).ToList();

		Assert.Equal(2, locales.Count);
		Assert.Equal("en-GB", locales[0].Language);
		Assert.Equal("Alice", locales[0].Name);
		Assert.Equal("alice", locales[0].Id);
		Assert.Null(locales[0].Country);
		Assert.Equal("it-IT", locales[1].Language);
	}

	[Fact]
	public async Task WithoutAnEngineEveryCallFailsWithFeatureNotSupported()
	{
		var tts = new GtkTextToSpeech(() => null);

		Assert.False(tts.IsSupported);
		await Assert.ThrowsAsync<FeatureNotSupportedException>(() => tts.GetLocalesAsync());
		await Assert.ThrowsAsync<FeatureNotSupportedException>(() => tts.SpeakAsync("hello"));
	}

	[Fact]
	public async Task OptionsReachTheEngineUnconverted()
	{
		var fake = new FakeSpeech();
		await Create(fake).SpeakAsync("hello", new SpeechOptions { Rate = 1.5f, Pitch = 0.5f, Volume = 0.25f });

		var request = Assert.Single(fake.Requests);
		Assert.Equal("hello", request.Text);
		Assert.Equal(1.5f, request.Rate);
		Assert.Equal(0.5f, request.Pitch);
		Assert.Equal(0.25f, request.Volume);
		Assert.Null(request.Voice);
		Assert.Null(request.Language);
	}

	[Fact]
	public async Task NoOptionsLeavesEverythingToTheEngineDefaults()
	{
		var fake = new FakeSpeech();
		await Create(fake).SpeakAsync("hello");

		var request = Assert.Single(fake.Requests);
		Assert.Null(request.Rate);
		Assert.Null(request.Pitch);
		Assert.Null(request.Volume);
	}

	[Fact]
	public async Task LocaleSelectsTheMatchingVoice()
	{
		var fake = new FakeSpeech();
		var tts = Create(fake);
		var locales = (await tts.GetLocalesAsync()).ToList();

		await tts.SpeakAsync("ciao", new SpeechOptions { Locale = locales[1] });

		Assert.Equal("bruno", fake.Requests.Single().Voice!.Value.Id);
	}

	[Fact]
	public async Task LocaleWithoutAnInstalledVoiceKeepsTheRequestedLanguage()
	{
		var fake = new FakeSpeech();
		var locale = (await Create(fake).GetLocalesAsync()).First();
		fake.Voices.Clear();

		await Create(fake).SpeakAsync("x", new SpeechOptions { Locale = locale });

		var request = fake.Requests.Single();
		Assert.Null(request.Voice);
		Assert.Equal("en-GB", request.Language);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	public async Task EmptyTextIsRejected(string? text) =>
		await Assert.ThrowsAsync<ArgumentNullException>(() => Create(new FakeSpeech()).SpeakAsync(text!));

	[Theory]
	[InlineData(-0.1f, null, null)]
	[InlineData(1.1f, null, null)]
	[InlineData(null, -0.1f, null)]
	[InlineData(null, 2.1f, null)]
	[InlineData(null, null, 0.05f)]
	[InlineData(null, null, 2.1f)]
	public async Task OutOfRangeOptionsAreRejectedBeforeSpeaking(float? volume, float? pitch, float? rate)
	{
		var fake = new FakeSpeech();
		await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
			Create(fake).SpeakAsync("x", new SpeechOptions { Volume = volume, Pitch = pitch, Rate = rate }));
		Assert.Empty(fake.Requests);
	}

	[Fact]
	public async Task RangeBoundariesAreAccepted()
	{
		var fake = new FakeSpeech();
		await Create(fake).SpeakAsync("x", new SpeechOptions { Volume = 0f, Pitch = 2f, Rate = 0.1f });
		await Create(fake).SpeakAsync("x", new SpeechOptions { Volume = 1f, Pitch = 0f, Rate = 2f });

		Assert.Equal(2, fake.Requests.Count);
	}

	[Fact]
	public async Task CancellingARunningUtteranceStopsTheEngineAndCancelsTheTask()
	{
		var fake = new FakeSpeech { Gate = new TaskCompletionSource() };
		using var cts = new CancellationTokenSource();

		var speaking = Create(fake).SpeakAsync("long", null, cts.Token);
		await WaitUntil(() => fake.Requests.Count == 1);
		cts.Cancel();

		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => speaking);
		Assert.True(fake.Cancelled);
	}

	[Fact]
	public async Task AnAlreadyCancelledTokenNeverReachesTheEngine()
	{
		var fake = new FakeSpeech();
		using var cts = new CancellationTokenSource();
		cts.Cancel();

		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Create(fake).SpeakAsync("x", null, cts.Token));
		Assert.Empty(fake.Requests);
	}

	[Fact]
	public async Task UtterancesAreQueuedOneAtATime()
	{
		var fake = new FakeSpeech { Gate = new TaskCompletionSource() };
		var tts = Create(fake);

		var first = tts.SpeakAsync("one");
		var second = tts.SpeakAsync("two");
		await WaitUntil(() => fake.Requests.Count == 1);
		await Task.Delay(50);
		Assert.Single(fake.Requests);

		var gate = fake.Gate;
		fake.Gate = null;
		gate.SetResult();
		await Task.WhenAll(first, second);

		Assert.Equal(new[] { "one", "two" }, fake.Requests.Select(r => r.Text));
		Assert.Equal(1, fake.MaxActive);
	}

	[Fact]
	public async Task CancellingAQueuedUtteranceRemovesItWithoutStoppingTheRunningOne()
	{
		var fake = new FakeSpeech { Gate = new TaskCompletionSource() };
		var tts = Create(fake);
		using var cts = new CancellationTokenSource();

		var first = tts.SpeakAsync("one");
		var queued = tts.SpeakAsync("two", null, cts.Token);
		await WaitUntil(() => fake.Requests.Count == 1);
		cts.Cancel();

		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => queued);
		Assert.False(first.IsCompleted);
		Assert.False(fake.Cancelled);
		Assert.Single(fake.Requests);

		fake.Gate.SetResult();
		await first;
	}

	[Fact]
	public async Task TheQueueRecoversAfterAFailedUtterance()
	{
		var fake = new FakeSpeech { Gate = new TaskCompletionSource() };
		var tts = Create(fake);
		using var cts = new CancellationTokenSource();

		var first = tts.SpeakAsync("one", null, cts.Token);
		await WaitUntil(() => fake.Requests.Count == 1);
		cts.Cancel();
		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => first);

		fake.Gate = null;
		await tts.SpeakAsync("two");

		Assert.Equal(2, fake.Requests.Count);
	}

	static async Task WaitUntil(Func<bool> condition)
	{
		for (var i = 0; i < 200 && !condition(); i++)
			await Task.Delay(10);
		Assert.True(condition());
	}
}

/// <summary>
/// The real engine of the OS (SAPI on Windows, speech-dispatcher on Linux). Silent: volume 0 where speaking is needed.
/// Each test returns early (counts as passed) where no engine is installed, e.g. a Linux box without libspeechd.
/// </summary>
public class NativeSpeechTests
{
	static GtkTextToSpeech? Engine()
	{
		var tts = new GtkTextToSpeech();
		if (tts.IsSupported)
			return tts;

		return null;
	}

	[Fact]
	public async Task TheEngineListsVoices()
	{
		if (Engine() is not { } tts)
			return;

		var locales = (await tts.GetLocalesAsync()).ToList();

		Assert.NotEmpty(locales);
		Assert.All(locales, l => Assert.False(string.IsNullOrEmpty(l.Id)));
	}

	[Fact]
	public async Task SpeakingSilentlyCompletes()
	{
		if (Engine() is not { } tts)
			return;

		var done = tts.SpeakAsync("Test.", new SpeechOptions { Volume = 0f, Rate = 2f });

		Assert.Same(done, await Task.WhenAny(done, Task.Delay(TimeSpan.FromSeconds(30))));
		await done;
	}

	[Fact]
	public async Task CancellingStopsALongUtterance()
	{
		if (Engine() is not { } tts)
			return;

		using var cts = new CancellationTokenSource();
		var text = string.Concat(Enumerable.Repeat("This sentence goes on and on. ", 40));
		var speaking = tts.SpeakAsync(text, new SpeechOptions { Volume = 0f, Rate = 0.5f }, cts.Token);
		await Task.Delay(500);
		cts.Cancel();

		var winner = await Task.WhenAny(speaking, Task.Delay(TimeSpan.FromSeconds(10)));
		Assert.Same(speaking, winner);
		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => speaking);
	}
}
