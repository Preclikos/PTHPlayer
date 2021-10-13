using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PTHPlayer.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        #region properties

        /// <summary>
        /// PropertyChanged event handler.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region methods

        /// <summary>
        /// Updates value of the "storage" argument with value given by the second argument.
        /// Notifies the application about update of the property which has executed this method.
        /// </summary>
        /// <param name="storage">Value storage object.</param>
        /// <param name="value">Value to set.</param>
        /// <param name="propertyName">Automatically obtained property name.</param>
        /// <typeparam name="T">Property value type.</typeparam>
        /// <returns>Returns true if storage is successfully updated, false otherwise.</returns>
        protected bool SetProperty<T>(ref T storage, T value,
            [CallerMemberName] string propertyName = null)
        {
            if (Object.Equals(storage, value))
            {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Notifies the application about update of the property with name given as a parameter.
        /// </summary>
        /// <param name="propertyName">Name of the changed property.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}