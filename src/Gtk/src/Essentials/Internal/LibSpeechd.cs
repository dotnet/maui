using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Essentials.Gtk;

/// <summary>
/// speech-dispatcher (<c>libspeechd.so.2</c>) bound at run time by symbol lookup, so a missing library only disables speech.
/// </summary>
/// <remarks>
/// <para>Each utterance opens its own <c>SPDConnection</c> (threaded mode): voice, rate, pitch and volume are per connection,
/// so nothing leaks into the next utterance. The end and cancel notifications are delivered on the library's event thread
/// through function pointers written into the public head of the struct
/// (<c>{ callback_begin; callback_end; callback_cancel; ... }</c>, one pointer each); they complete the utterance's task, which
/// is looked up by the connection's client id.</para>
/// <para>Cancelling the token calls <c>spd_cancel</c> on the connection. The connection is closed from the awaiting
/// continuation, never from the callback (<c>spd_close</c> joins the event thread).</para>
/// </remarks>
sealed class LibSpeechd : ISpeechBackend
{
	static readonly string[] LibNames = { "libspeechd.so.2", "libspeechd.so" };

	const int ModeThreaded = 1;
	const int PriorityText = 3;
	const int NotifyEnd = 2, NotifyCancel = 8;
	const int EventEnd = 1, EventCancel = 3;
	const int EndCallbackOffset = 8, CancelCallbackOffset = 16; // SPDConnection { begin, end, cancel, ... }

	/// <summary>Utterances waiting for their end/cancel notification, keyed by connection client id.</summary>
	static readonly ConcurrentDictionary<nuint, TaskCompletionSource> Pending = new();

	readonly unsafe delegate* unmanaged[Cdecl]<nint, nint, nint, int, nint> open;
	readonly unsafe delegate* unmanaged[Cdecl]<nint, void> close;
	readonly unsafe delegate* unmanaged[Cdecl]<nint, int> getClientId;
	readonly unsafe delegate* unmanaged[Cdecl]<nint, int, nint, int> say;
	readonly unsafe delegate* unmanaged[Cdecl]<nint, int> cancel;
	readonly unsafe delegate* unmanaged[Cdecl]<nint, int, int> setNotificationOn;
	readonly unsafe delegate* unmanaged[Cdecl]<nint, nint, int> setLanguage;
	readonly unsafe delegate* unmanaged[Cdecl]<nint, nint, int> setSynthesisVoice;
	readonly unsafe delegate* unmanaged[Cdecl]<nint, int, int> setRate;
	readonly unsafe delegate* unmanaged[Cdecl]<nint, int, int> setPitch;
	readonly unsafe delegate* unmanaged[Cdecl]<nint, int, int> setVolume;
	readonly unsafe delegate* unmanaged[Cdecl]<nint, nint> listVoices;
	readonly unsafe delegate* unmanaged[Cdecl]<nint, void> freeVoices;

	unsafe LibSpeechd()
	{
		var lib = Open();
		open = (delegate* unmanaged[Cdecl]<nint, nint, nint, int, nint>)Symbol(lib, "spd_open");
		close = (delegate* unmanaged[Cdecl]<nint, void>)Symbol(lib, "spd_close");
		getClientId = (delegate* unmanaged[Cdecl]<nint, int>)Symbol(lib, "spd_get_client_id");
		say = (delegate* unmanaged[Cdecl]<nint, int, nint, int>)Symbol(lib, "spd_say");
		cancel = (delegate* unmanaged[Cdecl]<nint, int>)Symbol(lib, "spd_cancel");
		setNotificationOn = (delegate* unmanaged[Cdecl]<nint, int, int>)Symbol(lib, "spd_set_notification_on");
		setLanguage = (delegate* unmanaged[Cdecl]<nint, nint, int>)Symbol(lib, "spd_set_language");
		setSynthesisVoice = (delegate* unmanaged[Cdecl]<nint, nint, int>)Symbol(lib, "spd_set_synthesis_voice");
		setRate = (delegate* unmanaged[Cdecl]<nint, int, int>)Symbol(lib, "spd_set_voice_rate");
		setPitch = (delegate* unmanaged[Cdecl]<nint, int, int>)Symbol(lib, "spd_set_voice_pitch");
		setVolume = (delegate* unmanaged[Cdecl]<nint, int, int>)Symbol(lib, "spd_set_volume");
		listVoices = (delegate* unmanaged[Cdecl]<nint, nint>)Symbol(lib, "spd_list_synthesis_voices");
		freeVoices = (delegate* unmanaged[Cdecl]<nint, void>)Symbol(lib, "free_spd_voices");
	}

