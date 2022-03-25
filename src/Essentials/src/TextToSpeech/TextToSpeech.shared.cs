#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Debug = System.Diagnostics.Debug;

namespace Microsoft.Maui.Media
{
	public interface ITextToSpeech
	{
		Task<IEnumerable<Locale>> GetLocalesAsync();

		Task SpeakAsync(string text, SpeechOptions? options = default, CancellationToken cancelToken = default);
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/TextToSpeech.xml" path="Type[@FullName='Microsoft.Maui.Essentials.TextToSpeech']/Docs" />
	public static partial class TextToSpeech
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/TextToSpeech.xml" path="//Member[@MemberName='GetLocalesAsync']/Docs" />
		public static Task<IEnumerable<Locale>> GetLocalesAsync() =>
			Default.GetLocalesAsync();

		/// <include file="../../docs/Microsoft.Maui.Essentials/TextToSpeech.xml" path="//Member[@MemberName='SpeakAsync'][1]/Docs" />
		public static Task SpeakAsync(string text, CancellationToken cancelToken = default) =>
			Default.SpeakAsync(text, default, cancelToken);

		/// <include file="../../docs/Microsoft.Maui.Essentials/TextToSpeech.xml" path="//Member[@MemberName='SpeakAsync'][2]/Docs" />
		public static Task SpeakAsync(string text, SpeechOptions? options, CancellationToken cancelToken = default) =>
			Default.SpeakAsync(text, options, cancelToken);

		static ITextToSpeech? defaultImplementation;

		public static ITextToSpeech Default =>
			defaultImplementation ??= new TextToSpeechImplementation();

		internal static void SetDefault(ITextToSpeech? implementation) =>
			defaultImplementation = implementation;

		internal static List<string> SplitSpeak(string text, int max)
		{
			var parts = new List<string>();
			if (text.Length <= max)
			{
				// no need to split
				parts.Add(text);
			}
			else
			{
				var positionbegin = 0;
				var positionend = max;
				var position = positionbegin;

				var p = string.Empty;
				while (position != text.Length)
				{
					while (positionend > positionbegin)
					{
						if (positionend >= text.Length)
						{
							// we just need the rest of it
							p = text.Substring(positionbegin, text.Length - positionbegin);
							parts.Add(p);
							return parts;
						}

						var ch = text[positionend];
						if (char.IsWhiteSpace(ch) || char.IsPunctuation(ch))
						{
							p = text.Substring(positionbegin, positionend - positionbegin);
							break;
						}
						else if (positionend == positionbegin)
						{
							// no whitespace or punctuation found
							// grab the whole buffer (max)
							p = text.Substring(positionbegin, positionbegin + max);
							break;
						}

						positionend--;
					}

					Debug.WriteLine($"p             = {p}");
					Debug.WriteLine($"p.Length      = {p.Length}");
					Debug.WriteLine($"positionbegin = {positionbegin}");
					Debug.WriteLine($"positionend   = {positionend}");
					Debug.WriteLine($"position      = {position}");

					positionbegin = positionbegin + p.Length + 1;
					positionend = positionbegin + max;
					position = positionbegin;

					Debug.WriteLine($"------------------------------");
					Debug.WriteLine($"positionbegin = {positionbegin}");
					Debug.WriteLine($"positionend   = {positionend}");
					Debug.WriteLine($"position      = {position}");

					parts.Add(p);
				}
			}

			return parts;
		}
	}

	partial class TextToSpeechImplementation : ITextToSpeech
	{
		internal const float PitchMax = 2.0f;
		internal const float PitchDefault = 1.0f;
		internal const float PitchMin = 0.0f;

		internal const float VolumeMax = 1.0f;
		internal const float VolumeDefault = 0.5f;
		internal const float VolumeMin = 0.0f;

		SemaphoreSlim? semaphore;

		public Task<IEnumerable<Locale>> GetLocalesAsync() =>
			PlatformGetLocalesAsync();

		public async Task SpeakAsync(string text, SpeechOptions? options = default, CancellationToken cancelToken = default)
		{
			if (string.IsNullOrEmpty(text))
				throw new ArgumentNullException(nameof(text), "Text cannot be null or empty string");

			if (options?.Volume.HasValue ?? false)
			{
				if (options.Volume.Value < VolumeMin || options.Volume.Value > VolumeMax)
					throw new ArgumentOutOfRangeException($"Volume must be >= {VolumeMin} and <= {VolumeMax}");
			}

			if (options?.Pitch.HasValue ?? false)
			{
				if (options.Pitch.Value < PitchMin || options.Pitch.Value > PitchMax)
					throw new ArgumentOutOfRangeException($"Pitch must be >= {PitchMin} and <= {PitchMin}");
			}

			if (semaphore == null)
				semaphore = new SemaphoreSlim(1, 1);

			try
			{
				await semaphore.WaitAsync(cancelToken);
				await PlatformSpeakAsync(text, options, cancelToken);
			}
			finally
			{
				if (semaphore.CurrentCount == 0)
					semaphore.Release();
			}
		}
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/Locale.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Locale']/Docs" />
	public class Locale
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Locale.xml" path="//Member[@MemberName='Language']/Docs" />
		public string Language { get; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/Locale.xml" path="//Member[@MemberName='Country']/Docs" />
		public string Country { get; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/Locale.xml" path="//Member[@MemberName='Name']/Docs" />
		public string Name { get; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/Locale.xml" path="//Member[@MemberName='Id']/Docs" />
		public string Id { get; }

		internal Locale(string language, string country, string name, string id)
		{
			Language = language;
			Country = country;
			Name = name;
			Id = id;
		}
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/SpeechOptions.xml" path="Type[@FullName='Microsoft.Maui.Essentials.SpeechOptions']/Docs" />
	public class SpeechOptions
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/SpeechOptions.xml" path="//Member[@MemberName='Locale']/Docs" />
		public Locale? Locale { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/SpeechOptions.xml" path="//Member[@MemberName='Pitch']/Docs" />
		public float? Pitch { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/SpeechOptions.xml" path="//Member[@MemberName='Volume']/Docs" />
		public float? Volume { get; set; }
	}
}
