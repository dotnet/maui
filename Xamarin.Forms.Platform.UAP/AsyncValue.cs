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

namespace Xamarin.Forms.Platform.UWP
{
	internal sealed class AsyncValue<T> : INotifyPropertyChanged
	{
		readonly T _defaultValue;
		readonly Task<T> _valueTask;
		bool _isRunning = true;

		public AsyncValue(Task<T> valueTask, T defaultValue)
		{
			if (valueTask == null)
				throw new ArgumentNullException("valueTask");

			_valueTask = valueTask;
			_defaultValue = defaultValue;

			TaskScheduler scheduler = TaskScheduler.FromCurrentSynchronizationContext();

			_valueTask.ContinueWith(t => { IsRunning = false; }, scheduler);

			_valueTask.ContinueWith(t => { OnPropertyChanged("Value"); }, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, scheduler);
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

		public event PropertyChangedEventHandler PropertyChanged;

		void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}