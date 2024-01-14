using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TreeStructures.Collections;
using TreeStructures.EventManagement;
using TreeStructures.Internals;
using TreeStructures.Linq;
using TreeStructures.Utility;

namespace TreeStructures.Tree {
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ChainedPropertyChangedEventArgs<T> : PropertyChangedEventArgs {
        /// <summary>プロパティ値</summary>
        [AllowNull]
        public T PropertyValue { get; init; }
        /// <summary>変更が行われたプロパティ名を列挙</summary>
        public IEnumerable<string> ChainedProperties { get; init; }
        /// <summary>変更が行われたプロパティ名</summary>
        public string ChainedName => string.Join('.', ChainedProperties);
        /// <summary>Constructor.</summary>
        public ChainedPropertyChangedEventArgs(string? propertyName,IEnumerable<string> propertyChain, [AllowNull] T propertyValue): base(propertyName ?? string.Empty) {
            PropertyValue=propertyValue;
            ChainedProperties = propertyChain;
        }
    }
    /// <summary>Provides method chain specification and subscription for observable properties in <typeparamref name="TSrc"/>.</summary>
    /// <typeparam name="TSrc">Type of the instance to be observed.</typeparam>
    public class ObservedPropertyTree<TSrc> : IDisposable{
        PropertyChainRoot _root;
        /// <summary>Represents the tree of properties currently registered with handlers.</summary>
        public PropertyChainNode Root => _root;
        /// <summary>Constructor.</summary>
        /// <param name="target">The instance to observe.</param>
        public ObservedPropertyTree(TSrc target) {
            _root = new PropertyChainRoot(target);
        }
        /// <summary>Gets or sets a value indicating whether change notifications should be issued when the source of observation is reset. Default value is true.</summary>
        public bool IsEvaluateTargetChanged {
            get => _root.IsEvaluateTargetChanged;
            set => _root.IsEvaluateTargetChanged = value;
        }
        /// <summary>Resets the source of observation.</summary>
        /// <param name="target">The new target instance to observe.</param>
        public void ChangeTarget(TSrc target) {
            ThrowExceptionIfDisposed();
            _root.ChangeTarget(target);
        }
        bool _isDisposed = false;
        ///<inheritdoc/>
        public void Dispose(){
            if(_isDisposed) return;
            foreach(var nd in _root?.Postorder() ?? Enumerable.Empty<PropertyChainNode>()){ nd.Dispose(); }
            _isDisposed = true;
        }
        void ThrowExceptionIfDisposed(){
            if (_isDisposed) throw new ObjectDisposedException(this.ToString(),
                "The instance has already been disposed and cannot be operated on.");
        }
        /// <summary>Creates an observable object for the specified property.</summary>
        /// <typeparam name="TValue">The type of the property.</typeparam>
        /// <param name="expression">Specifies the property described in the method chain.</param>
        /// <returns>Returns an object implementing <see cref="INotifyPropertyChanged"/> with a read-only property <see cref="NotifyObject{T}.Value"/> representing the value of the specified property.</returns>
        public NotifyObject<TValue> ToNotifyObject<TValue>(Expression<Func<TSrc, TValue>> expression) {
            ThrowExceptionIfDisposed();
            return _root.ToNotifyObject(expression);
        }
        /// <summary>Subscribes to the property change notification for the specified property.</summary>
        /// <param name="expression">Expression indicating the property.</param>
        /// <param name="changedAction">The action to be executed when the property changes.</param>
        /// <returns>An instance for unsubscribing from the subscription.</returns>
        public IDisposable Subscribe(Expression<Func<TSrc, object>> expression, Action changedAction) {
            ThrowExceptionIfDisposed();
            return Subscribe<object>(expression, (s, e) => changedAction());
        }
        /// <summary>Subscribes to the property change notification for the specified property.</summary>
        /// <typeparam name="TValue">Type of the property value.</typeparam>
        /// <param name="expression">Expression indicating the property.</param>
        /// <param name="changedAction">The action to be executed when the property changes.</param>
        /// <returns>An instance for unsubscribing from the subscription.</returns>
        public IDisposable Subscribe<TValue>(Expression<Func<TSrc, TValue>> expression, Action<TValue> changedAction) {
            ThrowExceptionIfDisposed();
            return Subscribe<TValue>(expression, (s, e) => changedAction(e.PropertyValue));
        }
        /// <summary>Subscribes to the property change notification for the specified property.</summary>
        /// <typeparam name="TValue">Type of the property value.</typeparam>
        /// <param name="expression">Expression indicating the property.</param>
        /// <param name="changedAction">The action to be executed when the property changes.</param>
        /// <returns>An instance for unsubscribing from the subscription.</returns>
        public IDisposable Subscribe<TValue>(Expression<Func<TSrc, TValue>> expression, EventHandler<ChainedPropertyChangedEventArgs<TValue>> changedAction) {
            ThrowExceptionIfDisposed();
            return _root.Subscribe(expression, changedAction);
        }
        /// <summary>
        /// Node representing a property chain.
        /// </summary>
        public class PropertyChainNode : TreeNodeBase<PropertyChainNode> {
            #region Fields
            readonly HashSet<IEnumerable<string>> ChainToLeafs = new(EqualityCompared<IEnumerable<string>>.By(a => string.Join('.', a)));
            IDisposable? targetListener;
            #endregion

