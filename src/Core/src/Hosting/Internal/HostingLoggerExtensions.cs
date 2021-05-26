#nullable enable
using System;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui.Hosting.Internal
{
	static class HostingLoggerExtensions
	{
		public static void ApplicationError(this ILogger logger, EventId eventId, string message, Exception exception)
		{
			if (exception is ReflectionTypeLoadException reflectionTypeLoadException)
			{
				foreach (Exception? ex in reflectionTypeLoadException.LoaderExceptions)
				{
					message = $"{message}{Environment.NewLine}{ex?.Message}";
				}
			}

			logger.LogCritical(
				eventId: eventId,
				message: message,
				exception: exception);
		}

		public static void Starting(this ILogger logger)
		{
			if (logger.IsEnabled(LogLevel.Debug))
			{
				logger.LogDebug(
					eventId: LoggerEventIds.Starting,
					message: "Hosting starting");
			}
		}

		public static void Started(this ILogger logger)
		{
			if (logger.IsEnabled(LogLevel.Debug))
			{
				logger.LogDebug(
					eventId: LoggerEventIds.Started,
					message: "Hosting started");
			}
		}

		public static void Stopping(this ILogger logger)
		{
			if (logger.IsEnabled(LogLevel.Debug))
			{
				logger.LogDebug(
					eventId: LoggerEventIds.Stopping,
					message: "Hosting stopping");
			}
		}

		public static void Stopped(this ILogger logger)
		{
			if (logger.IsEnabled(LogLevel.Debug))
			{
				logger.LogDebug(
					eventId: LoggerEventIds.Stopped,
					message: "Hosting stopped");
			}
		}

		public static void StoppedWithException(this ILogger logger, Exception ex)
		{
			if (logger.IsEnabled(LogLevel.Debug))
			{
				logger.LogDebug(
					eventId: LoggerEventIds.StoppedWithException,
					exception: ex,
					message: "Hosting shutdown exception");
			}
		}

		public static void BackgroundServiceFaulted(this ILogger logger, Exception ex)
		{
			if (logger.IsEnabled(LogLevel.Error))
			{
				logger.LogError(
					eventId: LoggerEventIds.BackgroundServiceFaulted,
					exception: ex,
					message: "BackgroundService failed");
			}
		}
	}

	static class LoggerEventIds
	{
		public static readonly EventId Starting = new EventId(1, "Starting");
		public static readonly EventId Started = new EventId(2, "Started");
		public static readonly EventId Stopping = new EventId(3, "Stopping");
		public static readonly EventId Stopped = new EventId(4, "Stopped");
		public static readonly EventId StoppedWithException = new EventId(5, "StoppedWithException");
		public static readonly EventId ApplicationStartupException = new EventId(6, "ApplicationStartupException");
		public static readonly EventId ApplicationStoppingException = new EventId(7, "ApplicationStoppingException");
		public static readonly EventId ApplicationStoppedException = new EventId(8, "ApplicationStoppedException");
		public static readonly EventId BackgroundServiceFaulted = new EventId(9, "BackgroundServiceFaulted");
	}
}