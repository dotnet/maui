using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials.Gtk;

/// <summary>
/// The Windows speech engine: SAPI 5 <c>SAPI.SpVoice</c>, driven through its automation (<c>IDispatch</c>) interface so no
/// interop assembly or WinRT projection is needed from plain <c>net10.0</c>.
/// </summary>
/// <remarks>
/// <para>Every utterance uses its own <c>SpVoice</c> on one worker thread: it starts the speech asynchronously, polls
/// <c>WaitUntilDone</c> and, when the token is signalled, purges the queue with <c>Speak("", SVSFPurgeBeforeSpeak)</c>
/// from that same thread (no cross-thread COM calls).</para>
/// <para>SAPI has no pitch property: a pitch is sent as an XML <c>&lt;pitch absmiddle="n"/&gt;</c> prefix.</para>
/// </remarks>
[SupportedOSPlatform("windows")]
[UnconditionalSuppressMessage("Trimming", "IL2077", Justification = "Late-bound calls on COM objects: members are resolved by IDispatch, not by reflection over trimmed managed types.")]
[UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Late-bound calls on COM objects: members are resolved by IDispatch, not by reflection over trimmed managed types.")]
[UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "Late-bound calls on COM objects: members are resolved by IDispatch, not by reflection over trimmed managed types.")]
sealed class SapiSpeech : ISpeechBackend
{
	const string ProgId = "SAPI.SpVoice";
	const int FlagAsync = 1, FlagPurgeBeforeSpeak = 2, FlagIsXml = 8;
	const int PollMilliseconds = 50;

	readonly Type voiceType;

	SapiSpeech(Type voiceType)
	{
		this.voiceType = voiceType;
	}

	/// <summary>Binds SAPI, or returns null when <c>SAPI.SpVoice</c> cannot be created.</summary>
	internal static SapiSpeech? TryCreate()
	{
		if (!OperatingSystem.IsWindows())
			return null;

		try
		{
			var type = Type.GetTypeFromProgID(ProgId);
			if (type is null)
				return null;

			Release(Activator.CreateInstance(type)); // proves the class can be instantiated
			return new SapiSpeech(type);
		}
		catch (Exception ex) when (ex is COMException or TypeLoadException or InvalidCastException or NotSupportedException)
		{
			return null;
		}
	}

	public IReadOnlyList<SpeechVoice> GetVoices()
	{
		var voice = CreateVoice();
		try
		{
			var tokens = Call(voice, "GetVoices")!;
			try
			{
				var count = Convert.ToInt32(Get(tokens, "Count"), CultureInfo.InvariantCulture);
				var list = new List<SpeechVoice>(count);
				for (var i = 0; i < count; i++)
				{
					var token = Call(tokens, "Item", i)!;
					try
					{
						list.Add(ToVoice(token));
					}
					finally
					{
						Release(token);
					}
				}

				return list;
			}
			finally
			{
				Release(tokens);
			}
		}
		finally
		{
			Release(voice);
		}
	}

	public Task SpeakAsync(SpeechRequest request, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		return Task.Factory.StartNew(() => SpeakBlocking(request, cancellationToken), cancellationToken,
			TaskCreationOptions.LongRunning, TaskScheduler.Default);
	}

	void SpeakBlocking(SpeechRequest request, CancellationToken cancellationToken)
	{
		var voice = CreateVoice();
		try
		{
			if (request.Voice is { } chosen)
				SelectVoice(voice, chosen.Id);

			if (request.Rate is float rate)
				Set(voice, "Rate", SpeechScale.Rate(rate, 10));
			if (request.Volume is float volume)
				Set(voice, "Volume", SpeechScale.SapiVolume(volume));

			var flags = FlagAsync;
			var text = request.Text;
			if (request.Pitch is float pitch)
			{
				text = string.Format(CultureInfo.InvariantCulture, "<pitch absmiddle=\"{0}\">{1}</pitch>",
					SpeechScale.Pitch(pitch, 10), SecurityElement.Escape(text));
				flags |= FlagIsXml;
			}

			Call(voice, "Speak", text, flags);
			while (!Convert.ToBoolean(Call(voice, "WaitUntilDone", PollMilliseconds), CultureInfo.InvariantCulture))
			{
				if (cancellationToken.IsCancellationRequested)
				{
					Call(voice, "Speak", string.Empty, FlagPurgeBeforeSpeak);
					break;
				}
			}
		}
		finally
		{
			Release(voice);
		}

		cancellationToken.ThrowIfCancellationRequested();
	}

	static void SelectVoice(object voice, string id)
	{
		var tokens = Call(voice, "GetVoices")!;
		try
		{
			var count = Convert.ToInt32(Get(tokens, "Count"), CultureInfo.InvariantCulture);
			for (var i = 0; i < count; i++)
			{
				var token = Call(tokens, "Item", i)!;
				var selected = false;
				try
				{
					if (string.Equals((string?)Get(token, "Id"), id, StringComparison.OrdinalIgnoreCase))
					{
						Invoke(voice, "Voice", BindingFlags.PutRefDispProperty, token);
						selected = true;
					}
				}
				finally
				{
					Release(token);
				}

				if (selected)
					return;
			}
		}
		finally
		{
			Release(tokens);
		}
	}

	static SpeechVoice ToVoice(object token)
	{
		var id = (string?)Get(token, "Id") ?? string.Empty;
		var name = (string?)Call(token, "GetDescription") ?? id;
		var language = string.Empty;
		try
		{
			// "409" or "409;9": hexadecimal LCIDs, the first is the primary one.
			var lcid = ((string?)Call(token, "GetAttribute", "Language"))?.Split(';')[0];
			if (!string.IsNullOrEmpty(lcid) && int.TryParse(lcid, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var value))
				language = new CultureInfo(value).Name;
		}
		catch (Exception ex) when (ex is COMException or CultureNotFoundException or TargetInvocationException)
		{
			// A voice without a language attribute is still usable.
		}

		return new SpeechVoice(language, name, id);
	}

	object CreateVoice() =>
		Activator.CreateInstance(voiceType) ?? throw new InvalidOperationException("SAPI.SpVoice could not be created.");

	static object? Get(object target, string name) =>
		Invoke(target, name, BindingFlags.GetProperty);

	static void Set(object target, string name, object value) =>
		Invoke(target, name, BindingFlags.SetProperty, value);

	static object? Call(object target, string name, params object[] args) =>
		Invoke(target, name, BindingFlags.InvokeMethod, args);

	static object? Invoke(object target, string name, BindingFlags flags, params object[] args)
	{
		try
		{
			return target.GetType().InvokeMember(name, flags, null, target, args, CultureInfo.InvariantCulture);
		}
		catch (TargetInvocationException ex) when (ex.InnerException is not null)
		{
			throw ex.InnerException;
		}
	}

	static void Release(object? com)
	{
		if (com is not null && Marshal.IsComObject(com))
			Marshal.FinalReleaseComObject(com);
	}
}
