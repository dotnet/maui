using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using XVisualElement = Xamarin.Forms.VisualElement;

namespace Xamarin.Forms.PlatformConfiguration.TizenSpecific
{
	public interface IVideoOutput
	{
		XVisualElement MediaView { get; }
		View Controller { get; set; }
		VideoOuputType OuputType { get; }
	}

	public enum VideoOuputType
	{
		Overlay,
		Buffer,
	}

	public enum DisplayAspectMode
	{
		Fill,
		AspectFit,
		AspectFill,
		OrignalSize
	}

	public enum PlaybackState
	{
		Stopped,
		Playing,
		Paused
	}

	public interface IMediaPlayer
	{
		PlaybackState State { get; }

		event EventHandler PlaybackPaused;
		event EventHandler PlaybackStarted;
		event EventHandler PlaybackStopped;

		}

	public interface IPlatformMediaPlayer : IDisposable
	{
		bool UsesEmbeddingControls { get; set; }
		bool AutoPlay { get; set; }
		bool AutoStop { get; set; }
		double Volume { get; set; }
		bool IsMuted { get; set; }
		bool IsLooping { get; set; }
		int Position { get; }
		int Duration { get; }
		DisplayAspectMode AspectMode { get; set; }

		event EventHandler PlaybackCompleted;
		event EventHandler PlaybackStarted;
		event EventHandler PlaybackPaused;
		event EventHandler PlaybackStopped;
		event EventHandler UpdateStreamInfo;
		event EventHandler<BufferingProgressUpdatedEventArgs> BufferingProgressUpdated;
		event EventHandler ErrorOccurred;

		void SetDisplay(IVideoOutput output);
		void SetSource(MediaSource source);
		Task<bool> Start();
		void Stop();
		void Pause();
		Task<int> Seek(int ms);
		Task<Stream> GetAlbumArts();
		Task<IDictionary<string, string>> GetMetadata();
		Task<Size> GetVideoSize();
		View GetEmbeddingControlView(IMediaPlayer player);
	}

	public class BufferingProgressUpdatedEventArgs : EventArgs
	{
		public double Progress { get; set; }
	}
}
