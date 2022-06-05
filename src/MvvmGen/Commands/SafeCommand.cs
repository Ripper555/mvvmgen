#nullable enable

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.Logging;

namespace MvvmGen.Commands
{

    /// <summary>
    /// A command whose sole purpose is to relay its functionality to other
    /// objects by invoking delegates. The default return value for the <see cref="CanExecute"/>
    /// method is <see langword="true"/>. This type does not allow you to accept command parameters
    /// in the <see cref="Execute"/> and <see cref="CanExecute"/> callback methods.
    /// </summary>
    public sealed class SafeCommand : IRelayCommand
    {
        private ILogger _logger;
        private IExceptionHandler _exceptionHandler;
        /// <summary>
        /// The <see cref="Action"/> to invoke when <see cref="Execute"/> is used.
        /// </summary>
        private readonly Action execute;

        /// <summary>
        /// The optional action to invoke when <see cref="CanExecute"/> is used.
        /// </summary>
        private readonly Func<bool>? canExecute;

        /// <inheritdoc/>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class that can always execute.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="logger"></param>
        /// <param name="exceptionHandler"></param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="execute"/> is <see langword="null"/>.</exception>
        public SafeCommand(Action execute, ILogger logger, IExceptionHandler exceptionHandler)
        {
            ArgumentNullException.ThrowIfNull(execute);

            this.execute = execute;
            _logger = logger;
            _exceptionHandler = exceptionHandler;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        /// <param name="logger"></param>
        /// <param name="exceptionHandler"></param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="execute"/> or <paramref name="canExecute"/> are <see langword="null"/>.</exception>
        public SafeCommand(Action execute, Func<bool> canExecute, ILogger logger, IExceptionHandler exceptionHandler)
        {
            ArgumentNullException.ThrowIfNull(execute);
            ArgumentNullException.ThrowIfNull(canExecute);

            this.execute = execute;
            this.canExecute = canExecute;
            _logger = logger;
            _exceptionHandler = exceptionHandler;
        }

        /// <inheritdoc/>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanExecute(object? parameter)
        {
            return this.canExecute?.Invoke() != false;
        }

        /// <inheritdoc/>
        public void Execute(object? parameter)
        {
            try
            {
                this.execute();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "");
                _exceptionHandler.Handle(e);
            }
        }
    }

    public class SafeAsyncCommand : ICommand
    {
        private readonly IExceptionHandler _handler;
        private readonly ILogger? _logger;
        private readonly Func<object?, Task> _execute;
        private readonly Func<object?, bool>? _canExecute;

        public SafeAsyncCommand(IExceptionHandler handler, ILogger? logger, Func<object?, Task> execute, Func<object?, bool>? canExecute = null)
        {
            _handler = handler;
            _logger = logger;
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <inheritdoc/>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event.
        /// </summary>
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        /// <inheritdoc/>
        public async Task ExecuteAsync(object? parameter)
        {
            try
            {
                await _execute(parameter);
            }
            catch (Exception e)
            {
                _logger?.LogError(e, "");
                _handler.Handle(e);
            }
        }

        public void Execute(object? parameter)
        {
            _ = ExecuteAsync(parameter);
        }

        /// <inheritdoc/>
        public bool CanExecute(object? parameter) => _canExecute == null || _canExecute(parameter);

    }
}
