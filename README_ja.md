# Introduction
このライブラリは、継承して使用するツリー構造をなすクラスを提供しています。

特徴は
1. `ITreeNode<T>` に対する豊富な拡張メソッド
1. 親ノードと子ノードの相互参照を実現
1. ツリー構造をなすクラス群とそれらの拡張性
1. 他ライブラリとの親和性

以上4つが挙げられます。

## 名前空間とその分類

### TreeStructures;  
　abstractで定義された汎用ツリーノード、周辺オブジェクト、イベント引数
 
 汎用ツリーノードの継承図
 
![継承図_汎用TreeNode](https://github.com/Houzkin/TreeStructure/assets/12586097/f92aca9b-a8c1-4f18-998c-4c10da68e8ea)

 NodePathとNodeIndex (周辺オブジェクト) の継承図
 
![継承図_周辺オブジェクト](https://github.com/Houzkin/TreeStructure/assets/12586097/9f17c735-3e0e-40dc-b374-d4d6b380b03a)

### TreeStructures.Linq;
　`ITreeNode<T>`, `IMutableTreeNode<T>`, `IEnumerable<T>`に対する拡張メソッド
### TreeStructures.Utility;
　Try○○メソッドの戻り値として使用する`ResultWithValue<T>`の定義など
### TreeStructures.Collections;
　内部実装や拡張メソッドの処理過程などで使用されるコレクション　
### TreeStructures.EventManagement;
　Event関連、ObservableなTreeNodeを実装するときに使用するオブジェクトなど
### TreeStructures.Xml.Serialization;
　シリアライズ・デシリアライズ時に使用するDictionaryなど
### TreeStrucutures.Tree;
　目的・用途を特定したツリー


## Useage
wikiに書く

## Concept
このライブラリで完結することは目指していません。  
既に様々な便利なライブラリが公開されているので、ツリー構造に関する機能を担いつつ、他ライブラリとの共存を目指しています。  
  
冒頭にて示した4つの特徴について補足します。
### 豊富な拡張メソッド
`ITreeNode<T>`の拡張メソッドはオーバーロードも含め60以上を定義しています。
例を挙げると、
列挙は`Preorder`, `Levelorder`など全ての走査法や、 `Leafs`, `Ancestors`, `DiscendArrivals`, `DescendTraces`, etc
移動は、`Root`, `NextSibling`, `LastSibling`, etc
編集は`TryAddChild`をはじめ、`Try○○Child`, `Disassemble`, `RemoveAllDescendant`, etc
パラメータの取得は、`NodeIndex`, `NodePath`, `Height`, `Depth`, etc
判定メソッドは、`IsDescendantOf`, `IsAncestorOf`, `IsRoot`, etc
変換は、`ToNodeMap`, `ToSerializableNodeMap`, `ToTreeDiagram`
組み立てメソッドは、`Convert`, `AssembleTree`, `AssembleAsNAryTree`


### 親ノードと子ノードの相互参照
親ノードと子ノードの相互参照は基底クラス(TreeNodeBase or CompositeWrapper)で処理されます。
TreeNodeBaseの派生型では、RemoveChildProcess、InsertChildProcessなど、protected virtualで定義された○○ChildProcessメソッドでカスタマイズできます。

### ツリー構造をなすクラス群とその汎用性
細かくカスタマイズするならTreeNodeBase。
GeneralTreeで、データ構造またはデータのコンテナとして使用するならTreeNodeまたはObservableTreeNode、
Branchの数を固定して、空のノードをnullとするN-Ary Treeとして使用するのであればNAryTreeNode、
Compositeパターンをなすオブジェクトまたはツリー構造の、ラッパークラスとして使用するならば(Composite | TreeNode) Wrapper、
ラッパーとしての機能に加え、MVVMにおけるViewModelなど、インスタンスの破棄とラップを一時的に停止・再開が可能なオブジェクトとして使用する場合は(Composite | TreeNode) Imitator
をそれぞれ継承して使用してください。

### 他ライブラリとの親和性
TreeNodeBaseとその派生型ではSetup(Inner | Public)ChildCollectionメソッドをオーバーライドすることで、内部で扱うコレクションと外部に公開するコレクションをカスタマイズできます。

CompositeWrapperとその派生型は外部に公開するコレクションのみカスタマイズできます。また、CompositeWrapperとCompositeImitatorは`ITreeNode<T>`を実装していないオブジェクトをラップすることで`ITreeNode<T>`の拡張メソッドを提供します。

拡張メソッドの`Convert`と`AssembleTree`は、`IMutableTreeNode`を実装していないCompositeパターンをなすオブジェクトに対しても使用できます。
