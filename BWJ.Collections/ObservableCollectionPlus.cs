using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BWJ.Collections
{
    /// <summary>
    /// An object collection implementing INotifyPropertyChanged and INotifyCollectionChanged.
    /// Very similar in functionality to ObservableCollection, with some variation in
    /// how multithreading scenarios are managed and most importantly, the ability to reload the instance
    /// with a distinct set of data without firing events several times
    /// </summary>
    [Serializable()]
    public class ObservableCollectionPlus<T> : Collection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        #region Ctors
        public ObservableCollectionPlus() : base() { }

        /// <summary>
        /// Initialization of the collection with a dataset
        /// </summary>
        /// <param name="collection">The initial dataset managed by this collection</param>
        public ObservableCollectionPlus(IEnumerable<T> collection)
        {
            if(collection == null)
            {
                throw new ArgumentNullException("collection");
            }

            Load(collection);
        }

        /// <summary>
        /// Initialization of the collection with configuration arguments and optional event responders
        /// </summary>
        /// <param name="config">Instance configuration options</param>
        /// <param name="onClear">Action invoked when this collection is cleared</param>
        /// <param name="onInsert">Action invoked when an item is added to this collection</param>
        /// <param name="onMove">Action invoked when an item in this collection is moved</param>
        /// <param name="onRemove">Action invoked when an item is removed from this collection</param>
        public ObservableCollectionPlus(ObservableCollectionPlusOptions config,
            Action onClear = null,
            Action<T> onInsert = null,
            Action<T> onMove = null,
            Action<T, T> onReplace = null,
            Action<T> onRemove = null)
        {
            ConfigureInstance(config, onClear, onInsert, onMove, onReplace, onRemove);
        }

        /// <summary>
        /// Initialization of the collection with a dataset, configuration arguments,
        /// and optional event responders
        /// </summary>
        /// <param name="collection">The initial dataset managed by this collection</param>
        /// <param name="config">Instance configuration options</param>
        /// <param name="onClear">Action invoked when this collection is cleared</param>
        /// <param name="onInsert">Action invoked when an item is added to this collection</param>
        /// <param name="onMove">Action invoked when an item in this collection is moved</param>
        /// <param name="onRemove">Action invoked when an item is removed from this collection</param>
        public ObservableCollectionPlus(IEnumerable<T> collection,
            ObservableCollectionPlusOptions config,
            Action onClear = null,
            Action<T> onInsert = null,
            Action<T> onMove = null,
            Action<T, T> onReplace = null,
            Action<T> onRemove = null)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }

            ConfigureInstance(config, onClear, onInsert, onMove, onReplace, onRemove);
            Load(collection);
        }
        #endregion Ctors

        /// <summary>
        /// Clears the previous items and reloads this collection with the given items
        /// </summary>
        /// <param name="collection">Collection of items to copy to this instance</param>
        /// <param name="raiseMultipleEventsOnLoad">When true, will raise an event when the collection is cleared,
        /// then for each item added. Otherwise only raises the reset event after the loading process completes.</param>
        /// <remarks>The SuppressChangeNotification property is overriden during the execution of this method
        /// when raiseMultipleEventsOnLoad is true</remarks>
        public void Load(IEnumerable<T> collection, bool raiseMultipleEventsOnLoad = false)
        {
            Load(collection, true, raiseMultipleEventsOnLoad);
        }

        public bool SuppressChangeNotification
        {
            get { return _SuppressChangeNotification; }
            set
            {
                if(value && DisallowNotificationSuppression)
                {
                    throw new InvalidOperationException("Change notification suppression is not permitted on this instance");
                }

                _SuppressChangeNotification = value;
            }
        }

        #region Change event hooks
        /// <summary>
        /// An action invoked after the collection is cleared, but before a collection changed event is fired
        /// </summary>
        public Action OnClear
        {
            get
            {
                return _OnClear ?? DoNothing;
            }
            set
            {
                if (DisallowChangeResponders)
                {
                    throw new InvalidOperationException(ChangeHookEx);
                }
                _OnClear = value;
            }
        }

        /// <summary>
        /// An action invoked after an item insertion, but before a collection changed event is fired
        /// </summary>
        public Action<T> OnInsert
        {
            get
            {
                return _OnInsert ?? DoNothingWithArg;
            }
            set
            {
                if (DisallowChangeResponders)
                {
                    throw new InvalidOperationException(ChangeHookEx);
                }
                _OnInsert = value;
            }
        }

        /// <summary>
        /// An action invoked after an item move, but before a collection changed event is fired
        /// </summary>
        public Action<T> OnMove
        {
            get
            {
                return _OnMove ?? DoNothingWithArg;
            }
            set
            {
                if (DisallowChangeResponders)
                {
                    throw new InvalidOperationException(ChangeHookEx);
                }
                _OnMove = value;
            }
        }

        /// <summary>
        /// An action invoked after an item move, but before a collection changed event is fired
        /// </summary>
        /// <remarks>Input argument order is newItem, oldItem</remarks>
        /// <example>(new, old) => { //logic here... }</example>
        public Action<T, T> OnReplace
        {
            get
            {
                return _OnReplace ?? ((t, u)=>{ });
            }
            set
            {
                if (DisallowChangeResponders)
                {
                    throw new InvalidOperationException(ChangeHookEx);
                }
                _OnReplace = value;
            }
        }

        /// <summary>
        /// An action invoked after an item removal, but before a collection changed event is fired
        /// </summary>
        public Action<T> OnRemove
        {
            get
            {
                return _OnRemove ?? DoNothingWithArg;
            }
            set
            {
                if (DisallowChangeResponders)
                {
                    throw new InvalidOperationException(ChangeHookEx);
                }
                _OnRemove = value;
            }
        }
        #endregion Change event hooks

        #region Interface implementation
        // explicitly implementing this event allows the event accessors to be overriden
        // by a protected member
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add
            {
                PropertyChanged += value;
            }
            remove
            {
                PropertyChanged -= value;
            }
        }

        public virtual event NotifyCollectionChangedEventHandler CollectionChanged;
        #endregion Interface implementation

        protected virtual event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the PropertyChanged event
        /// </summary>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged(this, e);
        }

        /// <summary>
        /// Throws an exception if there is an event handler in execution
        /// </summary>
        /// <remarks>To be called prior to any logic that will alter this collection</remarks>
        private void AssertNotInEventHandler()
        {
            if (HandlerCount > 0)
            {
                throw new InvalidOperationException("Modification of this collection during execution of an event handler is not permitted.");
            }
        }

        private void Load(IEnumerable<T> collection, bool raiseResetEvent,
            bool raiseMultipleEventsOnLoad = false)
        {
            // preserve the current state of change notification suppression
            bool origSuppressionState = SuppressChangeNotification;
            // suppress change notification temporarily when not firing multiple events on load
            _SuppressChangeNotification = !raiseMultipleEventsOnLoad;

            ClearItems();

            // we will stop at clearing this collection if the collection given is null
            if (collection != null)
            {
                foreach (var item in collection)
                {
                    Add(item);
                }
            }

            _SuppressChangeNotification = origSuppressionState;
            
            // we only raise the reset event when explicitly permitted to,
            // we didn't already raise multiple events during the load process,
            // and we are not suppressing change notifications
            if(raiseResetEvent && !raiseMultipleEventsOnLoad && !SuppressChangeNotification)
            {
                RaiseCollectionReset();
            }
        }

        private void ConfigureInstance(ObservableCollectionPlusOptions config,
            Action onClear,
            Action<T> onInsert,
            Action<T> onMove,
            Action<T, T> onReplace,
            Action<T> onRemove)
        {
            Options = config;
            _OnClear = onClear;
            _OnInsert = onInsert;
            _OnMove = onMove;
            _OnReplace = onReplace;
            _OnRemove = onRemove;
        }

        #region Collection change regulation
        /// <summary>
        /// Calls the <see cref="OnCollectionChanged(NotifyCollectionChangedEventArgs)"/> method
        /// safely, ensuring that the collection has not been altered by another event handler
        /// </summary>
        /// <param name="e">Event arguments</param>
        private void RaiseCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null)
            {
                lock (HandlerCountLock)
                {
                    HandlerCount++;
                }

                OnCollectionChanged(e);

                lock (HandlerCountLock)
                {
                    HandlerCount--;
                }
            }
        }

        private object HandlerCountLock = new object();
        private int HandlerCount = 0;
        #endregion Collection change regulation

        #region Raise event methods
        private void RaisePropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        private void RaiseInsertOrRemoveItem(bool addItem, object item, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                addItem ? NotifyCollectionChangedAction.Add : NotifyCollectionChangedAction.Remove,
                item, index));
        }
        private void RaiseMoveItem(object item, int index, int oldIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move,
                item, index, oldIndex));
        }
        private void RaiseReplaceItem(object oldItem, object newItem, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
                newItem, oldItem, index));
        }
        private void RaiseCollectionReset()
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        #endregion Raise event methods

        private bool DisallowChangeResponders
        {
            get
            {
                return (ObservableCollectionPlusOptions.DisallowChangeResponders & Options) ==
                    ObservableCollectionPlusOptions.DisallowChangeResponders;
            }
        }
        private bool DisallowNotificationSuppression
        {
            get
            {
                return (ObservableCollectionPlusOptions.DisallowNotificationSuppression & Options) ==
                    ObservableCollectionPlusOptions.DisallowNotificationSuppression;
            }
        }

        private ObservableCollectionPlusOptions Options = ObservableCollectionPlusOptions.Default;

        private Action DoNothing = () => { };
        private Action<T> DoNothingWithArg = (t) => { };
        private Action _OnClear = null;
        private Action<T> _OnInsert = null;
        private Action<T> _OnMove = null;
        private Action<T,T> _OnReplace = null;
        private Action<T> _OnRemove = null;

        private bool _SuppressChangeNotification = false;

        private string ChangeHookEx = "Utilization or removal of change event responders is not permitted on this instance";
    }
}
