﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SkyEditor.Core.UI
{
    /// <summary>
    /// View model for an anchorable control
    /// </summary>
    public abstract class AnchorableViewModel
    {
        public AnchorableViewModel(ApplicationViewModel viewModel)
        {
            CurrentApplicationViewModel = viewModel;
        }

        /// <summary>
        /// Raised when the IO/UI Manager's solution changed
        /// </summary>
        protected event EventHandler CurrentSolutionChanged;

        /// <summary>
        /// Raised when the IO/UI Manager is changed
        /// </summary>
        protected event EventHandler CurrentIOUIManagerChanged;

        /// <summary>
        /// The application ViewModel to which this anchorable view model belongs
        /// </summary>
        protected ApplicationViewModel CurrentApplicationViewModel
        {
            get
            {
                return _appViewModel;
            }
            set
            {
                // Remove old event handler
                if (_appViewModel != null)
                {
                    _appViewModel.SolutionChanged -= _iouiManager_SolutionName;
                }

                // Set the value
                _appViewModel = value;

                // Add new event handler
                if (_appViewModel != null)
                {
                    _appViewModel.SolutionChanged += _iouiManager_SolutionName;
                }

                // Raise changed event
                CurrentIOUIManagerChanged?.Invoke(this, new EventArgs());
            }
        }
        private ApplicationViewModel _appViewModel;

        /// <summary>
        /// Unique identifier for the anchorable view model
        /// </summary>
        public string ID
        {
            get
            {
                return GetType().AssemblyQualifiedName;
            }
        }

        /// <summary>
        /// Display name of the anchorable view model
        /// </summary>
        public string Header { get; set; }

        private void _iouiManager_SolutionName(object sender, EventArgs e)
        {
            CurrentSolutionChanged?.Invoke(sender, e);
        }
    }
}
