// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

#pragma warning disable CS0618

namespace MvvmGen.Commands;

/// <summary>
/// A generic command that provides a more specific version of <see cref="AsyncSafeCommand"/>.
/// </summary>
/// <typeparam name="T">The type of parameter being passed as input to the callbacks.</typeparam>
public sealed class AsyncSafeCommand<T> : IAsyncRelayCommand<T>, ICancellationAwareCommand
{
    private ILogger _logger;
    private IExceptionHandler _exceptionHandler;
    private string _name;
    /// <summary>
    /// The <see cref="Func{TResult}"/> to invoke when <see cref="Execute(T)"/> is used.
    /// </summary>
    private readonly Func<T?, Task>? execute;

    /// <summary>
    /// The cancelable <see cref="Func{T1,T2,TResult}"/> to invoke when <see cref="Execute(object?)"/> is used.
    /// </summary>
    private readonly Func<T?, CancellationToken, Task>? cancelableExecute;

    /// <summary>
    /// The optional action to invoke when <see cref="CanExecute(T)"/> is used.
    /// </summary>
    private readonly Predicate<T?>? canExecute;

    /// <summary>
    /// Indicates whether or not concurrent executions of the command are allowed.
    /// </summary>
    private readonly bool allowConcurrentExecutions;

    /// <summary>
    /// The <see cref="CancellationTokenSource"/> instance to use to cancel <see cref="cancelableExecute"/>.
    /// </summary>
    private CancellationTokenSource? cancellationTokenSource;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public event EventHandler? CanExecuteChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncSafeCommand{T}"/> class.
    /// </summary>
    /// <param name="execute">The execution logic.</param>
    /// <remarks>See notes in <see cref="RelayCommand{T}(Action{T})"/>.</remarks>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="execute"/> is <see langword="null"/>.</exception>
    public AsyncSafeCommand(ILogger logger, IExceptionHandler exceptionHandler, Func<T?, Task> execute, string name = "")
    {
        ArgumentNullException.ThrowIfNull(execute);

        this.execute = execute;
        _name = name;
        _logger = logger;
        _exceptionHandler = exceptionHandler;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncSafeCommand{T}"/> class.
    /// </summary>
    /// <param name="execute">The execution logic.</param>
    /// <param name="allowConcurrentExecutions">Whether or not to allow concurrent executions of the command.</param>
    /// <remarks>See notes in <see cref="RelayCommand{T}(Action{T})"/>.</remarks>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="execute"/> is <see langword="null"/>.</exception>
    public AsyncSafeCommand(ILogger logger, IExceptionHandler exceptionHandler, Func<T?, Task> execute, bool allowConcurrentExecutions, string name = "")
    {
        ArgumentNullException.ThrowIfNull(execute);

        this.execute = execute;
        this.allowConcurrentExecutions = allowConcurrentExecutions;
        _name = name;
        _logger = logger;
        _exceptionHandler = exceptionHandler;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncSafeCommand{T}"/> class.
    /// </summary>
    /// <param name="cancelableExecute">The cancelable execution logic.</param>
    /// <remarks>See notes in <see cref="RelayCommand{T}(Action{T})"/>.</remarks>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="cancelableExecute"/> is <see langword="null"/>.</exception>
    public AsyncSafeCommand(ILogger logger, IExceptionHandler exceptionHandler, Func<T?, CancellationToken, Task> cancelableExecute, string name = "")
    {
        ArgumentNullException.ThrowIfNull(cancelableExecute);

        this.cancelableExecute = cancelableExecute;
        _name = name;
        _logger = logger;
        _exceptionHandler = exceptionHandler;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncSafeCommand{T}"/> class.
    /// </summary>
    /// <param name="cancelableExecute">The cancelable execution logic.</param>
    /// <param name="allowConcurrentExecutions">Whether or not to allow concurrent executions of the command.</param>
    /// <remarks>See notes in <see cref="RelayCommand{T}(Action{T})"/>.</remarks>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="cancelableExecute"/> is <see langword="null"/>.</exception>
    public AsyncSafeCommand(ILogger logger, IExceptionHandler exceptionHandler, Func<T?, CancellationToken, Task> cancelableExecute, bool allowConcurrentExecutions, string name = "")
    {
        ArgumentNullException.ThrowIfNull(cancelableExecute);

        this.cancelableExecute = cancelableExecute;
        this.allowConcurrentExecutions = allowConcurrentExecutions;
        _name = name;
        _logger = logger;
        _exceptionHandler = exceptionHandler;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncSafeCommand{T}"/> class.
    /// </summary>
    /// <param name="execute">The execution logic.</param>
    /// <param name="canExecute">The execution status logic.</param>
    /// <remarks>See notes in <see cref="RelayCommand{T}(Action{T})"/>.</remarks>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="execute"/> or <paramref name="canExecute"/> are <see langword="null"/>.</exception>
    public AsyncSafeCommand(ILogger logger, IExceptionHandler exceptionHandler, Func<T?, Task> execute, Predicate<T?> canExecute, string name = "")
    {
        ArgumentNullException.ThrowIfNull(execute);
        ArgumentNullException.ThrowIfNull(canExecute);

        this.execute = execute;
        this.canExecute = canExecute;
        _logger = logger;
        _exceptionHandler = exceptionHandler;
        _name = name;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncSafeCommand{T}"/> class.
    /// </summary>
    /// <param name="execute">The execution logic.</param>
    /// <param name="canExecute">The execution status logic.</param>
    /// <param name="allowConcurrentExecutions">Whether or not to allow concurrent executions of the command.</param>
    /// <remarks>See notes in <see cref="RelayCommand{T}(Action{T})"/>.</remarks>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="execute"/> or <paramref name="canExecute"/> are <see langword="null"/>.</exception>
    public AsyncSafeCommand(ILogger logger, IExceptionHandler exceptionHandler, Func<T?, Task> execute, Predicate<T?> canExecute, bool allowConcurrentExecutions, string name ="")
    {
        ArgumentNullException.ThrowIfNull(execute);
        ArgumentNullException.ThrowIfNull(canExecute);

        this.execute = execute;
        this.canExecute = canExecute;
        this.allowConcurrentExecutions = allowConcurrentExecutions;
        _name = name;
        _logger = logger;
        _exceptionHandler = exceptionHandler;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncSafeCommand{T}"/> class.
    /// </summary>
    /// <param name="cancelableExecute">The cancelable execution logic.</param>
    /// <param name="canExecute">The execution status logic.</param>
    /// <remarks>See notes in <see cref="RelayCommand{T}(Action{T})"/>.</remarks>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="cancelableExecute"/> or <paramref name="canExecute"/> are <see langword="null"/>.</exception>
    public AsyncSafeCommand(Func<T?, CancellationToken, Task> cancelableExecute, Predicate<T?> canExecute, ILogger logger, IExceptionHandler exceptionHandler)
    {
        ArgumentNullException.ThrowIfNull(cancelableExecute);
        ArgumentNullException.ThrowIfNull(canExecute);

        this.cancelableExecute = cancelableExecute;
        this.canExecute = canExecute;
        _logger = logger;
        _exceptionHandler = exceptionHandler;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncSafeCommand{T}"/> class.
    /// </summary>
    /// <param name="cancelableExecute">The cancelable execution logic.</param>
    /// <param name="canExecute">The execution status logic.</param>
    /// <param name="allowConcurrentExecutions">Whether or not to allow concurrent executions of the command.</param>
    /// <remarks>See notes in <see cref="RelayCommand{T}(Action{T})"/>.</remarks>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="cancelableExecute"/> or <paramref name="canExecute"/> are <see langword="null"/>.</exception>
    public AsyncSafeCommand(Func<T?, CancellationToken, Task> cancelableExecute, Predicate<T?> canExecute, bool allowConcurrentExecutions, ILogger logger, IExceptionHandler exceptionHandler)
    {
        ArgumentNullException.ThrowIfNull(cancelableExecute);
        ArgumentNullException.ThrowIfNull(canExecute);

        this.cancelableExecute = cancelableExecute;
        this.canExecute = canExecute;
        this.allowConcurrentExecutions = allowConcurrentExecutions;
        _logger = logger;
        _exceptionHandler = exceptionHandler;
    }

    private Task? executionTask;

    /// <inheritdoc/>
    public Task? ExecutionTask {
        get => this.executionTask;
        private set {
            if (ReferenceEquals(this.executionTask, value))
            {
                return;
            }

            this.executionTask = value;

            PropertyChanged?.Invoke(this, AsyncSafeCommand.ExecutionTaskChangedEventArgs);
            PropertyChanged?.Invoke(this, AsyncSafeCommand.IsRunningChangedEventArgs);

            bool isAlreadyCompletedOrNull = value?.IsCompleted ?? true;

            if (this.cancellationTokenSource is not null)
            {
                PropertyChanged?.Invoke(this, AsyncSafeCommand.CanBeCanceledChangedEventArgs);
                PropertyChanged?.Invoke(this, AsyncSafeCommand.IsCancellationRequestedChangedEventArgs);
            }

            if (isAlreadyCompletedOrNull)
            {
                return;
            }

            static async void MonitorTask(AsyncSafeCommand<T> @this, Task task)
            {
                await task.GetAwaitableWithoutEndValidation();

                if (ReferenceEquals(@this.executionTask, task))
                {
                    @this.PropertyChanged?.Invoke(@this, AsyncSafeCommand.ExecutionTaskChangedEventArgs);
                    @this.PropertyChanged?.Invoke(@this, AsyncSafeCommand.IsRunningChangedEventArgs);

                    if (@this.cancellationTokenSource is not null)
                    {
                        @this.PropertyChanged?.Invoke(@this, AsyncSafeCommand.CanBeCanceledChangedEventArgs);
                    }

                    if (!@this.allowConcurrentExecutions)
                    {
                        @this.CanExecuteChanged?.Invoke(@this, EventArgs.Empty);
                    }
                }
            }

            MonitorTask(this, value!);
        }
    }

    /// <inheritdoc/>
    public bool CanBeCanceled => IsRunning && this.cancellationTokenSource is { IsCancellationRequested: false };

    /// <inheritdoc/>
    public bool IsCancellationRequested => this.cancellationTokenSource is { IsCancellationRequested: true };

    /// <inheritdoc/>
    public bool IsRunning => ExecutionTask is { IsCompleted: false };

    /// <inheritdoc/>
    bool ICancellationAwareCommand.IsCancellationSupported => this.execute is null;

    /// <inheritdoc/>
    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanExecute(T? parameter)
    {
        bool canExecute = this.canExecute?.Invoke(parameter) != false;

        return canExecute && (this.allowConcurrentExecutions || ExecutionTask is not { IsCompleted: false });
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanExecute(object? parameter)
    {
        if (default(T) is not null &&
            parameter is null)
        {
            return false;
        }

        return CanExecute((T?)parameter);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Execute(T? parameter)
    {
        _ = ExecuteAsync(parameter);
    }

    /// <inheritdoc/>
    public void Execute(object? parameter)
    {
        _ = ExecuteAsync((T?)parameter);
    }

    /// <inheritdoc/>
    public async Task ExecuteAsync(T? parameter)
    {
        try
        {
            Task executionTask;
            if (!string.IsNullOrEmpty(_name))
                _logger.LogInformation($"Starting executing {_name}");

            if (this.execute is not null)
            {
                // Non cancelable command delegate
                executionTask = ExecutionTask = this.execute(parameter);
            }
            else
            {
                // Cancel the previous operation, if one is pending
                this.cancellationTokenSource?.Cancel();

                CancellationTokenSource cancellationTokenSource = this.cancellationTokenSource = new();

                // Invoke the cancelable command delegate with a new linked token
                executionTask = ExecutionTask = this.cancelableExecute!(parameter, cancellationTokenSource.Token);
            }

            // If concurrent executions are disabled, notify the can execute change as well
            if (!this.allowConcurrentExecutions)
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }

            await executionTask;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "");
            _exceptionHandler.Handle(e);
        }
        finally
        {
            if (!string.IsNullOrEmpty(_name))
                _logger.LogInformation($"Completed executing {_name}");
        }
    }

    /// <inheritdoc/>
    public Task ExecuteAsync(object? parameter)
    {
        return ExecuteAsync((T?)parameter);
    }

    /// <inheritdoc/>
    public void Cancel()
    {
        if (this.cancellationTokenSource is CancellationTokenSource { IsCancellationRequested: false } cancellationTokenSource)
        {
            cancellationTokenSource.Cancel();

            PropertyChanged?.Invoke(this, AsyncSafeCommand.CanBeCanceledChangedEventArgs);
            PropertyChanged?.Invoke(this, AsyncSafeCommand.IsCancellationRequestedChangedEventArgs);
        }
    }
}
