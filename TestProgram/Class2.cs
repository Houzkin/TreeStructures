using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.Linq;
using TreeStructures.EventManagement;
using TreeStructures.Utility;
using TreeStructures;
using TreeStructures.Tree;

namespace TestProgram {
    internal class Class2 {
        public class TesTreeNode : TreeNodeCollection<TesTreeNode> { }
        //public class ImitNode : CompositeImitator<TesTreeNode, ImitNode> {
        //    public ImitNode(TesTreeNode sourceNode) : base(sourceNode) {
        //    }
        //    protected override ImitNode GenerateChild(TesTreeNode srcNode) {
        //        return new ImitNode(srcNode);
        //    }
        //}
        static public void Main() {
            var tt = new TesTreeNode();



            SampleClass sample = new SampleClass();
            //PropertyChainObserver.
            var root = new ObservablePropertyTree<SampleClass>(sample);
            
            var lst1 = root.Subscribe<INotifyPropertyChanged>(x => x.PropA.PropB, (o, e) => { Console.WriteLine($"changed {e.PropertyName}"); });
            var lst2 = root.Subscribe<PropC>(x => x.PropA.PropB.PropC, e => { Console.WriteLine($"changed A-B-C {e?.ToString() ?? "null"}"); });
            var lst3 = root.Subscribe<int>(x => x.PropA.PropB.PropC.PropD.Result, e => Console.WriteLine($"{e.ToString()}"));
            var ntf = root.ToNotifyObject(x => x.PropA.PropB.PropC);
            //ntf.PropertyChanged += (o, e) => { Console.WriteLine($"changed {e.PropertyName} from notifyobject"); };
            sample.PropA.PropB = new PropB();
            sample.PropA.PropB.PropC.PropD.Result = 234;
            //sample.PropA = new PropA();
            //sample.PropA.PropB.PropC = new PropC();
            root.ChangeTarget(new SampleClass());
            Console.WriteLine(root.Root.ToTreeDiagram(x => x.NamedProperty));
            new[] { lst3 }.ToLumpDisposables().Dispose();
            Console.WriteLine(root.Root.ToTreeDiagram(x => x.NamedProperty));

            var enu1 = new List<string>() { "a", "b", "c", }.AsReadOnlyEnumerable();
            var enu2 = new List<string>() { "a", "b", "c", }.AsReadOnlyEnumerable();
            //var dic = new Dictionary<IEnumerable<string>,object>()
            Console.WriteLine(object.Equals(enu1,enu2));
            //Sample<SampleClass>(sample,x => x.PropA.PropB.PropC.PropD.Result);
            
        }
        

        static void Sample<T>(T sampleObject, Expression<Func<T, object>> expression) {
            // プロパティチェーンを辿る
            var propertyName = GetPropertyPath(expression);

            // プロパティの値を取得するためには実際のオブジェクトが必要
            // 例として T 型のインスタンスを作成する
            //T sampleObject = Activator.CreateInstance<T>();
            
            // プロパティの値を取得
            var propertyValue = expression.Compile().Invoke(sampleObject);

            // プロパティの値を利用する処理...
            Console.WriteLine($"Property Value: {propertyValue}");

            // 各プロパティの変更通知をサブスクライブ
            SubscribeToPropertyChanged(sampleObject, expression);
        }
        static Stack<string> GetPropertyPath<T>(Expression<Func<T, object>> expression) {
            var memberExpression = GetMemberExpression(expression.Body);

            if (memberExpression == null) {
                throw new ArgumentException("Expression must be a member access expression");
            }
            var propNames = new Stack<string>();
            var propertyNames = new List<string>();

            while (memberExpression != null) {
                propertyNames.Insert(0, memberExpression.Member.Name);
                propNames.Push(memberExpression.Member.Name);
                memberExpression = GetMemberExpression(memberExpression.Expression);
            }

            return propNames;
        }

        static MemberExpression GetMemberExpression(Expression expression) {
            if (expression is UnaryExpression unaryExpression) {
                return unaryExpression.Operand as MemberExpression;
            }

            return expression as MemberExpression;
        }

        static object GetValueFromPropertyName(object obj,string propertyName) {
            // obj が null の場合は例外処理が必要かもしれません
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            // obj の型から指定されたプロパティを取得
            PropertyInfo? propertyInfo = obj.GetType().GetProperty(propertyName);

            // プロパティが存在するか確認
            if (propertyInfo != null) {
                // プロパティの値を取得
                return propertyInfo.GetValue(obj);
            } else {
                // プロパティが存在しない場合は例外処理が必要かもしれません
                throw new ArgumentException($"Property '{propertyName}' not found in type {obj.GetType().Name}");
            }
        }

        static void SubscribeToPropertyChanged<T>(T sampleObject, Expression<Func<T, object>> expression) {
            if (sampleObject == null) {
                throw new ArgumentNullException(nameof(sampleObject));
            }

            var propertyChain = (expression.Body as MemberExpression)?.ToString();
            if (string.IsNullOrEmpty(propertyChain)) {
                throw new ArgumentException("Invalid property chain.", nameof(expression));
            }

            var propertyNames = propertyChain.Split('.');
            foreach (var propertyName in propertyNames) {
                var propertyInfo = typeof(T).GetProperty(propertyName);
                if (propertyInfo == null) {
                    throw new ArgumentException($"Property '{propertyName}' not found in type {typeof(T).Name}.");
                }

                var propertyValue = propertyInfo.GetValue(sampleObject);
                if (propertyValue == null) {
                    throw new InvalidOperationException($"Property '{propertyName}' is null.");
                }

                if (propertyValue is INotifyPropertyChanged notifyPropertyChanged) {
                    notifyPropertyChanged.PropertyChanged += (sender, e) =>
                    {
                        Console.WriteLine($"Property '{propertyName}' changed: {e.PropertyName}");
                    };
                }

                sampleObject = (T)propertyValue;
            }
        }
    }

    public class NotificationObject : INotifyPropertyChanged {

        /// <summary>プロパティ値が変更されたときに発生する。</summary>
        public event PropertyChangedEventHandler? PropertyChanged;
        /// <summary>プロパティ変更通知を発行する。</summary>
        protected void OnPropertyChanged([CallerMemberName] string name = null) {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        bool setProperty<TProp>(ref TProp storage, TProp value, string propertyName) {
            if (object.Equals(storage, value)) {
                return false;
            }
            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }
        /// <summary>値の設定と変更通知を行う</summary>
        protected virtual bool SetProperty<TProp>(ref TProp strage, TProp value, [CallerMemberName] string name = "") {
            return setProperty(ref strage, value, name);
        }
        public int DelegateCount => PropertyChanged.GetLength();
    }
    class SampleClass :NotificationObject {
        PropA _a = new PropA();
        public PropA PropA {
            get { return _a; }
            set { SetProperty(ref _a, value); }
        }
    }
    public class PropA : NotificationObject {
        PropB _b = new PropB();
        public PropB PropB {
            get { return _b; }
            set { SetProperty(ref _b, value); }
        }
    }

    public class PropB : NotificationObject {
        PropC _c = new PropC();
        public PropC PropC {
            get { return _c; }
            set { SetProperty(ref _c, value); }
        }
    }

    public class PropC : NotificationObject {
        PropD _d = new PropD();
        public PropD PropD {
            get { return _d; }
            set { SetProperty(ref _d, value); }
        }
    }

    public class PropD : NotificationObject {
        private int result=3;

        public int Result {
            get { return result; }
            set { SetProperty(ref result, value); }
        }

    }
}
