# ObservableCollectionPlus
A flexible, highly-performant Collection implementing [INotifyPropertyChanged](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged) and [INotifyCollectionChanged](https://docs.microsoft.com/en-us/dotnet/api/system.collections.specialized.inotifycollectionchanged) with a few nice features:
 - Automatically configures itself to detect changes to Item properties when the Item implements **INotifyPropertyChanged**, providing feedback via its **ItemPropertyChangedEvent**
 - Replace one set of items with another set, optionally raising only one event when this occurs
 - Ability to suspend or resume raising property or collection change events at any time
 - In addition to a generic **CollectionChanged** event which is raised when the collection changes in any way, subscribe to a specific collection change event via the **CollectionReset**, **ItemInserted**, **ItemMoved**, **ItemReplaced**, and **ItemRemoved** events

 ### Namespace
 BWJ.Collections

 ### Inheritance
 Derived from [System.Collections.ObjectModel.Collection&lt;T&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.objectmodel.collection-1)

 
### Constructors
|||
|---|---|
ObservableCollectionPlus&lt;T&gt;() | Initialize a new instance of the ObservableCollectionPlus&lt;T&gt; class|
ObservableCollectionPlus&lt;T&gt;(IEnumerable&lt;T&gt; collection) | Initialize a new instance of the ObservableCollectionPlus&lt;T&gt; class with a set of items<br/><br/>*collection:* An initial set of items copied to the collection|
ObservableCollectionPlus&lt;T&gt;(ObservableCollectionPlusOptions config) | Initialize a new instance of the ObservableCollectionPlus&lt;T&gt; class with options<br/><br/>*config:* One or more bitwise options to alter the default behavior of the instance|
ObservableCollectionPlus&lt;T&gt;(IEnumerable&lt;T&gt; collection, ObservableCollectionPlusOptions config) | Initialize a new instance of the ObservableCollectionPlus&lt;T&gt; class with a set of items and options<br/><br/>*collection:* An initial set of items copied to the collection<br/>*config:* One or more bitwise options to alter the default behavior of the instance|

### Properties
|||
|---|---|
public bool SuppressChangeNotification | When set to true, prevents the generic *CollectionChanged*, specific collection change, and *ItemPropertyChanged* events from being raised|
**Inherited Properties:** | See documentation for [Collection&lt;T&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.objectmodel.collection-1)|

### Methods
|||
|---|---|
public void Load(IEnumerable&lt;T&gt; collection, bool raiseMultipleEventsOnLoad = false) | Clears the previous items and reloads the collection with the given items<br/><br/>*collection:* Collection of items to copy to this instance<br/>*raiseMultipleEventsOnLoad:* When true, will raise the *CollectionChanged* and *CollectionReset* events, then the *CollectionChanged* and *ItemInserted* events for each item added. Otherwise, only the *CollectionChanged* and *CollectionReset* events are raised once after the loading process completes. *SuppressChangeNotification* is ignored when this argument is set to true, as setting this value to true signifies intent to have events raised|
public void Move(int currentIndex, int newIndex) | Move an item from a given index to a given index<br/><br/>*currentIndex:* Index where the target item is currently located<br/>*newIndex:* Index to move the target item to|
public void Move(T item, int newIndex) | Move the first occurrence of the given item to the given index<br/><br/>*item:* Item to move<br/>*newIndex:* Index to move the item to|
public void Replace(T item, T replacement) | Replace the first occurence of the given item with the given replacement item<br/><br/>*item:* Item to replace<br/>*replacement:* Replacement item|
public List&lt;T&gt; Replace(Func&lt;T, bool&gt; predicate, T replacement) | Replaces every item matching the given predicate with the given replacement<br/><br/>*predicate:* Predicate function returning true when argument T is an item to be replaced<br/>*replacement:* Replacement item<br/><br/>**Returns** The items replaced|
public List&lt;T&gt; RemoveWhere(Func&lt;T, bool&gt; predicate) | Removes every item matching the given predicate from the collection<br/><br/>*predicate:* Predicate function returning true when argument T is an item to be removed<br/><br/>**Returns** The items removed|
**Inherited Methods:** | See documentation for [Collection&lt;T&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.objectmodel.collection-1)|

### Events
|||
|---|---|
protected virtual event PropertyChangedEventHandler PropertyChanged | Occurs when a public property on the collection changes (excluding *SuppressChangeNotification*)|
public virtual event ItemPropertyChangedEventHandler ItemPropertyChanged | Occurs when an item in this collection raises a PropertyChanged event|
public virtual event NotifyCollectionChangedEventHandler CollectionChanged | Occurs upon any alteration of the content or order of items in the collection|
public virtual event NotifyCollectionChangedEventHandler CollectionReset | Occurs when the collection is cleared or reloaded with a set of items|
public virtual event NotifyCollectionChangedEventHandler ItemInserted | Occurs when an item is added to the collection|
public virtual event NotifyCollectionChangedEventHandler ItemMoved | Occurs when an item in the collection is moved from one index to another|
public virtual event NotifyCollectionChangedEventHandler ItemReplaced | Occurs when an item in the collection is replaced|
public virtual event NotifyCollectionChangedEventHandler ItemRemoved | Occurs when an item is removed from the collection|


## ObservableCollectionPlusOptions
ObservableCollectionPlus configuration options. Multiple options values can be specified using a bitwise operator, e.g.:
```C#
// Do not allow suspension of normal change event occurences,
// AND do not automatically subscribe to the PropertyChanged event on collection items
var options = DisallowNotificationSuppression | DisableAutoPropertyChangedSubscription;
```

|||
|---|---|
Default | Implement default collection behavior, i.e.:<br/><br/> - The occurence of change events can be suspended and/or reenabled at any time<br/> - When type param T implements INotifyPropertyChanged, items are automatically configured for change detection.  This means that the collection subscribes to each item's PropertyChanged event when it is added, and unsubscribes from when it is removed|
DisallowNotificationSuppression | Do not allow suspension of normal change event firing.  When this option is activated, setting *SuppressChangeNotification* will result in an *InvalidOperationException*|
DisableAutoPropertyChangedSubscription | Do not automatically subscribe to the PropertyChanged event on collection items when type param T implements INotifyPropertyChanged and has a public PropertyChanged event|

## ItemPropertyChangedEventHandler
Delegate for *ItemPropertyChangedEvent*

 ### Namespace
 BWJ.Collections

 ### Inheritance
 Derived from [System.Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)

 ### Arguments
|||
|---|---|
*sender:* | The object that raised the event|
*e:* | Information related to the event|

## ItemPropertyChangedEventArgs
Encapsulates information related to the occurence of an *ItemPropertyChangedEvent*

 ### Namespace
 BWJ.Collections

 ### Inheritance
 Derived from [System.ComponentModel.PropertyChangedEventArgs](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.propertychangedeventargs)

### Properties
|||
|---|---|
public object Item | The object instance containing the property which changed|
**Inherited Properties:** | See documentation for [PropertyChangedEventArgs](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.propertychangedeventargs)|