            #region Constructor
            internal PropertyChainNode(object? target) {
                this.NamedProperty = string.Empty;
                SubscribePropertyValue(target);
            }
            private PropertyChainNode(object? target,string named,IEnumerable<string> propNames) {
                NamedProperty = named;
                if(propNames.Any())
                    ChainToLeafs.Add(propNames);
                SubscribePropertyValue(target);
            }
            #endregion
            /// <inheritdoc/>
            protected override IEnumerable<ObservedPropertyTree<TSrc>.PropertyChainNode> SetupInnerChildCollection()
                => new HashSet<PropertyChainNode>();
            //protected override ISet<PropertyChainNode> ChildNodes { get; } = new HashSet<PropertyChainNode>();

            /// <summary>Add child node.</summary>
            protected void AddChildProcess(PropertyChainNode child) {
                if (this.CanAddChildNode(child)) base.InsertChildProcess(0, child, (c, i, n) => ((ICollection<PropertyChainNode>)c).Add(n));
            }
            /// <summary><inheritdoc/></summary>
            protected override void RemoveChildProcess(PropertyChainNode child,Action<IEnumerable<PropertyChainNode>,PropertyChainNode>? action = null) {
                if (!ChildNodes.Contains(child)) return;
                base.RemoveChildProcess(child);
                foreach (var item in ChainToLeafs.Where(x => x.First() == child.NamedProperty).ToArray()){
                    var lst = new Stack<string>(item.Reverse());
                    foreach(var upstm in this.Upstream()) {
                        upstm.ChainToLeafs.Remove(lst);
                        lst.Push(upstm.NamedProperty);
                    }
                }
            }
            /// <summary>Monitored target or property value.</summary>
            [AllowNull]
            public object Target { get; private set; }
            /// <summary>Name of the property indicating the target of observation.</summary>
            public string NamedProperty { get; init; }
            bool TrySetTarget(object? target) {
                if (ReferenceEquals(target,Target)) return false;
                Target = target;
                return true;
            }
            /// <summary>Registers properties to observe.</summary>
            /// <param name="propNames">The names of the properties to observe.</param>
            protected void AddSubscribeProperty(IEnumerable<string> propNames) {
                if (!propNames.Any()) return;
                if (!ChainToLeafs.Add(propNames)) return;
                var obsname = propNames.First();
                var cld = ChildNodes.FirstOrDefault(a => a.NamedProperty == obsname);
                if(cld != null) {
                    cld.AddSubscribeProperty(propNames.Skip(1));
                } else {
                    var propValue = PropertyUtils.GetValueFromPropertyName(Target, obsname);
                    this.AddChildProcess(new PropertyChainNode(propValue, obsname, propNames.Skip(1)));
                }
            }
            /// <summary>Subscribes to property value change notifications.</summary>
            /// <param name="target"></param>
            /// <exception cref="InvalidCastException"></exception>
            protected void SubscribePropertyValue(object? target) {
                if (!TrySetTarget(target)) return;//ターゲットが同一であれば何もしない
                targetListener?.Dispose();
                if(target is INotifyPropertyChanged self) {//ターゲットの観測を開始
                    targetListener = new EventListener<PropertyChangedEventHandler>(
                        h => self.PropertyChanged += h,
                        h => self.PropertyChanged -= h,
                        onPropChanged);
                } else { 
                    if(target!=null && ChainToLeafs.Any()) 
                        throw new InvalidCastException(
                            $"For the observed target: {this.Root().Target?.ToString()}, the value of the PropertyChain: {string.Join('.', this.Upstream().Reverse().Skip(1))} should implement {nameof(INotifyPropertyChanged)}."); 
                }
                foreach(var chain in ChainToLeafs) {
                    if (!chain.Any()) return;
                    var obsname = chain.First();
                    var propValue = PropertyUtils.GetValueFromPropertyName(Target, obsname);
                    var cld = ChildNodes.FirstOrDefault(a => a.NamedProperty == obsname);
                    if (cld != null) {
                        cld.SubscribePropertyValue(propValue);
                    } else {
                        this.AddChildProcess(new PropertyChainNode(propValue, obsname, chain.Skip(1)));
                    }
                }
            }
            void onPropChanged(object? sender, PropertyChangedEventArgs e) {
                if (string.IsNullOrEmpty(e.PropertyName)) return;
                //観測プロパティの変更だった場合、該当する子ノードの値を再設定
                var chgcld = this.ChildNodes.FirstOrDefault(a => a.NamedProperty == e.PropertyName);
                if (chgcld == null) return;
                chgcld.SubscribePropertyValue(PropertyUtils.GetValueFromPropertyName(Target, e.PropertyName));
                RaisePropertyChanged( e, chgcld.Leafs());
            }
            /// <summary>イベントの発行</summary>
            protected virtual void RaisePropertyChanged(PropertyChangedEventArgs e, IEnumerable<PropertyChainNode> leafs) {
                var root = this.Root();
                if(object.ReferenceEquals(root, this)) return;
                root.RaisePropertyChanged(e,leafs);
            }
            
