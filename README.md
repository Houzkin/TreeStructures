# Introduction

This library provides a class for creating a tree structure that can be used by inheriting from it.

Features:

1. Rich extension methods for `ITreeNode<TNode>`
1. Achieving mutual references between parent and child nodes
1. Classes forming a tree structure and their extensibility
1. Compatibility with other libraries
These are the four main features of the library.

[Download Nuget Package](https://www.nuget.org/packages/TreeStructures/)  

## Namespaces Classification

### TreeStructures;
Abstract classes define generic tree nodes, peripheral objects, and event arguments.

Inheritance diagram of generic tree nodes

![継承図_汎用TreeNode](https://github.com/Houzkin/TreeStructure/assets/12586097/1a83bff7-4534-41e8-915f-4879e80da2cf)

Inheritance diagram of NodePath and NodeIndex (peripheral objects)

![継承図_周辺オブジェクト](https://github.com/Houzkin/TreeStructure/assets/12586097/9f17c735-3e0e-40dc-b374-d4d6b380b03a)

### TreeStructures.Linq;
　Extension methods for `ITreeNode<TNode>`, `IMutableTreeNode<TNode>`, `IEnumerable<T>`
### TreeStructures.Utility;
　Definition of `ResultWithValue<T>` used as a return value for Try○○ methods
### TreeStructures.Collections;
　Collections used in internal implementations and processing of extension methods
### TreeStructures.EventManagement;
　Objects used in event handling, implementing Observable tree nodes
### TreeStructures.Xml.Serialization;
　Dictionary and others used in serialization and deserialization
### TreeStrucutures.Tree;
　Purpose-specific trees
 
## Usage
To be documented in the wiki.

## Concept
This library does not aim to be standalone.   
Various useful libraries are already available, so we aim to handle tree structure-related functions while coexisting with other libraries.  
  
Let's elaborate on the four features mentioned at the beginning.

### Rich Extension Methods
Over 60 extension methods for `ITreeNode<TNode>` are defined, including various overloads.    
Examples include:

Enumeration: `Preorder`, `Levelorder`, all traversal methods, `Leafs`, `Ancestors`, `DiscendArrivals`, `DescendTraces`, etc.  
Navigation: `Root`, `NextSibling`, `LastSibling`, etc.   
Editing: Including `TryAddChild`, Try○○Child, `Disassemble`, `RemoveAllDescendant`, etc.  
Parameter Retrieval: `NodeIndex`, `NodePath`, `Height`, `Depth`, etc.  
Predicate Methods: `IsDescendantOf`, `IsAncestorOf`, `IsRoot`, etc.  
Conversion: `ToNodeMap`, `ToSerializableNodeMap`, `ToTreeDiagram`  
Assembly Methods: `Convert`, `AssembleTree`, `AssembleAsNAryTree`

### Mutual References Between Parent and Child Nodes
Mutual references between parent and child nodes are handled by base classes (`TreeNodeBase<TNode>` or `CompositeWrapper<TSrc,TWrpr>`). 

In the derived types of `TreeNodeBase<TNode>`, customization can be achieved through protected virtual methods such as `RemoveChildProcess` and `InsertChildProcess`, which are defined as `○○ChildProcess` methods.

### Classes Forming a Tree Structure and Their Generality
If you want to customize in detail, use `TreeNodeBase<TNode>`.   
For a GeneralTree, if you want to use it as a data structure or container for data, use `TreeNode<TNode>` or `ObservableTreeNode<TNode>`.   
If you want to use an N-Ary Tree with a fixed number of branches and empty nodes set to null, use `NAryTreeNode<TNode>`.   
If you want to use it as a wrapper for objects or tree structure that forms the Composite pattern, use `(Composite | TreeNode) Wrapper<TSrc,TWrpr>`.   
If you need a Wrapper that, in addition to its wrapper functionality, can temporarily pause/resume instance disposal and wrapping, as is the case with ViewModel in MVVM, use `(Composite | TreeNode) Imitator<TSrc,TImtr>`.   
Inherit and use each as needed.

### Compatibility with Other Libraries
In `TreeNodeBase<TNode>` and its derived types, you can customize the collections used internally and those exposed externally by overriding the `Setup(Inner | Public)ChildCollection` methods.  
`CompositeWrapper<TSrc,TWrpr>` and its derived types allow customization only of the collection exposed externally.   
Additionally, `CompositeWrapper<TSrc,TWrpr>` and `CompositeImitator<TSrc,TImtr>` provide the extension methods of `ITreeNode<TNode>` by wrapping an object that does not implement `ITreeNode<TNode>`.  
The extension methods `Convert` and `AssembleTree` can also be used with objects forming a Composite pattern that do not implement `ITreeNode<TNode>` or `IMutableTreeNode<TNode>`.

