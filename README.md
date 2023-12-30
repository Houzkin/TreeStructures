# Overview

This library provides a class for creating a tree structure that can be used by inheriting from it.

Features:

1. Rich extension methods for ITreeNode<T>
1. Achieving mutual references between parent and child nodes
1. Classes forming a tree structure and their extensibility
1. Compatibility with other libraries
These are the four main features of the library.

## Namespaces Classification

### TreeStructures;
Abstract classes define generic tree nodes, peripheral objects, and event arguments.

Inheritance diagram of generic tree nodes

![継承図_汎用TreeNode](https://github.com/Houzkin/TreeStructure/assets/12586097/f92aca9b-a8c1-4f18-998c-4c10da68e8ea)

Inheritance diagram of NodePath and NodeIndex (peripheral objects)

![継承図_周辺オブジェクト](https://github.com/Houzkin/TreeStructure/assets/12586097/9f17c735-3e0e-40dc-b374-d4d6b380b03a)

### TreeStructures.Linq;
　Extension methods for ITreeNode<T>, IMutableTreeNode<T>, IEnumerable<T>
### TreeStructures.Utility;
　Definition of ResultWithValue<T> used as a return value for Try○○ methods
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
Let's elaborate on the four features mentioned at the beginning.

### Rich Extension Methods
Over 60 extension methods for ITreeNode<T> are defined, including various overloads. Examples include:

Enumeration: Preorder, Levelorder, all traversal methods, Leafs, Ancestors, DiscendArrivals, DescendTraces, etc.

Navigation: Root, NextSibling, LastSibling, etc.

Editing: Starting with TryAddChild, Try○○Child, Disassemble, RemoveAllDescendant, etc.

Parameter Retrieval: NodeIndex, NodePath, Height, Depth, etc.

Predicate Methods: IsDescendantOf, IsAncestorOf, IsRoot, etc.

Conversion: ToNodeMap, ToSerializableNodeMap, ToTreeDiagram

Assembly Methods: Convert, AssembleTree, AssembleAsNAryTree

### Mutual References Between Parent and Child Nodes
Mutual references between parent and child nodes are handled by base classes (TreeNodeBase or CompositeWrapper). 

In the derived types of TreeNodeBase, customization can be achieved through protected virtual methods such as RemoveChildProcess and InsertChildProcess, which are defined as ○○ChildProcess methods.

### Classes Forming a Tree Structure and Their Generality
If you want to customize in detail, use TreeNodeBase. 

For a GeneralTree, if you want to use it as a data structure or container for data, use TreeNode or ObservableTreeNode. 

If you want to use an N-Ary Tree with a fixed number of branches and empty nodes set to null, use NAryTreeNode. 

If you want to use an object or tree structure that forms the Composite pattern, use (Composite | TreeNode) Wrapper. 

If you need a Wrapper that, in addition to its wrapper functionality, can temporarily pause/resume instance disposal and wrapping, as is the case with ViewModel in MVVM, use (Composite | TreeNode) Imitator. Inherit and use each as needed.

### Compatibility with Other Libraries
In TreeNodeBase and its derived types, you can customize the collections used internally and those exposed externally by overriding the Setup(Inner | Public)ChildCollection methods.

CompositeWrapper and its derived types allow customization only of the collection exposed externally. Additionally, CompositeWrapper and CompositeImitator provide the extension methods of ITreeNode<T> by wrapping an object that does not implement ITreeNode<T>.

The Convert and AssembleTree extension methods can be used with Composite pattern objects that do not implement IMutableTreeNode.

