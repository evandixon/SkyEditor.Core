using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace SkyEditor.Core.UI
{
    /// <summary>
    /// Implementation of <see cref="ICommand"/> that executes a delegate <see cref="Action{object}"/>.
    /// </summary>
    public class RelayCommand : ICommand
    {
        /// <summary>
        /// Creates a new instance of <see cref="RelayCommand"/>
        /// </summary>
        /// <param name="executeAction">The action to be executed</param>
        public RelayCommand(Action<object> executeAction)
        {
            if (executeAction == null)
            {
                throw new ArgumentNullException(nameof(executeAction));
            }

            this.ExecuteAction = executeAction;
            this.IsEnabled = true;
        }

        /// <summary>
        /// Raised when <see cref="IsEnabled"/> is changed
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Whether or not the action is enabled
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                if (_isEnabled == value)
                {
                    _isEnabled = value;
                    CanExecuteChanged?.Invoke(this, new EventArgs());
                }
            }
        }
        private bool _isEnabled;

        /// <summary>
        /// Action to execute
        /// </summary>
        private Action<object> ExecuteAction { get; set; }

        /// <summary>
        /// Determines whether execution can occur
        /// </summary>
        public bool CanExecute(object parameter)
        {
            return IsEnabled;
        }

        /// <summary>
        /// Executes the action
        /// </summary>
        public void Execute(object parameter)
        {
            ExecuteAction.Invoke(parameter);
        }
    }
}
