using SkyEditor.Core.ConsoleCommands;
using SkyEditor.Core.IO;
using SkyEditor.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public Wizard(ApplicationViewModel applicationViewModel)
        {
            StepsInternal = new ObservableCollection<IWizardStepViewModel>();
            CurrentApplicationViewModel = applicationViewModel ?? throw new ArgumentNullException(nameof(applicationViewModel));
        }

        /// <summary>
        /// Raised when a property is changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The application ViewModel with which this wizard is associated
        /// </summary>
        public ApplicationViewModel CurrentApplicationViewModel { get; private set; }

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
                    _currentStep = Steps.First();
                }

                return _currentStep;
            }
            protected set
            {
                if (!ReferenceEquals(_currentStep, value))
                {
                    _currentStep = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentStep)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanGoForward)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanGoBack)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsComplete)));
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
            CurrentApplicationViewModel.CurrentPluginManager.CurrentConsoleProvider.WriteLine(line);
        }
    }
}
