# Introduction

This is a C# library designed for effectively handling tree structures.  
Emphasizing flexibility and extensibility, the library allows easy manipulation of various tree structures.

Features:

1. Rich extension methods for `ITreeNode<TNode>`
1. Achieving mutual references between parent and child nodes
1. Classes forming a tree structure and their Generality
1. Bidirectional Conversion between Different Data Structures and Tree Structures.


These are the four main features of the library.

[Download Nuget Package](https://www.nuget.org/packages/TreeStructures/)  
 
## Usage
To be documented in the [wiki.](https://github.com/Houzkin/TreeStructures/wiki)

## Concept
This library does not aim to be standalone.   
Various useful libraries are already available, so we aim to handle tree structure-related functions while coexisting with other libraries.  
  
Let's elaborate on the four features mentioned at the beginning.

### Rich Extension Methods
Over 60 extension methods for `ITreeNode<TNode>` are defined, including various overloads.    
Examples include:

Enumeration: `Preorder`, `Levelorder`, all traversal methods, `Leafs`, `Ancestors`, `DiscendArrivals`, `DescendTraces`, etc.  
Navigation: `Root`, `NextSibling`, `LastSibling`, etc.   
Editing: Including `TryAddChild`, Try●●Child, `Disassemble`, `RemoveAllDescendant`, etc.  
Parameter Retrieval: `NodeIndex`, `NodePath`, `Height`, `Depth`, etc.  
Predicate Methods: `IsDescendantOf`, `IsAncestorOf`, `IsRoot`, etc.  
Conversion: `ToNodeMap`, `ToSerializableNodeMap`, `ToTreeDiagram`, `AsValuedTreeNode`  
Assembly Methods: `Convert`, `AssembleTree`, `AssembleAsNAryTree`,`AssembleForestByPath`

### Mutual References Between Parent and Child Nodes
Mutual references between parent and child nodes are handled by base classes (`TreeNodeBase<TNode>` or `HierarchyWrapper<TSrc,TWrpr>`). 

In the derived types of `TreeNodeBase<TNode>`, customization can be achieved through protected virtual methods such as `RemoveChildProcess` and `InsertChildProcess`, which are defined as `●●ChildProcess` methods.

### Classes Forming a Tree Structure and Their Generality
If you want to customize in detail, use `TreeNodeBase<TNode>`.   
For a GeneralTree, if you want to use it as a data structure or container for data, use `GeneralTreeNode<TNode>` or `ObservableTreeNode<TNode>`.   
If you want to use an N-Ary Tree with a fixed number of branches and empty nodes set to null, use `NAryTreeNode<TNode>`.   
If you want to use it as a wrapper for objects or tree structure that forms the hierarchy, use `(Hierarchy | TreeNode) Wrapper<TSrc,TWrpr>`.   
If you need to handle resource disposal and observation, along with its role as a wrapper (e.g., ViewModel in MVVM), inherit and use `Bindable(Hierarchy | TreeNode)Wrapper<TSrc,TWrpr>`.


In `TreeNodeBase<TNode>` and its derived types, you can customize the collections used internally and those exposed externally by overriding the `Setup(Inner | Public)ChildCollection` methods.  
`HierarchyWrapper<TSrc,TWrpr>` and its derived types allow customization only of the collection exposed externally.  
  
### Bidirectional Conversion between Different Data Structures and Tree Structures.
Support is provided for exte `ITreeNode<TNode>` methods to objects that do not implement `ITreeNode<TNode>`.  
This is achieved by wrapping objects in a `HierarchyWrapper<TSrc,TWrpr>` or `BindableHierarchyWrapper<TSrc,TWrpr>`, or by calling `AsValuedTreeNode` to provide the extension methods of `ITreeNode<TNode>`.  
  
Furthermore, various methods for mutual conversion, such as `Convert`, `AssembleTree`, and `ToNodeMap`, are available through extension methods.  



## Namespaces Classification

### TreeStructures;
　Abstract classes define generic tree nodes, peripheral objects, and event arguments.

Inheritance diagram of generic tree nodes  
![InheritanceGenericTreeNode](https://raw.githubusercontent.com/Houzkin/TreeStructures/master/images/InheritanceGenericTreeNode.png)

Inheritance diagram of NodePath and NodeIndex (peripheral objects)  
![InheritancePeripheralObjects](https://raw.githubusercontent.com/Houzkin/TreeStructures/master/images/InheritancePeripheralObjects.png)

### TreeStructures.Linq;
　Extension methods for `ITreeNode<TNode>`, `IMutableTreeNode<TNode>`, `IEnumerable<T>`
### TreeStructures.Utility;
　Definition of `ResultWithValue<T>` used as a return value for Try●● methods
### TreeStructures.Collections;
　It defines collections used in internal implementations or processing steps of extension methods.  
　These include the synchronizable `ImitableCollection<TSrc,TConv>`,  
　the combinable observable collection `CombinableObservableCollection<T>`,  
　and the `ReadOnlySortFilterObservableCollection<T>` that applies sorting and filtering while synchronizing with an observable collection.

### TreeStructures.EventManagement;
　Objects used in event handling, implementing Observable tree nodes
### TreeStructures.Xml.Serialization;
　Dictionary and others used in serialization and deserialization
### TreeStrucutures.Tree;
　Purpose-specific trees

