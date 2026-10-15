using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.HybridWebApps.UI;

namespace Microsoft.Maui.Controls.HybridWebApps;

/// <summary>A per-view provider overlay forwarding business services to the owning host scope.</summary>
/// <remarks>Dispose this overlay before the host scope. Forwarded and supplied instances are never owned here.</remarks>
public sealed class HybridServiceProvider : IServiceProvider, IKeyedServiceProvider, IDisposable, IAsyncDisposable
{
	readonly IServiceProvider _host;
	readonly Dictionary<Type, Func<IServiceProvider, object>> _factories;
	readonly Dictionary<Type, ServiceDescriptor> _overrides;
	readonly HashSet<Type> _borrowed;
	readonly Dictionary<Type, object> _instances = new();
	readonly HashSet<Type> _constructing = new();
	readonly List<object> _owned = new();
	readonly object _gate = new();
	bool _disposed;
	Task? _disposalTask;

	internal HybridServiceProvider(IServiceProvider host, HybridLocalConfiguration configuration)
	{
		_host = host;
		_factories = new(configuration.Factories);
		_borrowed = new(configuration.Borrowed);
		_overrides = configuration.Services.Where(d => !d.IsKeyedService && _factories.ContainsKey(d.ServiceType))
			.GroupBy(d => d.ServiceType).ToDictionary(g => g.Key, g => g.Last());
		foreach (var descriptor in _overrides.Values)
			if (descriptor.Lifetime == ServiceLifetime.Singleton && configuration.PerViewOnly.Contains(descriptor.ServiceType))
				throw new InvalidOperationException($"{descriptor.ServiceType.Name} captures view/session resources and cannot be shared as a global singleton. Use a local typed factory.");
	}

	/// <summary>Resolves local objects with this proxy, contextual objects dynamically, or unowned host services.</summary>
	/// <param name="serviceType">The requested contract.</param>
	/// <returns>The service, or null for an unregistered contract.</returns>
	public object? GetService(Type serviceType)
	{
		ArgumentNullException.ThrowIfNull(serviceType);
		lock (_gate)
		{
			ObjectDisposedException.ThrowIf(_disposed, this);
			if (serviceType == typeof(IServiceProvider) || serviceType == typeof(IKeyedServiceProvider))
				return this;
			if (serviceType == typeof(Page))
				return this.GetRequiredService<HybridViewContext>().Page;
			if (serviceType == typeof(Application))
				return this.GetRequiredService<UI.HybridWindowHost>().Application;
			if (serviceType == typeof(Window) || serviceType == typeof(Microsoft.Maui.IWindow))
				return this.GetRequiredService<HybridViewContext>().Window;
			if (serviceType == typeof(HybridWebView))
				return this.GetRequiredService<HybridViewContext>().WebView;
			if (serviceType == typeof(Microsoft.Maui.Dispatching.IDispatcher))
				return this.GetRequiredService<HybridViewContext>().Page.Dispatcher;
			if (_borrowed.Contains(serviceType) && _host.GetService(serviceType) is { } supplied)
				return supplied;
			if (_instances.TryGetValue(serviceType, out var existing))
				return existing;
			if (!_factories.TryGetValue(serviceType, out var factory))
				return _host.GetService(serviceType);
			if (!_constructing.Add(serviceType))
				throw new InvalidOperationException($"A hybrid-local construction cycle includes {serviceType.Name}.");
			try
			{
				bool owned = !_borrowed.Contains(serviceType);
				object? value;
				if (!owned)
					value = factory(this);
				else if (_overrides.TryGetValue(serviceType, out var descriptor))
				{
					if (descriptor.ImplementationInstance is not null)
					{ value = descriptor.ImplementationInstance; owned = false; }
					else if (descriptor.Lifetime == ServiceLifetime.Singleton)
					{ value = _host.GetService(serviceType); owned = false; }
					else if (descriptor.ImplementationFactory is not null)
						value = descriptor.ImplementationFactory(this);
					else if (descriptor.ImplementationType == serviceType)
						value = factory(this);
					else
						throw new InvalidOperationException($"Use a typed factory for the hybrid-local override of {serviceType.Name}.");
				}
				else
					value = factory(this);
				if (value is null || !serviceType.IsInstanceOfType(value))
					throw new InvalidOperationException($"The hybrid-local factory for {serviceType.Name} returned an invalid instance.");
				_instances.Add(serviceType, value);
				if (owned && !_owned.Contains(value, ReferenceEqualityComparer.Instance))
					_owned.Add(value);
				return value;
			}
			finally { _constructing.Remove(serviceType); }
		}
	}

