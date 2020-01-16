using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;

namespace BWJ.Collections.Tests
{
    [TestClass]
    public class ObservableCollectionPlusTests
    {
        #region Configuration
        [TestMethod]
        public void Test_DisallowNotificationSuppression()
        {
            var collection = new ObservableCollectionPlus<string>
                (ObservableCollectionPlusOptions.DisallowNotificationSuppression);

            Action act = () => {
                collection.SuppressChangeNotification = true;
            };

            Assert.ThrowsException<InvalidOperationException>(act);
        }

        [TestMethod]
        public void Test_DisallowSettingChangeRespondersAfterInstanciation()
        {
            var collection = new ObservableCollectionPlus<string>
                (ObservableCollectionPlusOptions.DisallowChangeResponders,
                onClear: (l) => { });

            Action act = () => {
                collection.OnInsert = (i) => { };
            };

            Assert.ThrowsException<InvalidOperationException>(act);
        }
        #endregion Configuration

        #region Item Insertion
        [TestMethod]
        public void Test_AddItemToCollection()
        {
            var collection = new ObservableCollectionPlus<string>();

            collection.Add("test");

            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual("test", collection[0]);
        }

        [TestMethod]
        public void Test_FireEventOnItemInsertion()
        {
            var collection = new ObservableCollectionPlus<string>();
            NotifyCollectionChangedAction action = NotifyCollectionChangedAction.Reset;
            string item = null;
            collection.CollectionChanged += (sender, e) => {
                action = e.Action;
                item = (string)e.NewItems[0];
            };

            collection.Add("test");

            Assert.AreEqual(NotifyCollectionChangedAction.Add, action);
            Assert.AreEqual("test", item);
        }

        [TestMethod]
        public void Test_InvokeResponderOnItemInsertion()
        {
            string item = null;
            Action<string> responder = (s) => {
                item = s;
            };
            var collection = new ObservableCollectionPlus<string>(ObservableCollectionPlusOptions.Default,
                onClear: null,
                onInsert: responder);

            collection.Add("test");

            Assert.AreEqual("test", item);
        }
        #endregion Item Insertion

        #region Item Translation
        [TestMethod]
        public void Test_MoveCollectionItemByIndex()
        {
            var collection = new ObservableCollectionPlus<string>
                (new List<string> { "foo", "bar", "baz" });

            collection.Move(0, 2);

            Assert.AreEqual("bar,baz,foo", string.Join(",", collection));
        }

        [TestMethod]
        public void Test_MoveCollectionItemByRef()
        {
            var collection = new ObservableCollectionPlus<string>
                (new List<string> { "foo", "bar", "baz" });

            collection.Move("foo", 2);

            Assert.AreEqual("bar,baz,foo", string.Join(",", collection));
        }

        [TestMethod]
        public void Test_FireEventOnItemMovement()
        {
            var collection = new ObservableCollectionPlus<string>
                (new List<string> { "foo", "bar", "baz" });
            NotifyCollectionChangedAction action = NotifyCollectionChangedAction.Reset;
            string item = null;
            int index = 0;
            collection.CollectionChanged += (sender, e) => {
                action = e.Action;
                item = (string)e.NewItems[0];
                index = e.NewStartingIndex;
            };

            collection.Move(0, 2);

            Assert.AreEqual(NotifyCollectionChangedAction.Move, action);
            Assert.AreEqual("foo", item);
            Assert.AreEqual(2, index);
        }

        [TestMethod]
        public void Test_InvokeResponderOnItemMovement()
        {
            string item = null;
            Action<string> responder = (s) => {
                item = s;
            };
            var collection = new ObservableCollectionPlus<string>(
                new List<string> { "foo", "bar", "baz" },
                ObservableCollectionPlusOptions.Default,
                onClear: null,
                onInsert: null,
                onMove: responder);

            collection.Move(0, 2);

            Assert.AreEqual("foo", item);
        }
        #endregion Item Translation

        #region Item Replacement
        [TestMethod]
        public void Test_ReplaceCollectionItemByIndex()
        {
            var collection = new ObservableCollectionPlus<string>
                (new List<string> { "foo", "bar", "baz" });

            collection[1] = "spartacus";

            Assert.AreEqual("foo,spartacus,baz", string.Join(",", collection));
        }

        [TestMethod]
        public void Test_ReplaceCollectionItemByRef()
        {
            var collection = new ObservableCollectionPlus<string>
                (new List<string> { "foo", "bar", "baz" });

            collection.Replace("bar", "spartacus");

            Assert.AreEqual("foo,spartacus,baz", string.Join(",", collection));
        }

