//
// AsyncValue.cs
//
// Author:
//   Eric Maupin <me@ermau.com>
//
// Copyright (c) 2013-2014 Xamarin, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public sealed class AsyncValue<T> : INotifyPropertyChanged
	{
		readonly T _defaultValue;
		readonly Task<T> _valueTask;
		bool _isRunning = true;

		public AsyncValue(Task<T> valueTask, T defaultValue = default(T))
		{
			_valueTask = valueTask ?? throw new ArgumentNullException(nameof(valueTask));
			_defaultValue = defaultValue;

			TaskScheduler scheduler = TaskScheduler.FromCurrentSynchronizationContext();

			if (_valueTask.IsCompleted)
				IsRunning = false;
			else
				_valueTask.ContinueWith(t => IsRunning = false, scheduler);

			if (_valueTask.Status == TaskStatus.RanToCompletion)
				OnPropertyChanged(nameof(Value));
			else
				_valueTask.ContinueWith(t => OnPropertyChanged(nameof(Value)), CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, scheduler);
		}

		public bool IsRunning
		{
			get { return _isRunning; }
			set
			{
				if (_isRunning == value)
					return;

				_isRunning = value;
				OnPropertyChanged();
			}
		}

		public T Value
		{
			get
			{
				if (_valueTask.Status != TaskStatus.RanToCompletion)
					return _defaultValue;

				return _valueTask.Result;
			}
		}

		public static AsyncValue<T> Null => new AsyncValue<T>(Task.FromResult<T>(default(T)));

		public event PropertyChangedEventHandler PropertyChanged;

		void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/AsyncValueExtensions.xml" path="Type[@FullName='Microsoft.Maui.Controls.Internals.AsyncValueExtensions']/Docs/*" />
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class AsyncValueExtensions
	{
		public static AsyncValue<T> AsAsyncValue<T>(this Task<T> valueTask, T defaultValue = default(T)) =>
			new AsyncValue<T>(valueTask, defaultValue);
	}
}
