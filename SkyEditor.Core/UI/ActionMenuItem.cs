using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SkyEditor.Core.UI
{
    /// <summary>
    /// Represents a menu item that performs a specific action
    /// </summary>
    public class ActionMenuItem : INotifyPropertyChanged
    {
        public ActionMenuItem()
        {
            Actions = new List<MenuAction>();
            Children = new ObservableCollection<ActionMenuItem>();
            Command = new RelayCommand(new Action<object>(RunActions));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Instance of the current IO/UI manager
        /// </summary>
        public ApplicationViewModel CurrentApplicationViewModel { get; set; }

        /// <summary>
        /// Header of the menu item
        /// </summary>
        public string Header
        {
            get
            {
                return _header;
            }
            set
            {
                if (_header != value)
                {
                    _header = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Header)));
                }
            }
        }
        private string _header;

        /// <summary>
        /// Whether or not the menu item is visible
        /// </summary>
        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsVisible)));
                }
            }
        }
        private bool _isVisible;

        /// <summary>
        /// The actions associated with the current action menu item
        /// </summary>
        public List<MenuAction> Actions { get; private set; }

        /// <summary>
        /// Children of the current menu item
        /// </summary>
        public ObservableCollection<ActionMenuItem> Children { get; private set; }

        /// <summary>
        /// Targets of the menu item.  Overrides targets retrieved from the IO/UI manager.
        /// </summary>
        public IEnumerable<object> ContextTargets { get; set; }

        /// <summary>
        /// The command to run
        /// </summary>
        public ICommand Command { get; private set; }

        private async void RunActions(object dummy)
        {
            // Run synchronously to avoid threading issues
            foreach (var t in Actions)
            {
                t.DoAction(await GetTargets(t));
            }
        }

        private async Task<IEnumerable<object>> GetTargets(MenuAction action)
        {
            if (ContextTargets != null)
            {
                return ContextTargets;
            }
            else
            {
                return await CurrentApplicationViewModel.GetMenuActionTargets(action);
            }
        }
    }
}