        [TestMethod]
        public void Test_ReplaceCollectionItemByPredicate()
        {
            var collection = new ObservableCollectionPlus<string>
                (new List<string> { "foo", "bar", "baz" });

            collection.Replace(s => s[1] == 'o', "horse");

            Assert.AreEqual("horse,bar,baz", string.Join(",", collection));
        }

        [TestMethod]
        public void Test_FireEventOnItemReplacement()
        {
            var collection = new ObservableCollectionPlus<string>
                (new List<string> { "foo", "bar", "baz" });
            NotifyCollectionChangedAction action = NotifyCollectionChangedAction.Reset;
            string oldItem = null, newItem = null;
            collection.CollectionChanged += (sender, e) => {
                action = e.Action;
                oldItem = (string)e.OldItems[0];
                newItem = (string)e.NewItems[0];
            };

            collection[1] = "spartacus";

            Assert.AreEqual(NotifyCollectionChangedAction.Replace, action);
            Assert.AreEqual("bar", oldItem);
            Assert.AreEqual("spartacus", newItem);
        }

        [TestMethod]
        public void Test_InvokeResponderOnItemReplacement()
        {
            string oldItem = null, newItem = null;
            Action<string, string> responder = (nu, old) => {
                oldItem = old;
                newItem = nu;
            };
            var collection = new ObservableCollectionPlus<string>(
                new List<string> { "foo", "bar", "baz" },
                ObservableCollectionPlusOptions.Default,
                onClear: null,
                onInsert: null,
                onMove: null,
                onReplace: responder);

            collection[0] = "banana";

            Assert.AreEqual("foo", oldItem);
            Assert.AreEqual("banana", newItem);
        }
        #endregion Item Replacement

        #region Item Removal
        [TestMethod]
        public void Test_RemoveCollectionItem()
        {
            var collection = new ObservableCollectionPlus<string>
                (new List<string> { "foo", "bar", "baz" });

            collection.Remove("bar");

            Assert.AreEqual("foo,baz", string.Join(",", collection));
        }
        
        [TestMethod]
        public void Test_RemoveCollectionItemByPredicate()
        {
            var collection = new ObservableCollectionPlus<string>
                (new List<string> { "foo", "bar", "baz" });

            collection.RemoveWhere(s => s[0] == 'b');

            Assert.AreEqual("foo", string.Join(",", collection));
        }
        
        [TestMethod]
        public void Test_FireEventOnItemRemoval()
        {
            var collection = new ObservableCollectionPlus<string>
                (new List<string> { "foo", "bar", "baz" });
            NotifyCollectionChangedAction action = NotifyCollectionChangedAction.Reset;
            string removed = null;
            collection.CollectionChanged += (sender, e) => {
                action = e.Action;
                removed = (string)e.OldItems[0];
            };

            collection.RemoveAt(1);

            Assert.AreEqual(NotifyCollectionChangedAction.Remove, action);
            Assert.AreEqual("bar", removed);
        }
        
        [TestMethod]
        public void Test_InvokeResponderOnItemRemoval()
        {
            string removed = null;
            Action<string> responder = (r) => {
                removed = r;
            };
            var collection = new ObservableCollectionPlus<string>(
                new List<string> { "foo", "bar", "baz" },
                ObservableCollectionPlusOptions.Default,
                onClear: null,
                onInsert: null,
                onMove: null,
                onReplace: null,
                onRemove: responder);

            collection.RemoveAt(1);

            Assert.AreEqual("bar", removed);
        }
        #endregion Item Removal

        #region Clear All Items
        [TestMethod]
        public void Test_ClearCollection()
        {
            var collection = new ObservableCollectionPlus<string>
                (new List<string> { "foo", "bar", "baz" });

            collection.Clear();

            Assert.AreEqual(0, collection.Count);
        }
        
        [TestMethod]
        public void Test_FireEventOnCollectionCleared()
        {
            var collection = new ObservableCollectionPlus<string>
                (new List<string> { "foo", "bar", "baz" });
            NotifyCollectionChangedAction action = NotifyCollectionChangedAction.Add;
            collection.CollectionChanged += (sender, e) => {
                action = e.Action;
            };

            collection.Clear();

            Assert.AreEqual(NotifyCollectionChangedAction.Reset, action);
        }
        
        [TestMethod]
        public void Test_InvokeResponderOnCollectionCleared()
        {
            string items = null;
            Action<IList<string>> responder = (s) => {
                items = string.Join(",", s);
            };
            var collection = new ObservableCollectionPlus<string>(
                new List<string> { "foo", "bar", "baz" },
                ObservableCollectionPlusOptions.Default,
                onClear: responder);

            collection.Clear();

            Assert.AreEqual("foo,bar,baz", items);
        }
        #endregion Clear All Items