	/// <summary>Binds libspeechd, or returns null when it is not installed.</summary>
	internal static LibSpeechd? TryCreate()
	{
		if (!OperatingSystem.IsLinux())
			return null;

		try
		{
			return new LibSpeechd();
		}
		catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException)
		{
			return null;
		}
	}

	public unsafe IReadOnlyList<SpeechVoice> GetVoices()
	{
		var connection = Connect();
		try
		{
			var voices = new List<SpeechVoice>();
			var list = listVoices(connection); // SPDVoice**: null terminated; SPDVoice { char* name; char* language; char* variant; }
			if (list == 0)
				return voices;

			try
			{
				for (var i = 0; ; i++)
				{
					var voice = Marshal.ReadIntPtr(list, i * sizeof(nint));
					if (voice == 0)
						break;

					var name = Marshal.PtrToStringUTF8(Marshal.ReadIntPtr(voice, 0)) ?? string.Empty;
					var language = Marshal.PtrToStringUTF8(Marshal.ReadIntPtr(voice, sizeof(nint))) ?? string.Empty;
					voices.Add(new SpeechVoice(SpeechScale.Normalize(language), name, name));
				}
			}
			finally
			{
				freeVoices(list);
			}

			return voices;
		}
		finally
		{
			close(connection);
		}
	}

	public async Task SpeakAsync(SpeechRequest request, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var done = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		nint connection = 0;
		nuint client = 0;
		try
		{
			(connection, client) = await Task.Run(() => Start(request, done)).ConfigureAwait(false);

			var started = connection;
			using (cancellationToken.Register(() =>
			{
				CancelNative(started);
				done.TrySetCanceled(cancellationToken);
			}))
			{
				await done.Task.ConfigureAwait(false);
			}
		}
		finally
		{
			if (client != 0)
				Pending.TryRemove(client, out _);

			if (connection != 0)
			{
				var toClose = connection;
				await Task.Run(() => CloseNative(toClose)).ConfigureAwait(false);
			}
		}
	}

	unsafe void CancelNative(nint connection) => cancel(connection);

	unsafe void CloseNative(nint connection) => close(connection);

	/// <summary>Opens a connection, applies the options, registers the completion and queues the text.</summary>
	unsafe (nint Connection, nuint Client) Start(SpeechRequest request, TaskCompletionSource done)
	{
		var connection = Connect();
		var client = (nuint)getClientId(connection);
		try
		{
			Marshal.WriteIntPtr(connection, EndCallbackOffset, (nint)(delegate* unmanaged[Cdecl]<nuint, nuint, int, void>)&OnEvent);
			Marshal.WriteIntPtr(connection, CancelCallbackOffset, (nint)(delegate* unmanaged[Cdecl]<nuint, nuint, int, void>)&OnEvent);
			setNotificationOn(connection, NotifyEnd | NotifyCancel);

			var language = request.Voice?.Language;
			if (string.IsNullOrEmpty(language))
				language = request.Language;
			if (!string.IsNullOrEmpty(language))
				WithUtf8(language!, p => setLanguage(connection, p));
			if (request.Voice is { } voice)
				WithUtf8(voice.Id, p => setSynthesisVoice(connection, p));

			if (request.Rate is float rate)
				setRate(connection, SpeechScale.Rate(rate, 100));
			if (request.Pitch is float pitch)
				setPitch(connection, SpeechScale.Pitch(pitch, 100));
			if (request.Volume is float volume)
				setVolume(connection, SpeechScale.SsipVolume(volume));

			Pending[client] = done;
			var sent = WithUtf8(request.Text, p => say(connection, PriorityText, p));
			if (sent < 0)
				throw new InvalidOperationException("speech-dispatcher rejected the text.");

			return (connection, client);
		}
		catch
		{
			Pending.TryRemove(client, out _);
			close(connection);
			throw;
		}
	}

	[UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
	static unsafe void OnEvent(nuint messageId, nuint clientId, int type)
	{
		if ((type == EventEnd || type == EventCancel) && Pending.TryGetValue(clientId, out var done))
			done.TrySetResult();
	}

	unsafe nint Connect()
	{
		var connection = WithUtf8("maui", client => WithUtf8("speech", name => open(client, name, client, ModeThreaded)));
		if (connection == 0)
			throw new FeatureNotSupportedException("The speech-dispatcher daemon could not be reached.");

		return connection;
	}

	static T WithUtf8<T>(string value, Func<nint, T> use)
	{
		var ptr = Marshal.StringToCoTaskMemUTF8(value);
		try
		{
			return use(ptr);
		}
		finally
		{
			Marshal.FreeCoTaskMem(ptr);
		}
	}

	static nint Open()
	{
		foreach (var name in LibNames)
		{
			if (NativeLibrary.TryLoad(name, out var handle))
				return handle;
		}

		throw new DllNotFoundException(LibNames[0]);
	}

	static nint Symbol(nint library, string name) =>
		NativeLibrary.GetExport(library, name);
}