            internal void Dispose() {
                targetListener?.Dispose();
                this.Parent?.RemoveChildProcess(Self);
            }
        }
        private class PropertyChainRoot : PropertyChainNode {
            HashSet<ChainStatus> chainStatuses = new(EqualityCompared<ChainStatus>.By(a => string.Join('.', a.Key.Prepend(a.PropertyValueType))));
            public PropertyChainRoot(TSrc target) : base(target) { }
            public bool IsEvaluateTargetChanged { get; set; } = true;
            public void ChangeTarget(TSrc target) {
                var pre = this.Target;
                this.SubscribePropertyValue(target);
                if (IsEvaluateTargetChanged && !ReferenceEquals(pre, this.Target)) {
                    this.RaisePropertyChanged(null,this.Leafs());
                }
            }
            
            public IDisposable Subscribe<TValue>(Expression<Func<TSrc, TValue>> expression, EventHandler<ChainedPropertyChangedEventArgs<TValue>> changedAction) {
                var propChain = PropertyUtils.GetPropertyPath(expression).Prepend(string.Empty);
                var status = addChainStatus<TValue, object>(propChain);
                return getListener(status, changedAction);
            }
            public NotifyObject<TValue> ToNotifyObject<TValue>(Expression<Func<TSrc,TValue>> expression) {
                var propChain = PropertyUtils.GetPropertyPath(expression).Prepend(string.Empty);
                return new NotifyObject<TValue>(propChain, addChainStatus<TValue,TValue>, getListener);
            }
            ChainStatus<TValue> addChainStatus<TValue, TProp>(IEnumerable<string> propChain) {
                //this.AddSubscribeProperty(propChain.Except(new string[] { string.Empty }));
                //propChain = propChain.Where(x => x != string.Empty);
                this.AddSubscribeProperty(propChain.Where(x => x != string.Empty));
                //if(propChain.Any() && propChain.ElementAt(0) == string.Empty){
                //    propChain = propChain.Prepend(string.Empty).ToList();
                //}
                Func<TValue> getvalue = () => {
                    try {
                        if (this.Target != null) {
                            var val = this.DescendArrivals(a => a.NamedProperty, propChain).Single().Target;
                            if (val is TValue va) return va;
                        }
                        return default;
                    } catch (InvalidOperationException e) {
                        return default;
                        //throw new InvalidOperationException("There is a duplicate or unregistered property value specified in the PropertyChain.", e);
                    }
                };
                var propValue = getvalue();

                var status = chainStatuses.OfType<ChainStatus<TValue>>().FirstOrDefault(x => x.Key.SequenceEqual(propChain) && x.PropertyValueType == typeof(TValue).FullName);
                if (status == null) {
                    var chain = propChain.Any() ? propChain : new string[] { string.Empty };
                    status = new ChainStatus<TValue>(chain, getvalue);
                    chainStatuses.Add(status);
                }
                return status;
            }
            IDisposable getListener<T>(ChainStatus<T> status,EventHandler<ChainedPropertyChangedEventArgs<T>> changedAction) {
                var listener = new EventListener<EventHandler<ChainedPropertyChangedEventArgs<T>>>(
                    h => status.ChainedPropertyChanged += h,
                    h => status.ChainedPropertyChanged -= h,
                    changedAction);

                var dsp = new DisposableObject(() => {
                    listener.Dispose();
                    if (status.ChainedPropertyChanged.GetLength() == 0) {
                        chainStatuses.Remove(status);
                        TryRemoveNode(status.Key.ToList());
                    }
                });
                return dsp;
            }
            protected override void RaisePropertyChanged(PropertyChangedEventArgs e, IEnumerable<PropertyChainNode> leafs) {
                HashSet<ChainStatus> args = new();
                foreach (var leaf in leafs) {
                    var lfchain = leaf.Upstream().Reverse()/*.Skip(1)*/.Select(a => a.NamedProperty).ToArray();
                    lfchain = lfchain.Any() ? lfchain : new string[] { string.Empty };
                    foreach (var sts in chainStatuses) {
                        if (args.Contains(sts)) continue;
                        var mth = sts.Key.Zip(lfchain).TakeWhile((x, y) => x.First.Equals(x.Second));
                        if (mth.Any()) args.Add(sts);
                    }
                    if (args.Count == chainStatuses.Count) break;
                }
                foreach (var sts in args) {
                    sts.OnPropertyChanged(Target,e);
                }
            }
            void TryRemoveNode(IEnumerable<string> seq) {
                //Console.WriteLine("debug:\n"+this.ToTreeDiagram(x => x.NamedProperty));
                var lst = seq.SkipLast(1).ToList();
                //var dess = this.DescendTraces(a => a.NamedProperty, lst.SkipLast(1));
                var des = this.DescendTraces(a => a.NamedProperty, lst).FirstOrDefault()?.Skip(1)
                    ?? Enumerable.Empty<PropertyChainNode>();
                foreach (var sts in chainStatuses) {
                    var llst = sts.Key.ToList();
                    var ext = this.DescendTraces(a => a.NamedProperty, sts.Key).FirstOrDefault()?.Skip(1)
                        ?? Enumerable.Empty<PropertyChainNode>();
                    des = des.Except(ext);
                }
                foreach (var rmv in des.ToArray()) rmv.Dispose();
            }
            
        }
        /// <summary>Represents an object capable of subscribing to change notifications for a specified property.</summary>
        /// <remarks>When handlers are registered, they are referenced by the target object.<br/>If all handlers are removed or not registered,there is no need to dispose of this object.</remarks>
        /// <typeparam name="T">The type of the property value.</typeparam>
        public class NotifyObject<T> : INotifyPropertyChanged, IDisposable{
            IEnumerable<string> propChain;
            Func<IEnumerable<string>, ChainStatus<T>> setFunc;
            Func<ChainStatus<T>, EventHandler<ChainedPropertyChangedEventArgs<T>>,IDisposable> getListener;
            List<Tuple<PropertyChangedEventHandler, IDisposable>> HandlerDispoPair = new();
            void Set(PropertyChangedEventHandler handler, IDisposable disp) { HandlerDispoPair.Add(Tuple.Create(handler, disp)); }
            void Remove(PropertyChangedEventHandler handler) {
                var tpl = HandlerDispoPair.FirstOrDefault(a=>a.Item1 == handler);
                if (tpl != null) {
                    tpl.Item2.Dispose();
                    HandlerDispoPair.Remove(tpl);
                }
            }
            internal NotifyObject(IEnumerable<string> propchain,Func<IEnumerable<string>,ChainStatus<T>> func ,Func<ChainStatus<T>,EventHandler<ChainedPropertyChangedEventArgs<T>>,IDisposable> toListener){
                propChain = propchain.ToList();
                setFunc = func;
                getListener = toListener;
                //初期値を設定するためにChainStatusを設定しDispose
                var sts = setFunc(propChain);
                this.Value = sts.BeforeValue;
                getListener(sts, (o, e) => { }).Dispose();
            }
            event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged {
                add { this.PropertyChanged += value; }
                remove { this.PropertyChanged -= value; }
            }
            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            public event PropertyChangedEventHandler PropertyChanged {
                add {
                    Set(value, getListener(setFunc(propChain), (o, e) => {
                        //各イベント発行前に値を比較する
                        if(!object.Equals(this.Value,e.PropertyValue)) this.Value = e.PropertyValue;
                        //比較結果に関わらず実行。変更が発生したのは確実であり、比較は各リスナーに付随して行っている
                        //value.Invoke(this, e);
                        value.Invoke(this, new PropertyChangedEventArgs("Value"));
                    }));
                }
                remove { Remove(value); }
            }
            /// <summary>
            /// The value of the specified property at the time of creation. The value is updated only when event handlers are registered.
            /// </summary>
            public T Value { get; private set; }
            /// <summary>Removes all registered handlers.</summary>
            public void Dispose() {
                var hdlrs = HandlerDispoPair.Select(x=>x.Item1).ToArray();
                foreach (var hdlr in hdlrs) Remove(hdlr);

            }
        }

