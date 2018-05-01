using SkyEditor.Core.ConsoleCommands;
using SkyEditor.Core.IO;
using SkyEditor.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.UI
{
    /// <summary>
    /// Controller for a workflow of user interface steps
    /// </summary>
    public abstract class Wizard : INotifyPropertyChanged, INamed
    {
        public Wizard(PluginManager currentPluginManager)
        {
            StepsInternal = new ObservableCollection<IWizardStepViewModel>();
            StepsInternal.CollectionChanged += StepsInternal_CollectionChanged;
            CurrentPluginManager = currentPluginManager ?? throw new ArgumentNullException(nameof(currentPluginManager));
        }

        /// <summary>
        /// Raised when a property is changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public PluginManager CurrentPluginManager { get; private set; }

        /// <summary>
        /// Read-only collection of all wizard steps
        /// </summary>
        public ReadOnlyObservableCollection<IWizardStepViewModel> Steps
        {
            get
            {
                return _steps;
            }
            private set
            {
                _steps = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Steps)));
            }
        }
        private ReadOnlyObservableCollection<IWizardStepViewModel> _steps;

        /// <summary>
        /// Modifiable collection of all wizard steps
        /// </summary>
        protected ObservableCollection<IWizardStepViewModel> StepsInternal
        {
            get
            {
                return _stepsInternal;
            }
            set
            {
                _stepsInternal = value;
                Steps = new ReadOnlyObservableCollection<IWizardStepViewModel>(_stepsInternal);
            }
        }
        protected ObservableCollection<IWizardStepViewModel> _stepsInternal;

        /// <summary>
        /// The active step of the wizard
        /// </summary>
        public IWizardStepViewModel CurrentStep
        {
            get
            {
                if (_currentStep == null && Steps.Any())
                {
                    // Call property setter to set event handlers too
                    CurrentStep = Steps.First();
                }

                return _currentStep;
            }
            protected set
            {
                if (!ReferenceEquals(_currentStep, value))
                {
                    if (_currentStep != null && _currentStep is INotifyPropertyChanged oldNotifyPropertyChanged)
                    {
                        oldNotifyPropertyChanged.PropertyChanged -= CurrentStep_PropertyChanged;
                    }

                    _currentStep = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentStep)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentStepDisplayName)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanGoForward)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanGoBack)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsComplete)));

                    if (_currentStep != null && _currentStep is INotifyPropertyChanged newNotifyPropertyChanged)
                    {
                        newNotifyPropertyChanged.PropertyChanged += CurrentStep_PropertyChanged;
                    }
                }
            }
        }
        private IWizardStepViewModel _currentStep;

        public int CurrentStepIndex => CurrentStep != null ? Steps.IndexOf(CurrentStep) : -1;

        public string CurrentStepDisplayName => string.Format(Properties.Resources.Wizard_StepDisplayName, CurrentStepIndex + 1, CurrentStep.Name);

        public virtual bool CanGoForward => CurrentStep.IsComplete && Steps.Count > CurrentStepIndex + 1;

        public virtual bool CanGoBack => CurrentStepIndex > 0;

        public virtual bool IsComplete => CurrentStepIndex == Steps.Count - 1 && CurrentStep.IsComplete;

        /// <summary>
        /// Name of the wizard
        /// </summary>
        public abstract string Name { get; }

        public void GoForward()
        {
            if (!CanGoForward || Steps.Count <= CurrentStepIndex + 1)
            {
                return;
            }

            CurrentStep = Steps[CurrentStepIndex + 1];
        }

        public void GoBack()
        {
            if (!CanGoBack || CurrentStepIndex == 0)
            {
                return;
            }

            CurrentStep = Steps[CurrentStepIndex - 1];
        }

        /// <summary>
        /// Runs the wizard in a console
        /// </summary>
        public async Task RunInConsole(ConsoleShell shell, bool reportErrorsToConsole = false, IIOProvider provider = null)
        {
            foreach (var item in Steps)
            {
                CurrentStep = item; // In case any event handlers are listening

                ConsoleWriteLine(CurrentStepDisplayName);
                var command = CurrentStep.GetConsoleCommand();
                if (command != null)
                {
                    await shell.RunCommand(command, Enumerable.Empty<string>(), reportErrorsToConsole, provider);
                }
                else
                {
                    ConsoleWriteLine(Properties.Resources.Wizard_Console_NullCommand);
                }
                ConsoleWriteLine("");
            }
        }

        private void ConsoleWriteLine(string line)
        {
            CurrentPluginManager.CurrentConsoleProvider.WriteLine(line);
        }

        private void StepsInternal_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add &&
                e.NewItems.Count > 0 &&
                e.NewItems[0] is IWizardStepViewModel newStep &&
                _currentStep == null) // Compare to _currentStep instead of CurrentStep to avoid side effect
            {
                // Now we WANT to call CurrentStep to set event handlers
                CurrentStep = newStep;
            }
        }

        private void CurrentStep_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IWizardStepViewModel.IsComplete))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanGoForward)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanGoBack)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsComplete)));
            }
        }
    }
}
