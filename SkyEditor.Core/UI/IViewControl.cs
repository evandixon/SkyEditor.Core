using System;
using System.Collections.Generic;
using System.Reflection;


namespace SkyEditor.Core.UI
{
    /// <summary>
    /// Represents a View for a ViewModel
    /// </summary>
    public interface IViewControl
    {
        /// <summary>
        /// Raised when the header has changed
        /// </summary>
        event EventHandler<HeaderUpdatedEventArgs> HeaderUpdated;

        /// <summary>
        /// Raised when the <see cref="IsModified"/> property's value has changed
        /// </summary>
        event EventHandler IsModifiedChanged;

        /// <summary>
        /// The value of the Header.  Only used when the control is behaving as a tab.
        /// </summary>
        string Header { get; }

        /// <summary>
        /// The object this control is intended to edit.
        /// </summary>
        object ViewModel { get; set; }

        /// <summary>
        /// Whether or not the view model has been modified without saving.
        /// Set to true when the user changes anything in the GUI.
        /// Set to false when the object is saved or if the user undoes every change.
        /// </summary>
        /// <returns></returns>
        bool IsModified { get; set; }

        /// <summary>
        /// Updates the current reference to the current application ViewModel
        /// </summary>
        /// <param name="appViewModel">Instance of the current application ViewModel</param>
        void SetApplicationViewModel(ApplicationViewModel appViewModel);

        /// <summary>
        /// IEnumerable of every type of view model that the control can target.
        /// <see cref="ViewModel"/> will be of one of these types.
        /// </summary>
        /// <returns>IEnumerable of every type of view model that the control can target</returns>
        IEnumerable<TypeInfo> GetSupportedTypes();

        /// <summary>
        /// Returns whether or not the control supports the given object.
        /// <paramref name="obj"/> will be of one of the types in <see cref="GetSupportedTypes"/>, but this function gives the control more control over what objects it will edit.
        /// Should return true if there is no situation exists where a given object of a supported type is not supported.
        /// </summary>
        /// <returns>A boolean indicating whether or not the control supports the given object</returns>
        bool SupportsObject(object obj);

        /// <summary>
        /// Determines whether or not this control is a backup control.
        /// </summary>
        /// <returns>A boolean indicating whether or not this control is a backup control.</returns>
        /// <remarks>
        /// Determines whether or not this <see cref="IObjectControl"/> should be used for the given object if another control exists for it.
        /// If false, this will be used if <see cref="SupportsObject(Object)"/> is true.
        /// If true, this will only be used if no other <see cref="IViewControl"/> can edit the given object.
        ///
        /// If multiple backup controls are present, <see cref="GetSortOrder(Type, Boolean)"/> will be used to determine which <see cref="IViewControl"/> is used.</remarks>
        bool GetIsBackupControl();

        /// <summary>
        /// Gets the sort order of this control when editing the given type.
        /// Note: The returned value is context-specific.  Higher values make a Control more likely to be used, but lower values make tabs appear higher in the list of tabs.
        /// </summary>
        /// <param name="currentType">Type of the view model</param>
        /// <param name="isTab">Whether or not the control will registered to behave as a Tab or a Control.</param>
        /// <returns>The sort order used when for the given configuration</returns>
        int GetSortOrder(TypeInfo currentType, bool isTab);
    }

}