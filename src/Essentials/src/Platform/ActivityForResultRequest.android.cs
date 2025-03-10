using System;
using System.Threading.Tasks;
using AndroidX.Activity;
using AndroidX.Activity.Result;
using AndroidX.Activity.Result.Contract;
using JavaObject = Java.Lang.Object;

namespace Microsoft.Maui.ApplicationModel;

/// <summary>
/// Represents a request for an activity result.
/// Provides a type-safe mechanism for registering and launching 
/// activity result requests using the specified contract and callback.
/// </summary>
/// <typeparam name="TContract">The type of the activity result contract.</typeparam>
/// <typeparam name="TResult">The type of the result returned by the activity.</typeparam>
/// <remarks>
/// <para>
/// <see href="https://developer.android.com/training/basics/intents/result">Google docs</see>
/// </para>
/// This must be unconditionally registered every time our activity is created.
/// </remarks>
internal abstract class ActivityForResultRequest<TContract, TResult>
	where TContract : ActivityResultContract, new()
	where TResult : JavaObject
{
	protected ActivityResultLauncher launcher;
	protected TaskCompletionSource<TResult> tcs = null;

	/// <summary>
	/// Gets a value indicating whether the request is registered.
	/// </summary>
	protected bool IsRegistered => launcher is not null;

	/// <summary>
	/// Registers this request to start an activity for a result.
	/// </summary>
	/// <param name="componentActivity">The component activity to register the request with.</param>
	public void Register(ComponentActivity componentActivity)
	{
		var contract = new TContract();
		var callback = new ActivityResultCallback<TResult>(result => tcs?.SetResult(result));

		launcher = componentActivity.RegisterForActivityResult(contract, callback);
	}

	/// <summary>
	/// Launches the activity result request with the specified input.
	/// </summary>
	/// <typeparam name="T">The type of the input parameter.</typeparam>
	/// <param name="input">The input parameter to launch the request with.</param>
	/// <returns>
	/// A task that represents the asynchronous operation, containing the result of the activity.
	/// </returns>
	public Task<TResult> Launch<T>(T input)
		where T : JavaObject
	{
		tcs = new TaskCompletionSource<TResult>();

		if (!IsRegistered)
		{
			tcs.SetCanceled();
			return tcs.Task;
		}

		try
		{
			launcher.Launch(input);
		}
		catch (Exception ex)
		{
			tcs.SetException(ex);
		}

		return tcs.Task;
	}
}