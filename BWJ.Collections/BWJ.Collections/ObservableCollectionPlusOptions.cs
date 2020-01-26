using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BWJ.Collections
{
    /// <summary>
    /// ObservableCollectionPlus configuration options
    /// </summary>
    public enum ObservableCollectionPlusOptions
    {
        /// <summary>
        /// Default behavior:
        ///  - The occurence of change events can be suspended/reenabled
        ///  - When type param T implements INotifyPropertyChanged, items are automatically configured for
        ///  change detection
        /// </summary>
        Default = 0,

        /// <summary>
        /// Do not allow suspension of normal change event firing
        /// </summary>
        DisallowNotificationSuppression = 1,

        /// <summary>
        /// Do not automatically subscribe to PropertyChanged event on collection items when 
        /// item type T implements INotifyPropertyChanged with a publicly accessible 
        /// PropertyChanged event
        /// </summary>
        DisableAutoPropertyChangedSubscription = 1 << 1,
    }
}
