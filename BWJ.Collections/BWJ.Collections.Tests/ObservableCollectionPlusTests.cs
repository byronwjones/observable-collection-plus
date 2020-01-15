using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Specialized;
using System.Collections.Generic;

namespace BWJ.Collections.Tests
{
    [TestClass]
    public class ObservableCollectionPlusTests
    {
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
    }
}
