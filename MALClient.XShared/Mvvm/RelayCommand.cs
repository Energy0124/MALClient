using System;
using System.Reflection;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace MALClient.XShared.Mvvm
{
    /// <summary>
    ///     MvvmLight compatible relay command implementing CommunityToolkit's <see cref="IRelayCommand" />.
    /// </summary>
    public class RelayCommand : IRelayCommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, bool keepTargetAlive = false) : this(execute, null, keepTargetAlive)
        {
        }

        //keepTargetAlive is kept for source compatibility, targets are always held strongly
        public RelayCommand(Action execute, Func<bool> canExecute, bool keepTargetAlive = false)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute();
        }

        public void Execute(object parameter)
        {
            if (CanExecute(parameter))
                _execute();
        }

        public void NotifyCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public void RaiseCanExecuteChanged()
        {
            NotifyCanExecuteChanged();
        }
    }

    /// <summary>
    ///     MvvmLight compatible generic relay command. Unlike CommunityToolkit's RelayCommand&lt;T&gt; it converts
    ///     <see cref="IConvertible" /> parameters, so XAML like CommandParameter="2" keeps working with int commands.
    /// </summary>
    public class RelayCommand<T> : IRelayCommand<T>
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public RelayCommand(Action<T> execute, bool keepTargetAlive = false) : this(execute, null, keepTargetAlive)
        {
        }

        //keepTargetAlive is kept for source compatibility, targets are always held strongly
        public RelayCommand(Action<T> execute, Func<T, bool> canExecute, bool keepTargetAlive = false)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(T parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        bool ICommand.CanExecute(object parameter)
        {
            if (_canExecute == null)
                return true;

            if (parameter == null && typeof(T).GetTypeInfo().IsValueType)
                return _canExecute(default(T));

            if (parameter == null || parameter is T)
                return _canExecute((T) parameter);

            return false;
        }

        public void Execute(T parameter)
        {
            if (CanExecute(parameter))
                _execute(parameter);
        }

        void ICommand.Execute(object parameter)
        {
            var value = parameter;
            if (parameter != null && !(parameter is T) && parameter is IConvertible)
                value = Convert.ChangeType(parameter, typeof(T), null);

            if (!((ICommand) this).CanExecute(value))
                return;

            _execute(value == null ? default(T) : (T) value);
        }

        public void NotifyCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public void RaiseCanExecuteChanged()
        {
            NotifyCanExecuteChanged();
        }
    }
}
