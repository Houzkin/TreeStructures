using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TreeStructure;
using TreeStructure.EventManagement;

namespace TestProgram {
    public static class TestNullElement {
        static void SampleMethod<T>(Expression<Func<TestCommonNode,T>> func) {
            var t = typeof(T);
            Console.WriteLine(t);
            var memexp = (func.Body as MemberExpression).ToString();
            var name = (func.Body as MemberExpression)?.Member.Name;
            var y = func.Compile();
            var value = y.Invoke(new TestCommonNode("aaa"));
            Console.WriteLine(name);
            Console.WriteLine(value);
            var pt = func.Parameters[0].Type; 
            //文字列(TestCommonNode)とプロパティ名をキーにコンパイル結果をスタティックに保存予定
            Console.WriteLine(pt);
        }
        
        public static void Test() {

            var a = new TestCommonNode("A");
            var b = new TestCommonNode("B");
            var c = new TestCommonNode("C");
            var d = new TestCommonNode("D");
            var e = new TestCommonNode("E");
            var f = new TestCommonNode("F");
            var g = new TestCommonNode("G");
            var h = new TestCommonNode("H");
            var i = new TestCommonNode("I");
            var j = new TestCommonNode("J");
            var k = new TestCommonNode("K");
            
            a.Fork(x0 => x0.AddChild(b)
                .Fork(x1 => x1.AddChild(c)
                    .Fork(x2 => x2.AddChild(d))
                    .Fork(x2 => x2.AddChild(e)))
                .Fork(x1 => x1.AddChild(f)
                    .Fork(x2 => x2.AddChild(g))
                    .Fork(x2 => x2.AddChild(h)))
                .Fork(x1 => x1.AddChild(i)))
            .Fork(x0 => x0.AddChild(j)
                .Fork(x1 => x1.AddChild(k)));
            //var propa = new PropA();
            //var propb = new PropB();
            //var listener = new PropertyTreeChangedEventListener<PropA>(propa).RegisterHandler(x => x.PropB.PropC.PropD.Result, (s, e) => { Console.WriteLine(e.PropertyName); });
            //propa.PropB.PropC.PropD.Result = 100;
            //propa.PropB = propb;
            //a.AddChild(b).AddChild(c).AddChild(d)
            //    .Fork(c.AddChild(e).AddChild(f))
            //    .Fork(a.AddChild(g).AddChild(h))
            //    .Fork(b.AddChild(i).AddChild(j))
            //    .Fork(g.AddChild(k));

            //var dic = new Dictionary<int[], TestCommonNode>() {
            //    [new int[] { }] = a,
            //    [new int[] { 0 }] = b,
            //    [new int[] { 0, 0 }] = c,
            //    //{new int[]{ },a },
            //    //{new int[]{ 0 },b },
            //    //{new int[]{1},c },
            //};
            ////dic[new int[] { 1 }] = d;
            //var aa = dic.AssembleTree();

            //a.AddChild(b).AddChild(c).AddChild(d);
            //b.AddChild(e).AddChild(f).AddChild(g);
            //c.AddChild(h);
            //d.AddChild(i);
            //i.AddChild(j).AddChild(k);

            Console.Write(a.ToTreeDiagram(x=>x.Name));
            EventHandler<StructureChangedEventArgs<TestCommonNode>> handler = (s, e) => {
                Console.WriteLine($"sender:{s} \nTarget:{e.Target} TreeAction:{e.TreeAction} PreviousParentOfTarget:{e.PreviousParentOfTarget} OldIndex:{e.OldIndex} AncestorWasChanged:{e.AncestorWasChanged} DescendantWasChanged:{e.DescendantWasChanged}");
                if (e.AncestorWasChanged) {
                    var info = e.AncestorInfo!;
                    Console.WriteLine($"MovedTarget:{info.MovedTarget} OldIndex:{info.OldIndex} PreviousParentOfTarget:{info.PreviousParentOfTarget} RootWasChanged:{info.RootWasChanged}");
                }else if (e.DescendantWasChanged) {
                    var info = e.DescendantInfo!;
                    Console.WriteLine($"Target:{info.Target} NodeAction:{info.NodeAction} OldIndex:{info.OldIndex} PreviousParentOfTarget:{info.PreviousParentOfTarget}");
                }
                Console.Write("\n");
            };
            foreach (var node in a.Levelorder()) node.StructureChanged += handler;
            //f.PropertyChanged += (s, e) => { Console.WriteLine($"F pearent changed. new parent is {((TestCommonNode)s).Parent.Name}"); };
            //e.Test();
            var lstnr = new EventListener<EventHandler<StructureChangedEventArgs<TestCommonNode>>, PropertyChangedEventArgs>(
                conversion: h => (s, e) => h(s, new PropertyChangedEventArgs("")),
                add: h => a.StructureChanged += h,
                remove: h => a.StructureChanged -= h,
                handler: (s, e) => { });
            Console.WriteLine(string.Join(",",a.DescendArrivals(a => a.Name, new string[] { "A","B","I" })));
            var tac = a.ClearChildren();//a.DescendTraces(a => a.Name,new string[] {"A","B","I",});
            foreach (var t in tac) { 
                Console.WriteLine(string.Join(',', t)); }
            j.InsertChild(0,f);
            Console.WriteLine(a.ToTreeDiagram(a => a.Name));
            e.AddChild(j);
            Console.WriteLine(a.ToTreeDiagram(a => a.Name));
            Console.WriteLine(string.Join(",", a.Levelorder()));
            Console.WriteLine(string.Join(",", a.Preorder()));
            Console.WriteLine(string.Join(",", a.Postorder()));
            Console.WriteLine(string.Join(",", a.Inorder()));
            Console.WriteLine("test");
            Console.WriteLine(string.Join(",", a.Leafs()));
            Console.WriteLine(string.Join(",", h.Generations()));

            //var d = new TestCommonNode("D");
            //var e = new TestCommonNode("E");
            //var f = new TestCommonNode("F");
            //var alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray().Select(a => a.ToString());
            //var alphatree = Enumerable.Range(1, 50).CreateNAryTree(2, a => new TestCommonNode(a), (a, b) => a.AddChild(b))!;
            //Console.WriteLine(alphatree.ToTreeDiagram(a => a.Name));
        }
    }
    public class ObsaTesNode : ObservableTreeNodeCollection<ObsaTesNode> { 
        
        public void Hoge() {
        }
    }

    
    public class  TestCommonNode : ObservableTreeNodeCollection<TestCommonNode> {
        public TestCommonNode(string name) {
            Name = name;
        }
        public TestCommonNode(int index) {
            Name = getColumnNameFromIndex(index);
        }
        public void TokenTest() { }
        public void TokenTest2(int i) { }
        public string Name { get; set; }
        public override string ToString() {
            return Name;
        }
        protected override bool CanAddChildNode(TestCommonNode child) {
            //if (child.Name == "D") return false;
            return base.CanAddChildNode(child);
        }
        public static String getColumnNameFromIndex(int column) {
            column--;
            String col = Convert.ToString((char)('A' + (column % 26)));
            while (column >= 26) {
                column = (column / 26) - 1;
                col = Convert.ToString((char)('A' + (column % 26))) + col;
            }
            return col;
        }
    }
}
