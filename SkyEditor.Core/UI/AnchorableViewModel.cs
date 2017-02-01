using System;
using System.Collections.Generic;
using System.Text;

namespace SkyEditor.Core.UI
{
    public abstract class AnchorableViewModel
    {
        /// <summary>
        /// Raised when the IO/UI Manager's solution changed
        /// </summary>
        protected event EventHandler CurrentSolutionChanged;

        /// <summary>
        /// Raised when the IO/UI Manager is changed
        /// </summary>
        protected event EventHandler CurrentIOUIManagerChanged;

        /// <summary>
        /// The IO/UI Manager to which this anchorable view model belongs
        /// </summary>
        public IOUIManager CurrentIOUIManager
        {
            get
            {
                return _iouiManager;
            }
            set
            {
                // Remove old event handler
                if (_iouiManager != null)
                {
                    _iouiManager.SolutionChanged -= _iouiManager_SolutionName;
                }

                // Set the value
                _iouiManager = value;

                // Add new event handler
                if (_iouiManager != null)
                {
                    _iouiManager.SolutionChanged += _iouiManager_SolutionName;
                }

                // Raise changed event
                CurrentIOUIManagerChanged?.Invoke(this, new EventArgs());
            }
        }
        protected IOUIManager _iouiManager;

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