	/// <summary>Forwards an unowned keyed lookup to the same host scope.</summary>
	/// <param name="serviceType">The requested contract.</param><param name="serviceKey">The DI key.</param>
	/// <returns>The keyed host service, or null.</returns>
	public object? GetKeyedService(Type serviceType, object? serviceKey)
	{
		lock (_gate)
		{
			ObjectDisposedException.ThrowIf(_disposed, this);
			return _host is IKeyedServiceProvider keyed ? keyed.GetKeyedService(serviceType, serviceKey) : null;
		}
	}

	/// <inheritdoc/>
	public object GetRequiredKeyedService(Type serviceType, object? serviceKey) =>
		GetKeyedService(serviceType, serviceKey) ?? throw new InvalidOperationException($"No keyed {serviceType.Name} was registered in the owning scope.");

	List<object> Retire(bool synchronous)
	{
		lock (_gate)
		{
			if (_disposed)
				return new();
			if (_constructing.Count != 0)
				throw new InvalidOperationException("A hybrid provider cannot be disposed while a local target is being constructed.");
			if (synchronous && _owned.Any(v => v is IAsyncDisposable && v is not IDisposable))
				throw new InvalidOperationException("This hybrid provider owns async-only resources. Await DisposeAsync before disposing its host scope.");
			_disposed = true;
			// Cancel document work before disposing targets that may close in-flight native resources.
			var result = _owned.AsEnumerable().Reverse().OrderBy(value => value is HybridAppTargetObject ? 0 : 1).ToList();
			_owned.Clear();
			_instances.Clear();
			return result;
		}
	}

	/// <summary>Disposes only locally constructed synchronous resources once.</summary>
	public void Dispose()
	{
		List<object> owned;
		TaskCompletionSource completion;
		lock (_gate)
		{
			if (_disposalTask is not null)
			{
				if (!_disposalTask.IsCompleted)
					throw new InvalidOperationException("Await the ongoing disposal before disposing the owning scope.");
				return;
			}
			owned = Retire(true);
			completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
			_disposalTask = completion.Task;
		}
		try
		{
			List<Exception>? errors = null;
			foreach (var value in owned)
				try
				{ (value as IDisposable)?.Dispose(); }
				catch (Exception error) { (errors ??= new()).Add(error); }
			if (errors is not null)
				throw new AggregateException(errors);
			completion.SetResult();
		}
		catch (Exception error)
		{
			completion.SetException(error);
			_ = completion.Task.Exception;
			throw;
		}
	}

	/// <summary>Awaits locally owned asynchronous cleanup without disposing forwarded host services.</summary>
	public ValueTask DisposeAsync()
	{
		List<object> owned;
		TaskCompletionSource completion;
		lock (_gate)
		{
			if (_disposalTask is not null)
				return new(_disposalTask);
			owned = Retire(false);
			completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
			_disposalTask = completion.Task;
		}
		_ = DisposeOwnedAsync(owned, completion);
		return new(completion.Task);
	}
	static async Task DisposeOwnedAsync(List<object> owned, TaskCompletionSource completion)
	{
		List<Exception>? errors = null;
		foreach (var value in owned)
			try
			{
				if (value is IAsyncDisposable asynchronous)
					await asynchronous.DisposeAsync();
				else
					(value as IDisposable)?.Dispose();
			}
			catch (Exception error) { (errors ??= new()).Add(error); }
		if (errors is not null)
			completion.SetException(new AggregateException(errors));
		else
			completion.SetResult();
	}
}

internal sealed class HybridLocalConfiguration(IServiceCollection services)
{
	internal IServiceCollection Services { get; } = services;
	internal Dictionary<Type, Func<IServiceProvider, object>> Factories { get; } = new();
	internal HashSet<Type> Borrowed { get; } = new();
	internal HashSet<Type> PerViewOnly { get; } = new();
	internal Dictionary<Type, Action<CommandRegistry, IServiceProvider>> Assemblies { get; } = new();
}
