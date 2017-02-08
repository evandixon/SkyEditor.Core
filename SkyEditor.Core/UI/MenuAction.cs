using SkyEditor.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.UI
{
    public abstract class MenuAction
    {
        public MenuAction(IEnumerable<string> path)
        {

        }

        public event EventHandler CurrentPluginManagerChanged;

        /// <summary>
        /// Whether or not the menu action is a context menu action
        /// </summary>
        public bool IsContextBased { get; protected set; }

        /// <summary>
        /// Names representing the location in a heiarchy of menu items
        /// </summary>
        public List<string> ActionPath { get; private set; }

        /// <summary>
        /// Instance of the current plugin manager
        /// </summary>
        public PluginManager CurrentPluginManager
        {
            get
            {
                return _currentPluginManager;
            }
            set
            {
                if (_currentPluginManager != value)
                {
                    _currentPluginManager = value;
                    CurrentPluginManagerChanged?.Invoke(this, new EventArgs());
                }
            }
        }
        private PluginManager _currentPluginManager;

        /// <summary>
        /// Whether or not visibility is independent from the action supporting the current target
        /// </summary>
        /// <remarks>
        /// True to be visible regardless of current targets.
        /// False to be dependant on MenuAction.SupportsObjects.
        /// </remarks>
        public bool AlwaysVisible { get; protected set; }

        /// <summary>
        /// Whether or not the action is only available in development mode
        /// </summary>
        public bool DevOnly { get; set; }

        /// <summary>
        /// Order in which menu items are sorted
        /// </summary>
        public decimal SortOrder { get; set; }

        /// <summary>
        /// The types of targets supported by the menu action
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<TypeInfo> GetSupportedTypes()
        {
            return Array.Empty<TypeInfo>();
        }

        /// <summary>
        /// Determines whether or not the given object is supported.
        /// </summary>
        /// <param name="obj">Object to determine if it is supported.</param>
        /// <returns>A boolean indicating whether or not <paramref name="obj"/> is supported.</returns>
        public virtual Task<bool> SupportsObject(object obj)
        {
            if (obj == null)
            {
                return Task.FromResult(AlwaysVisible);
            }
            else
            {
                return Task.FromResult(GetSupportedTypes().Any(x => ReflectionHelpers.IsOfType(obj.GetType(), x)));
            }
        }

        /// <summary>
        /// Determines whether or not the combination of given objects is supported.
        /// </summary>
        /// <param name="objects"><see cref="IEnumerable(Of Object)"/> to determine if they are supported.</param>
        /// <returns>A boolean indicating whether or not the given combination of objects is supported.</returns>
        public virtual async Task<bool> SupportsObjects(IEnumerable<object> objects)
        {
            foreach (var item in objects)
            {
                if (await SupportsObject(item))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Executes the logical function of the current <see cref="MenuAction"/>.
        /// </summary>
        /// <param name="targets">Targets of the <see cref="MenuAction"/>.</param>
        public abstract void DoAction(IEnumerable<object> targets);
    }
}
