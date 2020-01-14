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

        /// <summary>
        /// Move the item from a given index to a given index.
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

        #region Collection alteration methods
        /// <summary>
        /// Removes all items in the collection
        /// </summary>
        protected override void ClearItems()
        {
            AssertNotInEventHandler();

            base.ClearItems();
            OnClear();

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
            OnRemove(removed);

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
            OnInsert(item);

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
            OnReplace(item, old);

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
            OnMove(item);

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
            
            // we only raise the reset event when explicitly permitted to,
            // we didn't already raise multiple events during the load process,
            // and we are not suppressing change notifications
            if(raiseResetEvent && !raiseMultipleEventsOnLoad && !SuppressChangeNotification)
            {
                OnCollectionReset();
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
        /// Raises the CollectionChanged event
        /// </summary>
        /// <remarks>This method uses a counter to track invokations of event handlers.
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

        private void OnInsertedOrRemovedItem(bool insertedItem, object item, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                insertedItem ? NotifyCollectionChangedAction.Add : NotifyCollectionChangedAction.Remove,
                item, index));
        }
        private void OnMovedItem(object item, int index, int oldIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move,
                item, index, oldIndex));
        }
        private void OnReplacedItem(object oldItem, object newItem, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
                newItem, oldItem, index));
        }
        private void OnCollectionReset()
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
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

        private object HandlerCountLock = new object();
        private int HandlerCount = 0;

        private const string ChangeHookEx = "Utilization or removal of change event responders is not permitted on this instance";
    }
}
