using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Specialized;
using System.Collections.Generic;

namespace BWJ.Collections.Tests
{
    [TestClass]
    public class ObservableCollectionPlusTests
    {
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
    }
}
