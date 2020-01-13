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
        /// During invokation of Load, fire change event when the collection is cleared, and on each
        ///    individual addition to the collection
        /// </summary>
        FireMultipleEventsOnLoad = 1,

        /// <summary>
        /// Do not allow assignment/removal of methods invoked at collection changes after instanciation
        /// </summary>
        DisallowChangeResponders = 1 << 1,

        /// <summary>
        /// Do not allow suspension of normal change event firing
        /// </summary>
        DisallowEventSuppression = 1 << 2
    }
}
