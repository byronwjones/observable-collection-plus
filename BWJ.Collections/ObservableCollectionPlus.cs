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
        /// <param name="list">The initial dataset managed by this collection</param>
        public ObservableCollectionPlus(IList<T> list)
        {

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
            Action<T> onRemove = null)
        {

        }

        /// <summary>
        /// Initialization of the collection with a dataset, configuration arguments,
        /// and optional event responders
        /// </summary>
        /// <param name="list">The initial dataset managed by this collection</param>
        /// <param name="config">Instance configuration options</param>
        /// <param name="onClear">Action invoked when this collection is cleared</param>
        /// <param name="onInsert">Action invoked when an item is added to this collection</param>
        /// <param name="onMove">Action invoked when an item in this collection is moved</param>
        /// <param name="onRemove">Action invoked when an item is removed from this collection</param>
        public ObservableCollectionPlus(IList<T> list,
            ObservableCollectionPlusOptions config,
            Action onClear = null,
            Action<T> onInsert = null,
            Action<T> onMove = null,
            Action<T> onRemove = null)
        {

        }
        #endregion Ctors

        #region Change occurence hooks
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
                    throw new Exception(ChangeHookEx);
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
                    throw new Exception(ChangeHookEx);
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
                    throw new Exception(ChangeHookEx);
                }
                _OnMove = value;
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
                    throw new Exception(ChangeHookEx);
                }
                _OnRemove = value;
            }
        }
        #endregion Change event hooks

        private bool DisallowChangeResponders
        {
            get
            {
                return (ObservableCollectionPlusOptions.DisallowChangeResponders & Options) ==
                    ObservableCollectionPlusOptions.DisallowChangeResponders;
            }
        }

        private ObservableCollectionPlusOptions Options = ObservableCollectionPlusOptions.Default;

        private Action DoNothing = () => { };
        private Action<T> DoNothingWithArg = (t) => { };
        private Action _OnClear = null;
        private Action<T> _OnInsert = null;
        private Action<T> _OnMove = null;
        private Action<T> _OnRemove = null;

        private string ChangeHookEx = "Utilization or removal of change event hooks is not permitted";
    }
}
