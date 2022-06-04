using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace MvvmGen.Commands;

/// <summary>
/// A generic command whose sole purpose is to relay its functionality to other
/// objects by invoking delegates. The default return value for the CanExecute
/// method is <see langword="true"/>. This class allows you to accept command parameters
/// in the <see cref="Execute(T)"/> and <see cref="CanExecute(T)"/> callback methods.
/// </summary>
/// <typeparam name="T">The type of parameter being passed as input to the callbacks.</typeparam>
public sealed class SafeCommand<T> : IRelayCommand<T>
{
    private ILogger _logger;
    private IExceptionHandler _exceptionHandler;
    /// <summary>
    /// The <see cref="Action"/> to invoke when <see cref="Execute(T)"/> is used.
    /// </summary>
    private readonly Action<T?> execute;

    /// <summary>
    /// The optional action to invoke when <see cref="CanExecute(T)"/> is used.
    /// </summary>
    private readonly Predicate<T?>? canExecute;

    /// <inheritdoc/>
    public event EventHandler? CanExecuteChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="RelayCommand{T}"/> class that can always execute.
    /// </summary>
    /// <param name="execute">The execution logic.</param>
    /// <param name="logger"></param>
    /// <param name="exceptionHandler"></param>
    /// <remarks>
    /// Due to the fact that the <see cref="System.Windows.Input.ICommand"/> interface exposes methods that accept a
    /// nullable <see cref="object"/> parameter, it is recommended that if <typeparamref name="T"/> is a reference type,
    /// you should always declare it as nullable, and to always perform checks within <paramref name="execute"/>.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="execute"/> is <see langword="null"/>.</exception>
    public SafeCommand(Action<T?> execute, ILogger logger, IExceptionHandler exceptionHandler)
    {
        ArgumentNullException.ThrowIfNull(execute);

        this.execute = execute;
        _logger = logger;
        _exceptionHandler = exceptionHandler;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RelayCommand{T}"/> class.
    /// </summary>
    /// <param name="execute">The execution logic.</param>
    /// <param name="canExecute">The execution status logic.</param>
    /// <param name="logger"></param>
    /// <param name="exceptionHandler"></param>
    /// <remarks>See notes in <see cref="RelayCommand{T}(Action{T})"/>.</remarks>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="execute"/> or <paramref name="canExecute"/> are <see langword="null"/>.</exception>
    public SafeCommand(Action<T?> execute, Predicate<T?> canExecute, ILogger logger, IExceptionHandler exceptionHandler)
    {
        ArgumentNullException.ThrowIfNull(execute);
        ArgumentNullException.ThrowIfNull(canExecute);

        this.execute = execute;
        this.canExecute = canExecute;
        _logger = logger;
        _exceptionHandler = exceptionHandler;
    }

    /// <inheritdoc/>
    public void NotifyCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanExecute(T? parameter)
    {
        return this.canExecute?.Invoke(parameter) != false;
    }

    /// <inheritdoc/>
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
        try
        {
            this.execute(parameter);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "");
            _exceptionHandler.Handle(e);
        }
    }

    /// <inheritdoc/>
    public void Execute(object? parameter)
    {
        Execute((T?)parameter);
    }
}