        internal abstract class ChainStatus {
            public abstract IEnumerable<string> Key { get; }
            public abstract string PropertyValueType { get; }
            public abstract void OnPropertyChanged(object? sender, PropertyChangedEventArgs e);
        }
        internal class ChainStatus<TVal> : ChainStatus {
            public ChainStatus(IEnumerable<string> key,Func<TVal> getPresentValue) {
                this.Key = key;
                this.GetPresentValue = getPresentValue;
                this.BeforeValue = getPresentValue();
            }
            public override IEnumerable<string> Key { get; }
            public override string PropertyValueType => typeof(TVal).FullName ?? "";
            public TVal BeforeValue { get; private set; }
            public EventHandler<ChainedPropertyChangedEventArgs<TVal>>? ChainedPropertyChanged;
            public Func<TVal> GetPresentValue { get; private set; }

            public override void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
                var curv = GetPresentValue();
                if(object.Equals(this.BeforeValue, curv)) return;
                //var arg = new ChainedPropertyChangedEventArgs<TVal>(string.Join('.',status.Key), curv);
                var arg = new ChainedPropertyChangedEventArgs<TVal>(e?.PropertyName ?? string.Empty, this.Key, curv);
                ChainedPropertyChanged?.Invoke(sender, arg);
                BeforeValue = curv;
            }
        
        }

        
    }
    
    //public class ObservablePropertyTree {
    //    public static ObservablePropertyTree<TSrc> Establish<TSrc>(TSrc target) {
    //        return new ObservablePropertyTree<TSrc>(target);
    //    }

    //}
}
