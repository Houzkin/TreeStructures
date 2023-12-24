using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.Internals;
using TreeStructures.Linq;
using TreeStructures.Utility;

namespace TreeStructures.EventManagement {
    /// <summary><see cref="IObservableTreeNode{TNode}"/>の変更イベントを制御する</summary>
    /// <typeparam name="TNode"></typeparam>
    public sealed class StructureChangedEventExecutor<TNode> : UniqueOperationExecutor where TNode: class, IObservableTreeNode<TNode> {
        /// <summary>新規インスタンスを初期化する</summary>
        /// <param name="self"></param>
        public StructureChangedEventExecutor(TNode self):base() {
            Self = self;
            preOldAnc = Array.Empty<TNode>();
            initialize();
            this.Register(structureeventkey, () => { });
        }
        readonly string structureeventkey = "in Library : " + nameof(StructureChangedEventExecutor<TNode>)+".StructureChanged - Manager";
        readonly TNode Self;
        TNode[] preOldAnc;
        int oldIndex;
        //int entryCount = 0;
        TNode? newParent;
        TNode? oldParent;
        TNode? NewParent {
            get { return newParent; }
            set { if (newParent == null) newParent = value; }
        }
        TNode? OldParent {
            get { return oldParent; }
            set {
                if(oldParent == null &&  value != null) {
                    oldParent = value;
                    oldIndex = Self.BranchIndex();
                    preOldAnc = oldParent.Upstream().ToArray();
                }
            }
        }
        TNode PreOldRoot => OldParent == null ? Self : preOldAnc.Last();
        private void initialize() {
            newParent = null; oldParent = null; oldIndex = -1; preOldAnc = Array.Empty<TNode>();
        }
        IDictionary<ChangedDescendantInfo<TNode>, IEnumerable<TNode>> desChangedRange() {
            //祖先宛の子孫変更通知
            //同一ツリー内での移動
            var dic = new Dictionary<ChangedDescendantInfo<TNode>, IEnumerable<TNode>>();
            var dArg = new ChangedDescendantInfo<TNode>(TreeNodeChangedAction.Deviate, Self, OldParent, oldIndex);
            var jArg = new ChangedDescendantInfo<TNode>(TreeNodeChangedAction.Join, Self, OldParent, oldIndex);
            if (object.ReferenceEquals(PreOldRoot, Self.Upstream().Last())) {
                var mArg = new ChangedDescendantInfo<TNode>(TreeNodeChangedAction.Move, Self, OldParent, oldIndex);
                var deviate = preOldAnc.Except(Self.Upstream().Skip(1));
                var join = Self.Upstream().Skip(1).Except(preOldAnc);
                var move = preOldAnc.Intersect(Self.Upstream().Skip(1));
                if (deviate.Any())
                    dic.Add(dArg, deviate);
                if (join.Any())
                    dic.Add(jArg, join);
                if (move.Any())
                    dic.Add(mArg, move);
            } else {
                if (OldParent != null)
                    dic.Add(dArg, OldParent.Upstream());
                if (NewParent != null)
                    dic.Add(jArg, NewParent.Upstream());
            }
            return dic;
        }

        Tuple<ChangedAncestorInfo<TNode>, IEnumerable<TNode>> ancChangedRange() {
            bool rootChanged = !object.ReferenceEquals(PreOldRoot, Self.Upstream().Last());
            var arg = new ChangedAncestorInfo<TNode>(Self, OldParent, oldIndex, rootChanged);
            return Tuple.Create(arg, Self.Levelorder());
        }

        IDictionary<StructureChangedEventArgs<TNode>, IEnumerable<TNode>> strChangeRange() {
            var nr = Self.Upstream().Last();
            Dictionary<StructureChangedEventArgs<TNode>, IEnumerable<TNode>> dic = new Dictionary<StructureChangedEventArgs<TNode>, IEnumerable<TNode>>();
            var dArg = new StructureChangedEventArgs<TNode>(TreeNodeChangedAction.Deviate, Self, OldParent, oldIndex);
            var jArg = new StructureChangedEventArgs<TNode>(TreeNodeChangedAction.Join, Self, OldParent, oldIndex);
            if (object.ReferenceEquals(PreOldRoot, nr)) {
                var mArg = new StructureChangedEventArgs<TNode>(TreeNodeChangedAction.Move, Self, OldParent, oldIndex);
                if (NewParent != null && OldParent != null && OldParent.Root() != PreOldRoot) {
                    dic.Add(dArg, OldParent.Root().Levelorder());//remove
                }
                dic.Add(mArg, nr.Levelorder());//move
            } else {
                //add or remove
                if (OldParent != null)
                    dic.Add(dArg, OldParent.Root().Levelorder());
                if (NewParent != null) {
                    dic.Add(jArg, NewParent.Root().Levelorder());
                } else {
                    IEnumerable<TNode>? v;
                    if (dic.TryGetValue(dArg, out v)) {
                        dic[dArg] = v.Union(Self.Levelorder());
                    } else {
                        dic.Add(dArg, Self.Levelorder());
                    }
                }
            }
            return dic;
        }

        void raiseProcess() {
            //子孫変更の通知を発行するノード
            var des = from x in desChangedRange()
                      from y in x.Value
                      select new { DesArg = x.Key, Node = y };
            //祖先変更の通知を発行するノード
            var anc = from x in ancChangedRange().Item2
                      select new { AncArg = ancChangedRange().Item1, Node = x };
            //ツリー構造変更の通知を発行するノード
            var str = from x in strChangeRange()
                      from y in x.Value
                      select new { StrArg = x.Key, Node = y };
            //イベントの発行シーケンス
            var ele = from n in str
                      join d in des on n.Node equals d.Node into dn
                      from da in dn.DefaultIfEmpty(new { DesArg = null as ChangedDescendantInfo<TNode>, Node = n.Node })
                      join a in anc on n.Node equals a.Node into an
                      from aa in an.DefaultIfEmpty(new { AncArg = null as ChangedAncestorInfo<TNode>, Node = n.Node })
                      select new { Node = n.Node, StrArg = n.StrArg, DesArgs = da.DesArg, AncArg = aa.AncArg };
            //発火
            foreach (var nd in ele) {
                nd.StrArg.AncestorInfo = nd.AncArg as ChangedAncestorInfo<TNode>;
                nd.StrArg.DescendantInfo = nd.DesArgs as ChangedDescendantInfo<TNode>;
                nd.Node.OnStructureChanged(nd.StrArg);
            }
        }
        
        bool IsChanged => Self.Parent != OldParent ? true : //Parentが異なれば、変化あり
            Self.Parent != null && Self.BranchIndex() != oldIndex ? true ://Parentに変化がなかった場合
            false;

        /// <summary>初回メソッド呼び出し時と戻り値の最後のDispose時でツリー構造に変更があった場合、変更イベントを発行する</summary>
        /// <returns></returns>
        public IDisposable LateEvaluateTree() {
            var ele = Result<CountOperationPair>.Of(Operations.TryGetValue, structureeventkey).When(
                o => {
                    o.Count++;
                    if (o.Count == 1) { OldParent = Self.Parent; }
                    return o;
                }, x => throw new KeyNotFoundException());
            return new DisposableObject(() => {
                this.NewParent = Self.Parent;
                if (ele.Count == 1 && IsChanged) {
                    raiseProcess(); initialize();
                }
                ele.Count--;
            });
        }
    }
    
}