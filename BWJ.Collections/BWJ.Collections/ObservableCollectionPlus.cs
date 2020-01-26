using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace BWJ.Collections
{
    /// <summary>
    /// An object collection implementing INotifyPropertyChanged and INotifyCollectionChanged.
    /// Very similar in functionality to ObservableCollection, with some variation in
    /// how multithreading scenarios are managed and most importantly, the ability to reload the instance
    /// with a distinct set of data without firing events several times
    /// </summary>
    public class ObservableCollectionPlus<T> : Collection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        #region Ctors
        public ObservableCollectionPlus() : base()
        {
            ConfigureInstance(ObservableCollectionPlusOptions.Default);
        }

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

            ConfigureInstance(ObservableCollectionPlusOptions.Default);
            Load(collection, false, false);
        }

        /// <summary>
        /// Initialization of the collection with configuration arguments
        /// </summary>
        /// <param name="config">Instance configuration options</param>
        public ObservableCollectionPlus(ObservableCollectionPlusOptions config)
        {
            ConfigureInstance(config);
        }

        /// <summary>
        /// Initialization of the collection with a dataset, configuration arguments,
        /// and optional event responders
        /// </summary>
        /// <param name="collection">The initial dataset managed by this collection</param>
        /// <param name="config">Instance configuration options</param>
        public ObservableCollectionPlus(IEnumerable<T> collection,
            ObservableCollectionPlusOptions config)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }

            ConfigureInstance(config);
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

        /// <summary>
        /// Move an item from a given index to a given index.
        /// </summary>
        /// <param name="currentIndex">Index where target item is currently located</param>
        /// <param name="newIndex">Index to move target item to</param>
        public void Move(int currentIndex, int newIndex)
        {
            MoveItem(currentIndex, newIndex);
        }
        /// <summary>
        /// Move the first occurrence of the given item to the given index
        /// </summary>
        /// <param name="item">Item to move</param>
        /// <param name="newIndex">Index to move item to</param>
        /// <exception cref="ArgumentNullException">Item must not be null</exception>
        /// <exception cref="ArgumentException">Item must exist in the collection</exception>
        public void Move(T item, int newIndex)
        {
            if(item == null)
            {
                throw new ArgumentNullException("item");
            }

            var currentIndex = Items.IndexOf(item);
            if(currentIndex < 0)
            {
                throw new ArgumentException("The item provided is not in the collection", "item");
            }

            MoveItem(currentIndex, newIndex);
        }

        /// <summary>
        /// Replace the first occurence of the given item with the given replacement
        /// </summary>
        /// <param name="item">Item to replace</param>
        /// <param name="replacement">Replacement item</param>
        /// <exception cref="ArgumentNullException">Neither item nor replacement may be null</exception>
        /// <exception cref="ArgumentException">Item must exist in the collection</exception>
        public void Replace(T item, T replacement)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            if (replacement == null)
            {
                throw new ArgumentNullException("replacement");
            }

            var index = Items.IndexOf(item);
            if (index < 0)
            {
                throw new ArgumentException("The item provided is not in the collection", "item");
            }

            SetItem(index, replacement);
        }

        /// <summary>
        /// Replaces every item matching the given predicate with the given replacement
        /// </summary>
        /// <returns>The items replaced</returns>
        /// <exception cref="ArgumentNullException">Neither predicate nor replacement may be null</exception>
        public List<T> Replace(Func<T, bool> predicate, T replacement)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            if (replacement == null)
            {
                throw new ArgumentNullException("replacement");
            }

            var targets = FilterCollection(predicate);
            foreach(var item in targets)
            {
                Replace(item, replacement);
            }

            return targets;
        }

        /// <summary>
        /// Removes every item matching the given predicate from the collection
        /// </summary>
        /// <returns>The items removed</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public List<T> RemoveWhere(Func<T, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }

            var targets = FilterCollection(predicate);
            foreach (var item in targets)
            {
                Remove(item);
            }

            return targets;
        }

        /// <summary>
        /// Prevents the generic <see cref="CollectionChanged"/>, specific collection change, and
        /// <see cref="ItemPropertyChanged"/> events from being raised when set to true
        /// </summary>
        public bool SuppressChangeNotification
        {
            get { return _SuppressChangeNotification; }
            set
            {
                if (value && DisallowNotificationSuppression)
                {
                    throw new InvalidOperationException("Change notification suppression is not permitted on this instance");
                }

                _SuppressChangeNotification = value;
            }
        }

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

        /// <summary>
        /// Occurs upon the alteration of the content or order of items in the collection
        /// </summary>
        public virtual event NotifyCollectionChangedEventHandler CollectionChanged;
        #endregion Interface implementation

        /// <summary>
        /// Occurs when an item in this collection raises PropertyChanged
        /// </summary>
        public virtual event ItemPropertyChangedEventHandler ItemPropertyChanged;
        /// <summary>
        /// Occurs when the collection is cleared or reloaded with a set of items
        /// </summary>
        public virtual event NotifyCollectionChangedEventHandler CollectionReset;
        /// <summary>
        /// Occurs when an item is added to the collection
        /// </summary>
        public virtual event NotifyCollectionChangedEventHandler ItemInserted;
        /// <summary>
        /// Occurs when an item in the collection is moved from one index to another
        /// </summary>
        public virtual event NotifyCollectionChangedEventHandler ItemMoved;
        /// <summary>
        /// Occurs when an item in the collection is replaced
        /// </summary>
        public virtual event NotifyCollectionChangedEventHandler ItemReplaced;
        /// <summary>
        /// Occurs when an item is removed from the collection
        /// </summary>
        public virtual event NotifyCollectionChangedEventHandler ItemRemoved;

        protected virtual event PropertyChangedEventHandler PropertyChanged;

        #region Collection alteration methods
        /// <summary>
        /// Removes all items in the collection
        /// </summary>
        protected override void ClearItems()
        {
            AssertNotInEventHandler();

            T[] items = new T[Count];
            CopyTo(items, 0);

            base.ClearItems();

            if(!DisableAutoPropertyChangedSubscription)
            {
                UnsubscribeFromItemEvents(items);
            }

            if(!SuppressChangeNotification)
            {
                OnCountAndIndexerNameChanged();
                OnCollectionReset();
            }
        }

        /// <summary>
        /// Removes item at the specified index
        /// </summary>
        protected override void RemoveItem(int index)
        {
            AssertNotInEventHandler();

            T removed = this[index];
            base.RemoveItem(index);

            if(!DisableAutoPropertyChangedSubscription)
            {
                UnsubscribeFromItemEvents(removed);
            }

            if (!SuppressChangeNotification)
            {
                OnCountAndIndexerNameChanged();
                OnInsertedOrRemovedItem(false, removed, index);
            }
        }

        /// <summary>
        /// Inserts item into the collection at the specified index
        /// </summary>
        protected override void InsertItem(int index, T item)
        {
            AssertNotInEventHandler();

            base.InsertItem(index, item);

            if(!DisableAutoPropertyChangedSubscription)
            {
                SubscribeToItemEvents(item);
            }

            if (!SuppressChangeNotification)
            {
                OnCountAndIndexerNameChanged();
                OnInsertedOrRemovedItem(true, item, index);
            }
        }

        /// <summary>
        /// Replaces the item currently at the given index with the given item
        /// </summary>
        protected override void SetItem(int index, T item)
        {
            AssertNotInEventHandler();

            T old = this[index];
            base.SetItem(index, item);

            if(!DisableAutoPropertyChangedSubscription)
            {
                UnsubscribeFromItemEvents(old);
                SubscribeToItemEvents(item);
            }

            if (!SuppressChangeNotification)
            {
                OnIndexerNameChanged();
                OnReplacedItem(old, item, index);
            }
        }

        /// <summary>
        /// Removes the item at an index and reinserts it at another
        /// </summary>
        /// <param name="currentIndex">Current index of item being moved</param>
        /// <param name="newIndex">New index for item</param>
        protected virtual void MoveItem(int currentIndex, int newIndex)
        {
            AssertNotInEventHandler();

            T item = this[currentIndex];

            base.RemoveItem(currentIndex);
            base.InsertItem(newIndex, item);

            if (!SuppressChangeNotification)
            {
                OnIndexerNameChanged();
                OnMovedItem(item, newIndex, currentIndex);
            }
        }
        #endregion Collection alteration methods

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
            
            // we only raise the reset event when: a) we are explicitly permitted to,
            // b) we didn't already raise multiple events during the load process,
            // and c) we are not suppressing change notifications
            if(raiseResetEvent && !raiseMultipleEventsOnLoad && !SuppressChangeNotification)
            {
                OnCollectionReset();
            }
        }

        private void ConfigureInstance(ObservableCollectionPlusOptions config)
        {
            Options = config;

            // determine if we can/should automatically subscribe to PropertyChanged events on items
            if(!DisableAutoPropertyChangedSubscription)
            {
                // generic type T must implement INotifyPropertyChanged and
                // have a publicly accessible PropertyChanged property
                var itemType = typeof(T);
                if ((itemType.GetInterface("INotifyPropertyChanged", true) == null) ||
                    (itemType.GetEvent("PropertyChanged") == null))
                {
                    Options |= ObservableCollectionPlusOptions.DisableAutoPropertyChangedSubscription;
                }
            }
        }

        #region Raise event methods
        /// <summary>
        /// Raises the PropertyChanged event
        /// </summary>
        /// <param name="e">Event arguments</param>
        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Delegate for the PropertyChanged event.  Raises the ItemPropertyChanged event when a
        /// PropertyChanged event is raised on an item in the collection
        /// </summary>
        /// <param name="item">Item raising the PropertyChanged event</param>
        /// <param name="e">Event arguments</param>
        /// <remarks>While <see cref="SuppressChangeNotification"/> is true, neither the 
        /// CollectionChanged nor the ItemPropertyChanged events are raised</remarks>
        private void OnItemPropertyChanged(object item, PropertyChangedEventArgs e)
        {
            if(!SuppressChangeNotification)
            {
                var args = new ItemPropertyChangedEventArgs(item, e.PropertyName);
                ItemPropertyChanged?.Invoke(this, args);
            }
        }
        /// <summary>
        /// Raises the CollectionChanged event
        /// </summary>
        /// <remarks>This method uses a counter to track invocations of event handlers.
        /// This is necessary because we want to ensure that the collection is not altered
        /// while event handlers are executing</remarks>
        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged == null) { return; }

            lock (HandlerCountLock)
            {
                HandlerCount++;
            }

            CollectionChanged(this, e);

            lock (HandlerCountLock)
            {
                HandlerCount--;
            }
        }

        private void OnCountChanged()
        {
            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
        }
        private void OnIndexerNameChanged()
        {
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        }
        private void OnCountAndIndexerNameChanged()
        {
            OnCountChanged();
            OnIndexerNameChanged();
        }

        private void OnInsertedOrRemovedItem(bool insertedItem, T item, int index)
        {
            var args = new NotifyCollectionChangedEventArgs(
                insertedItem ? NotifyCollectionChangedAction.Add : NotifyCollectionChangedAction.Remove,
                item, index);
            if(args.Action == NotifyCollectionChangedAction.Add)
            {
                ItemInserted?.Invoke(this, args);
            }
            else
            {
                ItemRemoved?.Invoke(this, args);
            }
            OnCollectionChanged(args);
        }
        private void OnMovedItem(T item, int index, int oldIndex)
        {
            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move,
                item, index, oldIndex);

            ItemMoved?.Invoke(this, args);
            OnCollectionChanged(args);
        }
        private void OnReplacedItem(T oldItem, T newItem, int index)
        {
            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
                newItem, oldItem, index);

            ItemReplaced?.Invoke(this, args);
            OnCollectionChanged(args);
        }
        private void OnCollectionReset()
        {
            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);

            CollectionReset?.Invoke(this, args);
            OnCollectionChanged(args);
        }

        private void SubscribeToItemEvents(T item)
        {
            if (item == null) { return; }
            ((INotifyPropertyChanged)item).PropertyChanged += OnItemPropertyChanged;
        }
        private void UnsubscribeFromItemEvents(T item)
        {
            if(item == null) { return; }
            ((INotifyPropertyChanged)item).PropertyChanged -= OnItemPropertyChanged;
        }
        private void UnsubscribeFromItemEvents(IList<T> items)
        {
            foreach(var item in items)
            {
                UnsubscribeFromItemEvents(item);
            }
        }
        #endregion Raise event methods

        private List<T> FilterCollection(Func<T, bool> predicate)
        {
            var subset = new List<T>();

            foreach(var item in Items)
            {
                if(predicate(item))
                {
                    subset.Add(item);
                }
            }

            return subset;
        }

        private bool DisallowNotificationSuppression
        {
            get
            {
                return (ObservableCollectionPlusOptions.DisallowNotificationSuppression & Options) ==
                    ObservableCollectionPlusOptions.DisallowNotificationSuppression;
            }
        }
        private bool DisableAutoPropertyChangedSubscription
        {
            get
            {
                return (ObservableCollectionPlusOptions.DisableAutoPropertyChangedSubscription & Options) ==
                    ObservableCollectionPlusOptions.DisableAutoPropertyChangedSubscription;
            }
        }

        private ObservableCollectionPlusOptions Options = ObservableCollectionPlusOptions.Default;

        private bool _SuppressChangeNotification = false;

        private object HandlerCountLock = new object();
        private int HandlerCount = 0;

        private const string ChangeHookEx = "Utilization or removal of change event responders is not permitted on this instance";
    }
}
