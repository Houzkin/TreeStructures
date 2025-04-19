# Introduction
ツリー構造を効果的に扱うためのC#ライブラリです。  
このライブラリは柔軟性と拡張性を重視し、さまざまなツリー構造を容易に操作できるように設計されています。

特徴は
1. `ITreeNode<TNode>` に対する豊富な拡張メソッド
1. 親ノードと子ノードの相互参照を実現
1. ツリー構造をなすクラス群とそれらの拡張性
1. 異なるデータ構造とツリー構造の相互変換
1. 上記を実装する過程で必要となった、汎用性のあるクラス

[Nuget TreeStructures](https://www.nuget.org/packages/TreeStructures/)

# Useage
[wiki](https://github.com/Houzkin/TreeStructures/wiki/Home_ja) 参照  

# Concept
このライブラリで完結することは目指していません。  
ツリー構造の機能を提供し、他のライブラリと共存できる設計を目指しています。
  
上記の特徴について、詳細を説明します。
## 豊富な拡張メソッド
`ITreeNode<TNode>`の拡張メソッドはオーバーロードも含め60以上を定義しています。  
例を挙げると、  
列挙は`Preorder`, `Levelorder`など全ての走査法や、 `Leafs`, `Ancestors`, `DescendArrivals`, `DescendTraces`, etc  
移動は、`Root`, `NextSibling`, `LastSibling`, etc  
編集は`TryAddChild`をはじめ、`Try○○Child`, `Disassemble`, `RemoveAllDescendant`, etc  
パラメータの取得は、`TreeIndex`, `NodePath`, `Height`, `Depth`, etc  
判定メソッドは、`IsDescendantOf`, `IsAncestorOf`, `IsRoot`, etc  
変換は、`ToNodeMap`, `ToSerializableNodeMap`, `ToTreeDiagram`, `AsValuedTreeNode`  
組み立てメソッドは、`Convert`, `AssembleTree`, `AssembleAsNAryTree`, `AssembleForestByPath`  


## 親ノードと子ノードの相互参照
親ノードと子ノードの相互参照は基底クラス(`TreeNodeBase` or `HierarchyWrapper`)で処理されます。  
`TreeNodeBase`の派生型では、`RemoveChildProcess`、`InsertChildProcess`など、`protected virtual`メソッドをオーバーライドすることで動作をカスタマイズできます。

## ツリー構造をなすクラス群とその汎用性
用途に応じて以下のクラスを継承して利用できます。

- **TreeNodeBase**:   
メソッド定義など細かくカスタマイズしたい場合   

- **GeneralTreeNode / ObservableTreeNode**:  
データ構造やコンテナとして使用する場合  

- **NAryTreeNode**:   
空のノードを `null` とする N 分岐ツリーとして使用する場合  

- **HierarchyWrapper / TreeNodeWrapper**:   
階層構造をラップする。  
ラップされるノードの子ノードコレクションに変更があった場合は、`Wrapper.Children`プロパティにアクセス時に更新されます。  
子ノードコレクションの変更通知を反映させるためには`Bindable(Hierarchy|TreeNode)Wrapper`を継承してください。

- **BindableHierarchyWrapper / BindableTreeNodeWrapper**:   
ラップされるノードの子ノードコレクションの変更通知を受け取り、随時更新されます。  
Disposeメソッドも定義されており、Disposeによって対象ノードと子孫ノードが破棄されます。  
MVVM の ViewModel など、観測可能で破棄が必要な場合を想定しています。

`TreeNodeBase`とその派生型では、`Setup(Inner | Public)ChildCollection` メソッドをオーバーライドすることで、  
内部で使用するコレクションや外部に公開するコレクションをカスタマイズできます。

`HierarchyWrapper`とその派生型は外部に公開するコレクションのみカスタマイズできます。  
  
## 異なるデータ構造とツリー構造の相互変換
`ITreeNode<TNode>`を実装していないオブジェクトでも、`ITreeNode<TNode>`の拡張メソッドを利用できます。  
`HierarchyWrapper<TSrc,TWrpr>`や`BindableHierarchyWrapper<TSrc,TWrpr>`を使って階層構造をラップする、または、`AsValuedTreeNode`メソッドを呼び出すことによって、`ITreeNode<TNode>`の拡張メソッドにアクセスできます。

その他にも、拡張メソッドの`AssembleTree`や`AssembleTryByPath`、`ToNodeMap`など、相互変換方法をいくつか用意しています。  

## 実装過程で必要となった、汎用性のあるクラス 

内部実装に使用しているクラスも公開しています。  

- `ListAligner<T,TList>`   
指定したリストを操作して並び替えを行う。

- `ImitableCollection<TSrc,TConv>`  
指定したコレクションに同期したコレクション。

- `CombinableObservableCollection<T>`  
観測可能なコレクションを結合するコレクション。

- `ReadOnlyObservableItemCollection<T>`  
指定したコレクションと連動しつつ、各要素のプロパティを一括で観測する機能を追加した、観測可能なコレクション。

- `ReadOnlySortFilterObservableCollection<T>`  
指定したコレクションと連動しつつ、ソートやフィルター機能を追加した、観測可能なコレクション。

- `ListScroller<T>`  
コレクション内を移動できる機能を提供する。

- `UniqueOperationExecutor`  
操作の一意性制御（重複実行防止）

- `PropertyChangeProxy`  
`INotifyPropertyChanged`の実装を補助する。 

- `ResultWithValue<TValue>`,`ResultWith<TValue>`  
Tryメソッドの結果を管理する。

など

# 名前空間とその分類

目的・用途別に名前空間を分けています。
`TreeStructures.Tree` のみ、用途別ではなく実装を理由にまとめており、特定の規則でツリー構造をなしているクラス群です。

## TreeStructures;  
　abstractで定義された汎用ツリーノード、周辺オブジェクト、イベント引数
 
 汎用ツリーノードの継承図
 
 ![InheritanceGenericTreeNode](https://raw.githubusercontent.com/Houzkin/TreeStructures/master/images/InheritanceGenericTreeNode.png)

 NodePathとNodeIndex (周辺オブジェクト) の継承図
 
![InheritancePeripheralObjects](https://raw.githubusercontent.com/Houzkin/TreeStructures/master/images/InheritancePeripheralObjects.png)

## TreeStructures.Linq;
　`ITreeNode<TNode>`, `IMutableTreeNode<TNode>`, `IEnumerable<T>`に対する拡張メソッド。
## TreeStructures.Collections;
 内部実装や拡張メソッドの処理過程などで使用されるコレクション。  
## TreeStructures.Events;
　Event関連。リスナーや通知クラスの実装をサポートする。
## TreeStructures.Results;
　Try○○メソッドの戻り値として使用する`ResultWithValue<T>`の定義など。
## TreeStructures.Utilities;
　上記以外の汎用性のあるクラス。
## TreeStructures.Xml.Serialization;
　シリアライズ・デシリアライズ時に使用するDictionaryなど。
## TreeStructures.Internals;
　ライブラリ内部で使用されるが、汎用性が低いクラス。
## TreeStrucutures.Tree;
　目的・用途を特定した、ツリー構造をなすクラス。
