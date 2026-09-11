#nullable enable
using System;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Benchmarks
{
	[MemoryDiagnoser]
	public class MauiLoggerBenchmarker
	{
		readonly string _propertyName = "TestProperty";
		readonly InvalidOperationException _exception = new("test");

		[GlobalSetup]
		public void Setup()
		{
			var services = new ServiceCollection();
			var sp = services.BuildServiceProvider();

			var app = new Application();
			var mauiContext = new BenchmarkMauiContext(sp);
			app.Handler = new BenchmarkHandler(mauiContext);
			Application.Current = app;
		}

		[GlobalCleanup]
		public void Cleanup()
		{
			Application.Current = null;
		}

		[Benchmark(Baseline = true)]
		public void OldPattern_AlwaysFormat()
			=> OldStyleLogWarning($"Cannot set the BindableProperty \"{_propertyName}\" because it is readonly.");

		[Benchmark]
		public void MauiLogger_Interpolated()
			=> MauiLogger<BindableObject>.Log(LogLevel.Warning, $"Cannot set the BindableProperty \"{_propertyName}\" because it is readonly.");

		[Benchmark]
		public void MauiLogger_PlainString()
			=> MauiLogger<BindableObject>.Log(LogLevel.Warning, "FormatException");

		[Benchmark]
		public void OldPattern_WithException_AlwaysFormat()
			=> OldStyleLogWarning(_exception, $"Failed for property \"{_propertyName}\"");

		[Benchmark]
		public void MauiLogger_WithException_Interpolated()
			=> MauiLogger<BindableObject>.Log(LogLevel.Warning, _exception, $"Failed for property \"{_propertyName}\"");

		[Benchmark]
		public void MauiLogger_WithException_PlainString()
			=> MauiLogger<BindableObject>.Log(LogLevel.Warning, _exception, "Failed to Navigate Back");

		static void OldStyleLogWarning(string message)
		{
			var logger = Application.Current?.FindMauiContext()?.CreateLogger<BindableObject>();
			if (logger is not null && logger.IsEnabled(LogLevel.Warning))
				logger.LogWarning(message);
		}

		static void OldStyleLogWarning(Exception? exception, string message)
		{
			var logger = Application.Current?.FindMauiContext()?.CreateLogger<BindableObject>();
			if (logger is not null && logger.IsEnabled(LogLevel.Warning))
				logger.LogWarning(exception, message);
		}
	}

	/// <summary>
	/// Benchmarks with a real logger configured at MinimumLevel = Error.
	/// This tests the scenario where an app has logging set up, but Warning
	/// is below the threshold — the handler should skip formatting.
	/// </summary>
	[MemoryDiagnoser]
	public class MauiLoggerWithLoggerMinLevelErrorBenchmarker
	{
		readonly string _propertyName = "TestProperty";

		[GlobalSetup]
		public void Setup()
		{
			var services = new ServiceCollection();
			services.AddLogging(builder =>
			{
				builder.SetMinimumLevel(LogLevel.Error);
				builder.AddConsole();
			});
			var sp = services.BuildServiceProvider();

			var app = new Application();
			var mauiContext = new BenchmarkMauiContext(sp);
			app.Handler = new BenchmarkHandler(mauiContext);
			Application.Current = app;
		}

		[GlobalCleanup]
		public void Cleanup()
		{
			Application.Current = null;
		}

		[Benchmark(Baseline = true)]
		public void OldPattern_LogWarning_AlwaysFormat()
			=> OldStyleLogWarning($"Cannot set the BindableProperty \"{_propertyName}\" because it is readonly.");

		[Benchmark]
		public void MauiLogger_LogWarning_Interpolated()
			=> MauiLogger<BindableObject>.Log(LogLevel.Warning, $"Cannot set the BindableProperty \"{_propertyName}\" because it is readonly.");

		[Benchmark]
		public void MauiLogger_LogWarning_PlainString()
			=> MauiLogger<BindableObject>.Log(LogLevel.Warning, "FormatException");

		[Benchmark]
		public void OldPattern_LogError_AlwaysFormat()
			=> OldStyleLogError($"Cannot set the BindableProperty \"{_propertyName}\" because it is readonly.");

		[Benchmark]
		public void MauiLogger_LogError_Interpolated()
			=> MauiLogger<BindableObject>.Log(LogLevel.Error, $"Cannot set the BindableProperty \"{_propertyName}\" because it is readonly.");

		[Benchmark]
		public void MauiLogger_LogError_PlainString()
			=> MauiLogger<BindableObject>.Log(LogLevel.Error, "An error occurred");

		static void OldStyleLogWarning(string message)
		{
			var logger = Application.Current?.FindMauiContext()?.CreateLogger<BindableObject>();
			if (logger is not null && logger.IsEnabled(LogLevel.Warning))
				logger.LogWarning(message);
		}

		static void OldStyleLogError(string message)
		{
			var logger = Application.Current?.FindMauiContext()?.CreateLogger<BindableObject>();
			if (logger is not null && logger.IsEnabled(LogLevel.Error))
				logger.LogError(message);
		}
	}

	sealed class BenchmarkMauiContext : IMauiContext
	{
		public BenchmarkMauiContext(IServiceProvider services) => Services = services;
		public IServiceProvider Services { get; }
		public IMauiHandlersFactory Handlers => throw new NotSupportedException();
		public Animations.IAnimationManager AnimationManager => throw new NotSupportedException();
	}

	sealed class BenchmarkHandler : IElementHandler
	{
		public BenchmarkHandler(IMauiContext mauiContext) => MauiContext = mauiContext;
		public IMauiContext? MauiContext { get; }
		public IElement? VirtualView => null;
		public object? PlatformView => null;
		public void DisconnectHandler() { }
		public void Invoke(string command, object? args = null) { }
		public void SetMauiContext(IMauiContext mauiContext) { }
		public void SetVirtualView(IElement view) { }
		public void UpdateValue(string property) { }
	}
}
