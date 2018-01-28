using SkyEditor.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace SkyEditor.Core.UI
{
    /// <summary>
    /// Controller for a workflow of user interface steps
    /// </summary>
    public abstract class Wizard : INotifyPropertyChanged
    {
        public Wizard()
        {
            StepsInternal = new ObservableCollection<IWizardStepViewModel>();
            Steps = new ReadOnlyObservableCollection<IWizardStepViewModel>(StepsInternal);
        }

        /// <summary>
        /// Raised when a property is changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

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

        public abstract bool CanGoForward { get; }

        public virtual bool CanGoBack => CurrentStepIndex > 0;

        public virtual bool IsComplete => CurrentStepIndex == Steps.Count - 1;

        public void GoForward()
        {
            if (!CanGoForward || Steps.Count <= CurrentStepIndex + 1)
            {
                return;
            }

            CurrentStep = Steps[CurrentStepIndex + 1];
        }

    }
}
