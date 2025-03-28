# Introduction
This is a C# library designed to effectively handle tree structures.  
The library focuses on flexibility and extensibility, making it easy to manipulate various tree structures.

Key features:
1. Rich extension methods for `ITreeNode<TNode>`
1. Mutual references between parent and child nodes
1. A set of classes forming tree structures with extensibility
1. Conversion between different data structures and tree structures
1. Generic utility classes needed for implementing the above features

[Nuget TreeStructures](https://www.nuget.org/packages/TreeStructures/)

# Usage
Refer to the [wiki](https://github.com/Houzkin/TreeStructures/wiki/Home_ja)  

# Concept
This library is not intended to be a complete solution on its own.  
It aims to provide tree structure functionality while maintaining compatibility with other libraries.

Below is a detailed explanation of the key features.

## Rich Extension Methods
Over 60 extension methods are defined for `ITreeNode<TNode>`, including overloads.  
For example:  
Enumeration: `Preorder`, `Levelorder`, all traversal methods, `Leafs`, `Ancestors`, `DescendArrivals`, `DescendTraces`, etc.  
Navigation: `Root`, `NextSibling`, `LastSibling`, etc.  
Editing: `TryAddChild`, `Try○○Child`, `Disassemble`, `RemoveAllDescendant`, etc.  
Retrieving parameters: `NodeIndex`, `NodePath`, `Height`, `Depth`, etc.  
Validation methods: `IsDescendantOf`, `IsAncestorOf`, `IsRoot`, etc.  
Conversion: `ToNodeMap`, `ToSerializableNodeMap`, `ToTreeDiagram`, `AsValuedTreeNode`  
Tree construction: `Convert`, `AssembleTree`, `AssembleAsNAryTree`, `AssembleForestByPath`

## Mutual References Between Parent and Child Nodes
Mutual references between parent and child nodes are handled in the base classes (`TreeNodeBase` or `HierarchyWrapper`).  
In `TreeNodeBase` derivatives, behavior can be customized by overriding `protected virtual` methods such as `RemoveChildProcess` and `InsertChildProcess`.

## A Set of Tree Structure Classes and Their Versatility
Depending on the use case, the following classes can be inherited and used:

- **TreeNodeBase**: When fine-tuning method definitions is necessary
- **GeneralTreeNode / ObservableTreeNode**: For use as a data structure or container
- **NAryTreeNode**: For an N-ary tree where empty nodes are represented as `null`
- **HierarchyWrapper / TreeNodeWrapper**: To wrap hierarchical structures
- **BindableHierarchyWrapper / BindableTreeNodeWrapper**: For MVVM ViewModels that require observability and disposal

In `TreeNodeBase` and its derivatives, the internal collection used and the collection exposed externally can be customized by overriding `Setup(Inner | Public)ChildCollection` methods.

For `HierarchyWrapper` and its derivatives, only the externally exposed collection can be customized.

## Conversion Between Different Data Structures and Tree Structures
Even objects that do not implement `ITreeNode<TNode>` can utilize the extension methods for `ITreeNode<TNode>`.  
By wrapping hierarchical structures using `HierarchyWrapper<TSrc,TWrpr>` or `BindableHierarchyWrapper<TSrc,TWrpr>`, or calling `AsValuedTreeNode`, the extension methods of `ITreeNode<TNode>` can be provided.  
Additionally, methods such as `Convert`, `AssembleTree`, and `ToNodeMap` enable various conversion options.

## Generic Utility Classes Needed for Implementation

Some classes used for internal implementation are also exposed.  

- `ListAligner<T,TList>`  
  Reorders a specified list through manipulation.
- `ImitableCollection<TSrc,TConv>`  
  A collection synchronized with a specified collection.
- `CombinableObservableCollection<T>`  
  A collection that merges multiple observable collections.
- `ReadOnlyObservableItemCollection<T>`  
  An observable collection that links with a specified collection and enables batch property observation of each element.
- `ReadOnlySortFilterObservableCollection<T>`  
  An observable collection that links with a specified collection and adds sorting and filtering functionality.
- `ListScroller<T>`  
  Provides navigation functionality within a collection.
- `UniqueOperationExecutor`  
  Controls operation uniqueness (prevents duplicate execution).
- `PropertyChangeProxy`  
  Assists in implementing `INotifyPropertyChanged`.
- `ResultWithValue<TValue>`, `ResultWith<TValue>`  
  Manages the result of Try methods.

And more.

# Namespaces and Their Classification

Namespaces are categorized by purpose and use case.

Only `TreeStructures.Tree` is grouped based on implementation rather than purpose, containing classes that form tree structures according to specific rules.

## TreeStructures;
Defines abstract generic tree nodes, related objects, and event arguments.

### Inheritance diagram of generic tree nodes:
![InheritanceGenericTreeNode](https://raw.githubusercontent.com/Houzkin/TreeStructures/master/images/InheritanceGenericTreeNode.png)

### Inheritance diagram of NodePath and NodeIndex (related objects):
![InheritancePeripheralObjects](https://raw.githubusercontent.com/Houzkin/TreeStructures/master/images/InheritancePeripheralObjects.png)

## TreeStructures.Linq;
Extension methods for `ITreeNode<TNode>`, `IMutableTreeNode<TNode>`, and `IEnumerable<T>`.

## TreeStructures.Collections;
Collections used in internal implementation and extension methods.

## TreeStructures.Events;
Event-related classes that support listener and notification implementation.

## TreeStructures.Results;
Defines `ResultWithValue<T>` used as the return value for Try○○ methods.

## TreeStructures.Utilities;
General-purpose classes that do not fit into the above categories.

## TreeStructures.Xml.Serialization;
Dictionaries and other structures used for serialization and deserialization.

## TreeStructures.Internals;
Classes used internally in the library but with limited general applicability.

## TreeStructures.Tree;
Classes that form tree structures based on specific purposes and use cases.

