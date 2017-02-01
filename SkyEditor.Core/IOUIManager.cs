using SkyEditor.Core.UI;
using SkyEditor.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace SkyEditor.Core
{
    public class IOUIManager : IDisposable, INotifyPropertyChanged, IReportProgress
    {

        public IOUIManager(PluginManager manager)
        {
            throw new NotImplementedException();
        }

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<ProgressReportedEventArgs> ProgressChanged;
        public event EventHandler Completed;
        #endregion

        #region Properties
        /// <summary>
        /// The files that are currently open
        /// </summary>
        public ObservableCollection<FileViewModel> OpenFiles { get; private set; }

        #endregion

        public IEnumerable<object> GetViewModelsForModel(object dummy)
        {
            throw new NotImplementedException();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        public float Progress => throw new NotImplementedException();

        public string Message => throw new NotImplementedException();

        public bool IsIndeterminate => throw new NotImplementedException();

        public bool IsCompleted => throw new NotImplementedException();

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~IOUIManager() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
