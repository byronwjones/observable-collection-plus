using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BWJ.Collections
{
    public enum ObservableCollectionPlusOptions
    {
        /// <summary>
        /// Default behavior:
        ///  - Only one change event fires on load
        ///  - Custom actions can be set for execution before/after changes after instanciation
        ///  - Change event firing can be suspended/reenabled
        /// </summary>
        Default = 0,

        /// <summary>
        /// Do not allow assignment/removal of methods invoked at collection changes after instanciation
        /// </summary>
        DisallowChangeResponders = 1,

        /// <summary>
        /// Do not allow suspension of normal change event firing
        /// </summary>
        DisallowNotificationSuppression = 1 << 1,

        /// <summary>
        /// Do not automatically subscribe to PropertyChanged event on collection items when 
        /// item type T implements INotifyPropertyChanged with a publicly accessible 
        /// PropertyChanged event
        /// </summary>
        DisableAutoPropertyChangedSubscription = 1 << 2,
    }
}