        #region Load New Collection
        [TestMethod]
        public void Test_LoadCollection()
        {
            var collection = new ObservableCollectionPlus<string>
                (new List<string> { "a", "b", "c" });

            collection.Load(new List<string> { "x", "y", "z" });

            Assert.AreEqual("x,y,z", string.Join(",", collection));
        }
        
        [TestMethod]
        public void Test_FireOneEventOnLoad()
        {
            var collection = new ObservableCollectionPlus<string>();
            var events = new List<NotifyCollectionChangedAction>();
            collection.CollectionChanged += (sender, e) => {
                events.Add(e.Action);
            };

            collection.Load(new List<string> { "a", "b", "c" });

            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(NotifyCollectionChangedAction.Reset, events[0]);
        }

        [TestMethod]
        public void Test_FireMultipleEventsOnLoad()
        {
            var collection = new ObservableCollectionPlus<string>
                (new List<string> { "a", "b", "c" });
            var events = new List<NotifyCollectionChangedAction>();
            int resetEventCount = 0;
            int addEventCount = 0;
            collection.CollectionChanged += (sender, e) => {
                events.Add(e.Action);
            };

            collection.Load(new List<string> { "x", "y", "z" }, raiseMultipleEventsOnLoad: true);
            resetEventCount = events.Count(e => e == NotifyCollectionChangedAction.Reset);
            addEventCount = events.Count(e => e == NotifyCollectionChangedAction.Add);

            // should raise 4 events total:
            //     - 1 reset collection event
            //     - 3 add item events
            Assert.AreEqual(4, events.Count);
            Assert.AreEqual(1, resetEventCount);
            Assert.AreEqual(3, addEventCount);
        }
        #endregion Load New Collection

        #region Item Property Change Notification
        [TestMethod]
        public void Test_AutomaticItemPropertyChangeNotification()
        {
            var objA = new InpcObject();
            var objB = new InpcObject();
            string notifications = string.Empty;
            var collection = new ObservableCollectionPlus<InpcObject>
                (new List<InpcObject> { objA });
            collection.ItemPropertyChanged += (sender, e) => {
                notifications += e.PropertyName;
            };
            collection.Add(objB);

            objA.A = 1;
            objB.B = 2;
            objB.C = 3;

            Assert.AreEqual("ABC", notifications);
        }

        [TestMethod]
        public void Test_DisableAutomaticItemPropertyChangeNotification()
        {
            var objA = new InpcObject();
            var objB = new InpcObject();
            string notifications = string.Empty;
            var collection = new ObservableCollectionPlus<InpcObject> (
                new List<InpcObject> { objA },
                ObservableCollectionPlusOptions.DisableAutoPropertyChangedSubscription);
            collection.ItemPropertyChanged += (sender, e) => {
                notifications += e.PropertyName;
            };
            collection.Add(objB);

            objA.A = 1;
            objB.B = 2;

            Assert.AreEqual(string.Empty, notifications);
        }

        [TestMethod]
        public void Test_AutomaticUnsubscribeFromRemovedItemPropertyChanges()
        {
            var objA = new InpcObject();
            var objB = new InpcObject();
            string notifications = string.Empty;
            var collection = new ObservableCollectionPlus<InpcObject>
                (new List<InpcObject> { objA, objB });
            collection.ItemPropertyChanged += (sender, e) => {
                notifications += e.PropertyName;
            };

            objA.A = 1;
            objB.B = 2;
            collection.Remove(objB);
            objB.C = 4;

            Assert.AreEqual("AB", notifications);
        }
        #endregion Item Property Change Notification

        [TestMethod]
        public void Test_SuppressNotifications()
        {
            var objA = new InpcObject();
            var objB = new InpcObject();
            int notifications = 0;
            var collection = new ObservableCollectionPlus<InpcObject>();
            collection.ItemPropertyChanged += (sender, e) => {
                notifications++;
            };
            collection.CollectionChanged += (sender, e) => {
                notifications++;
            };

            collection.Load(new List<InpcObject> { objA });
            collection.SuppressChangeNotification = true;
            collection.Add(objB);
            objA.A = 1;
            collection.SuppressChangeNotification = false;
            objB.B = 2;

            Assert.AreEqual(2, notifications);
        }

        [TestMethod]
        public void Test_NoCollectionChangesAllowedInCollectionChangedHandler()
        {
            var objA = new InpcObject();
            var objB = new InpcObject();
            var objC = new InpcObject();
            var collection = new ObservableCollectionPlus<InpcObject>
                (new List<InpcObject> { objA });
            collection.CollectionChanged += (sender, e) => {
                collection.Add(objC);
            };

            Action act = () => {
                collection.Add(objB);
            };

            Assert.ThrowsException<InvalidOperationException>(act);
        }
    }
}
