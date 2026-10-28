using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.Collections;
using TreeStructures.Linq;

namespace SampleConsoleApp;

public static partial class Demo {
	public static void Proxy_1() {
		var srcList = new ObservableCollection<char>("ABCDEFG".ToCharArray());
		var pxyList = new ReadOnlyObservableProxyCollection<char, string>(
			srcList,
			x => x.ToString().ToLower(),
			(chr, str) => chr.ToString().ToLower() == str);

		Console.WriteLine($"source:{string.Join(",", srcList)}");
		Console.WriteLine($"proxy :{string.Join(",", pxyList)}");

		srcList.Add('X');
		srcList.Move(1, 0);
		srcList.Remove('F');

		Console.WriteLine($"source:{string.Join(",", srcList)}");
		Console.WriteLine($"proxy :{string.Join(",", pxyList)}");

		pxyList.Dispose();
	}
	public static void Proxy_2() {
		var srcList = new ObservableCollection<char>("ABCDE".ToCharArray());
		var proxy1 = srcList.ToReadOnlyObservable(
			chr => chr.ToString(),
			(chr, str) => chr.ToString() == str,
			str => Console.WriteLine($"{str} removed."));


		srcList.Remove('A');

		proxy1.Dispose();

		var proxy2 = srcList.ToReadOnlyObservable(
			chr => new DisposableNamedObj() { Name = chr.ToString() },
			(chr, obj) => chr.ToString() == obj.Name,
			obj => obj.Dispose());

		srcList.Remove('B');

		proxy2.CombineDisposables().Dispose();
		proxy2.Dispose();

		Console.ReadKey();
	}

	public static void SortFilter_1() {

		var rdm = new Random();
		var srcList = new ObservableCollection<int>(Enumerable.Range(0, 5).Select(x => rdm.Next(100)));
		var pxyList = srcList.ToReadOnlyObservableFilterSort();

		Console.WriteLine($"source:{string.Join(",", srcList)}");
		Console.WriteLine($"proxy :{string.Join(",", pxyList)}");
		//source:92,32,90,13,83
		//proxy :92,32,90,13,83

		pxyList.SortBy(x => x);
		Console.WriteLine($"proxy :{string.Join(",", pxyList)}");
		//proxy :13,32,83,90,92

		pxyList.FilterBy(x => 30 < x);
		Console.WriteLine($"proxy :{string.Join(",", pxyList)}");
		//proxy :32,83,90,92

		srcList.Add(rdm.Next(100));
		Console.WriteLine($"proxy :{string.Join(",", pxyList)}");
		//proxy :31,32,83,90,92


		pxyList.ClearSorts();
		Console.WriteLine($"proxy :{string.Join(",", pxyList)}");
		//proxy :92,32,90,83,31

		pxyList.ClearFilter();
		Console.WriteLine($"proxy :{string.Join(",", pxyList)}");
		//proxy :92,32,90,13,83,31
	}
	public static void SortFilter_2() {
		var srcList = new ObservableCollection<ItemData>() {
			new ItemData(){ Name = "Alice", Number = 50, IsActive = true },
			new ItemData(){ Name = "Bob", Number = 30, IsActive = true },
			new ItemData(){ Name = "Charlie", Number = 25, IsActive = false },
			new ItemData(){Name = "Diana", Number = 42, IsActive = true},
			new ItemData(){Name = "Eve", Number = 28, IsActive = false},
			new ItemData(){Name = "Frank", Number = 35, IsActive = true},
			new ItemData(){Name = "Grace", Number = 22, IsActive = false},
			new ItemData(){Name = "Hank", Number = 60, IsActive = true},
			new ItemData(){Name = "Ivy", Number = 33, IsActive = false}
		};

		var pxyList = srcList.ToReadOnlyObservableFilterSort();
		pxyList.FilterBy(x => x.IsActive && 30 < x.Number, x => x.IsActive, x => x.Number);
		foreach (var p in pxyList) {
			Console.WriteLine(p);
		}
		/*
		Name:Alice, Number:50, IsActive:True
		Name:Diana, Number:42, IsActive:True
		Name:Frank, Number:35, IsActive:True
		Name:Hank, Number:60, IsActive:True
		*/

		srcList.First(x => x.Name == "Alice").IsActive = false;
		foreach (var p in pxyList) {
			Console.WriteLine(p);
		}
		/*
		Name:Diana, Number:42, IsActive:True
		Name:Frank, Number:35, IsActive:True
		Name:Hank, Number:60, IsActive:True
		*/

		pxyList.FilterProperty(x => x.IsActive);
		foreach (var p in pxyList) {
			Console.WriteLine(p);
		}
		/*
		Name:Bob, Number:30, IsActive:True
		Name:Diana, Number:42, IsActive:True
		Name:Frank, Number:35, IsActive:True
		Name:Hank, Number:60, IsActive:True
		*/

	}
	public static void Combine_1() {
		var obs1List = new ObservableCollection<string>("ABC".Select(x => x.ToString()));
		var obs2List = new ObservableCollection<string>("efg".Select(x => x.ToString()));

		var cmbList = new ObservableCombinableCollection<string>();
		cmbList.CollectionChanged += (s, e) => {
			Console.Write($"action:{e.Action}, ");
			Console.Write($"NewIndex:{e.NewStartingIndex}, ");
			Console.Write($"NewItems:{string.Join(" ", e.NewItems?.OfType<string>() ?? Array.Empty<string>())}, ");
			Console.Write($"OldIndex:{e.OldStartingIndex}, ");
			Console.Write($"OldItems:{string.Join(" ", e.OldItems?.OfType<string>() ?? Array.Empty<string>())} \n");
		};
		cmbList.AppendCollection(obs1List);
		cmbList.AppendCollection(obs2List);

		obs1List.Remove("C");
		obs1List[1] = "D";
		obs2List.Move(0, 1);

		obs2List.Clear();
	}
	public static void Tracking_1() {


		var nds = "ABCDE".ToCharArray().ToDictionary(x => x, x => new ObservableNamedNode() { Name = x.ToString() });

		var trackings = new PropertyTrackingCollection<ObservableNamedNode>(nds.Values);

		var disp = trackings.TrackHandler(
			x => x.Parent.Parent,
			(s, e) => Console.WriteLine($"GrandParent changed, sender:{s.Name}, Property:{e.ChainedName}, G-parent:{e.PropertyValue ?? "null"}"));

		var root = nds.Values.AssembleAsNAryTree(2);
		//GrandParent changed, sender:D, Property:Parent.Parent, G-parent:A
		//GrandParent changed, sender:E, Property:Parent.Parent, G-parent:A

		Console.WriteLine(root.ToTreeDiagram(x => $"{x.Name}"));
		/*
		A
		├ B
		│ ├ D
		│ └ E
		└ C
		 */

		nds['A'].AddChild(nds['E']);
		//GrandParent changed, sender:E, Property:Parent.Parent, G-parent:null

		Console.WriteLine(root.ToTreeDiagram(x => $"{x.Name}"));
		/*
		A
		├ B
		│ └ D
		├ C
		└ E
		 */
	}
	public static void Tracking_2() {

		var data = new ObservableCollection<ItemData>() {
			new(){ Name = "Alice", Number = 50, IsActive = true },
			new(){ Name = "Bob", Number = 30, IsActive = true },
			new(){ Name = "Charlie", Number = 25, IsActive = false },
			new(){ Name = "Diana", Number = 42, IsActive = true},
			new(){ Name = "Eve", Number = 28, IsActive = false},
		};

		var trackings = new PropertyTrackingCollection<ItemData>(data);

		var trcList = trackings.CreateTrackingList();
		trcList.AttachHandler((s, e) => Console.WriteLine($"Property changed:{e.ChainedName}, Name:{s.Name}, Number:{s.Number}, IsActive:{s.IsActive}"));

		trcList.Register(x => x.Number);
		data[0].Number = 30;
		//Property changed:Number, Name:Alice, Number:30, IsActive:True
		data[1].IsActive = false;
		//

		trcList.Clear();
		trcList.Register(x => x.IsActive);
		data[0].Number = 33;
		//
		data[1].IsActive = true;
		//Property changed:IsActive, Name:Bob, Number:30, IsActive:True


	}
	public static void Tracking_4() {
		var data = new ObservableCollection<ItemData>() {
			new(){ Name = "Alice", Number = 50, IsActive = true },
			new(){ Name = "Bob", Number = 30, IsActive = true },
			new(){ Name = "Charlie", Number = 25, IsActive = false },
			new(){ Name = "Diana", Number = 42, IsActive = true},
			new(){ Name = "Eve", Number = 28, IsActive = false},
		};

		var trackings = new PropertyTrackingCollection<ItemData>(data);

		var trcList1 = trackings.CreateTrackingList();
		trcList1.Register(x => x.Number);
		trcList1.AttachHandler((s, e) => Console.WriteLine($"tracking list1, Name:{s.Name}, Number:{s.Number}"));

		var trcList2 = trackings.CreateTrackingList();
		trcList2.Register(x => x.IsActive);
		trcList2.AttachHandler((s, e) => Console.WriteLine($"tracking list2, Name:{s.Name}, IsActive:{s.IsActive}"));

		data[0].Number = 30;
		//tracking list1, Name:Alice, Number:30

		data[1].IsActive = false;
		//tracking list2, Name:Bob, IsActive:False

		var itm = new ItemData() { Name = "Frank", Number = 35, IsActive = false };
		data.Add(itm);
		itm.IsActive = true;
		//tracking list2, Name:Frank, IsActive:True

		var rmv = data[0];
		data.RemoveAt(0);
		rmv.Number = 33;
		//

	}
	public static void Tracking_3() {

		var data = new ObservableCollection<ItemData>() {
			new(){ Name = "Alice", Number = 50, IsActive = true },
			new(){ Name = "Bob", Number = 30, IsActive = true },
			new(){ Name = "Charlie", Number = 25, IsActive = false },
		};

		var trackings = new PropertyTrackingCollection<ItemData>(data);

		trackings.TrackingPropertyChanged += (s, e) => {
			Console.WriteLine($"Handle all, Property:{e.ChainedName} Name:{s.Name}, Number:{s.Number}, IsActive:{s.IsActive}");
		};

		trackings.TrackHandler(x => x.Number, (s, e) => { });
		var trcList = trackings.CreateTrackingList();
		trcList.Register(x => x.IsActive);

		data[0].Number = 30;
		//Handle all, Property:Number Name:Alice, Number:30, IsActive:True
		data[1].IsActive = false;
		//Handle all, Property:IsActive Name:Bob, Number:30, IsActive:False



		//var listener1 = new EventListener<Action<SymbolNode, ChainedPropertyChangedEventArgs<object>>>(
		//	h => trackings.TrackingPropertyChanged += h,
		//	h => trackings.TrackingPropertyChanged -= h,
		//	(s, e) => {
		//		Console.WriteLine($"Handle All, Symbol{s.Symbol}, Property:{e.ChainedName}, Value:{e.PropertyValue}");
		//	});
	}
	public static void Imitate_1() {
		var rdm = new Random();

		var nameList = new ObservableCollection<char>("ABCD".ToArray()); 
		var imitList = nameList.ToImitable(x => rdm.Next(10));

		Console.WriteLine(string.Join(",", nameList));
		//A,B,C,D
		Console.WriteLine(string.Join(",", imitList));
		//5,4,5,7

		nameList.Add('E');
		Console.WriteLine(string.Join(",", nameList));
		//A,B,C,D,E
		Console.WriteLine(string.Join(",", imitList));
		//5,4,5,7,4

		nameList.Move(0, 1);
		Console.WriteLine(string.Join(",", nameList));
		//B,A,C,D,E
		Console.WriteLine(string.Join(",", imitList));
		//4,5,5,7,4

		nameList[0] = 'F';
		Console.WriteLine(string.Join(",", nameList));
		//F,A,C,D,E
		Console.WriteLine(string.Join(",", imitList));
		//8,5,5,7,4

		imitList.Pause();

		nameList.Remove('F');
		Console.WriteLine(string.Join(",", nameList));
		//A,C,D,E
		Console.WriteLine(string.Join(",", imitList));
		//8,5,5,7,4

		imitList.Imitate();
		Console.WriteLine(string.Join(",", imitList));
		//5,5,7,4

		imitList.ClearAndPause();
		Console.WriteLine(string.Join(",", imitList));
		//

		imitList.Imitate();
		Console.WriteLine(string.Join(",", imitList));
		//1,2,5,4

	}
	public static void Imitate_2() {
		var rdm = new Random();
		var lst = new List<char>("ABCD".ToArray());
		var imitList = lst.ToImitable(x => rdm.Next(10));

		Console.WriteLine(string.Join(",", lst));
		Console.WriteLine(string.Join(",", imitList));

		lst.Remove('A');
		lst.Add('A');

		Console.WriteLine(string.Join(",", lst));
		Console.WriteLine(string.Join(",", imitList));

		imitList.Imitate();
		Console.WriteLine(string.Join(",", lst));
		Console.WriteLine(string.Join(",", imitList));
	}
	public static void Imitate_3() {
		var rdm = new Random();
		//Collection whoes elements are reference types.
		var lst = new List<string>("ABCD".ToArray().Select(x=>x.ToString()));
		var imitList = lst.ToImitable(x => rdm.Next(10));

		Console.WriteLine(string.Join(",", lst));
		//A,B,C,D
		Console.WriteLine(string.Join(",", imitList));
		//9,7,0,3

		lst.Remove("A");
		lst.Add("A");

		Console.WriteLine(string.Join(",", lst));
		//B,C,D,A
		Console.WriteLine(string.Join(",", imitList));
		//9,7,0,3

		imitList.Imitate();
		Console.WriteLine(string.Join(",", lst));
		//B,C,D,A
		Console.WriteLine(string.Join(",", imitList));
		//7,0,3,8
	}
	public static void Imitate_4() {

		var srcList = new ObservableCollection<string>("ABC".ToCharArray().Select(x => x.ToString()));
		var imitList = srcList.ToImitable(
			x => new DisposableNamedObj() { Name = x },
			obj => obj.Dispose());

		srcList.Remove("A");
		//A Dispose

		srcList.Clear();
		//B Dispose
		//C Dispose

		srcList.Add("D");
		srcList.Add("E");

		imitList.ClearAndPause();
		//D Dispose
		//E Dispose

		imitList.Imitate();

		imitList.Dispose();
		//D Dispose
		//E Dispose
	}
	public static void Aligner_0() {
		var chars = "ABCDEFGHIJ".ToCharArray();
		var rdm = new Random();
		var list = new ObservableCollection<char>(Enumerable.Range(0,5).Select(x=>chars[rdm.Next(chars.Length)]));

		list.CollectionChanged += (s, e) => {
			Console.Write($"action:{e.Action}, ");
			Console.Write($"NewIndex:{e.NewStartingIndex}, ");
			Console.Write($"NewItems:{string.Join(" ", e.NewItems?.OfType<char>() ?? Array.Empty<char>())}, ");
			Console.Write($"OldIndex:{e.OldStartingIndex}, ");
			Console.Write($"OldItems:{string.Join(" ", e.OldItems?.OfType<char>() ?? Array.Empty<char>())} \n");
		};

		Console.WriteLine(string.Join(", ", list) + "\n");

		list.AlignBy(Enumerable.Range(0, 3).Select(x => chars[rdm.Next(chars.Length)]));
		Console.WriteLine(string.Join(", ", list));

		list.AlignBy(Enumerable.Range(6, 4).Select(x => chars[x]));
		Console.WriteLine(string.Join(", ", list));

		list.AlignBy(list.AsEnumerable().Reverse().Append(chars[rdm.Next(chars.Length)]));
		Console.WriteLine(string.Join(", ", list));

		list.AlignBy(Enumerable.Empty<char>());

		/*
		C, C, G, G, B

		action:Move, NewIndex:4, NewItems:C, OldIndex:0, OldItems:C
		action:Move, NewIndex:0, NewItems:G, OldIndex:1, OldItems:G
		action:Replace, NewIndex:2, NewItems:A, OldIndex:2, OldItems:G
		action:Remove, NewIndex:-1, NewItems:, OldIndex:4, OldItems:C
		action:Remove, NewIndex:-1, NewItems:, OldIndex:3, OldItems:B
		G, C, A

		action:Replace, NewIndex:1, NewItems:H, OldIndex:1, OldItems:C
		action:Replace, NewIndex:2, NewItems:I, OldIndex:2, OldItems:A
		action:Add, NewIndex:3, NewItems:J, OldIndex:-1, OldItems:
		G, H, I, J

		action:Move, NewIndex:3, NewItems:G, OldIndex:0, OldItems:G
		action:Move, NewIndex:0, NewItems:J, OldIndex:2, OldItems:J
		action:Move, NewIndex:2, NewItems:H, OldIndex:1, OldItems:H
		action:Add, NewIndex:4, NewItems:H, OldIndex:-1, OldItems:
		J, I, H, G, H

		action:Reset, NewIndex:-1, NewItems:, OldIndex:-1, OldItems:
		*/

		/*
		D, H, B, A, H

		action:Move, NewIndex:1, NewItems:D, OldIndex:0, OldItems:D
		action:Move, NewIndex:0, NewItems:B, OldIndex:2, OldItems:B
		action:Remove, NewIndex:-1, NewItems:, OldIndex:1, OldItems:H
		action:Replace, NewIndex:2, NewItems:F, OldIndex:2, OldItems:A
		action:Remove, NewIndex:-1, NewItems:, OldIndex:3, OldItems:H
		B, D, F
		action:Replace, NewIndex:0, NewItems:G, OldIndex:0, OldItems:B
		action:Replace, NewIndex:1, NewItems:H, OldIndex:1, OldItems:D
		action:Replace, NewIndex:2, NewItems:I, OldIndex:2, OldItems:F
		action:Add, NewIndex:3, NewItems:J, OldIndex:-1, OldItems:
		G, H, I, J
		action:Move, NewIndex:3, NewItems:H, OldIndex:1, OldItems:H
		action:Move, NewIndex:1, NewItems:J, OldIndex:2, OldItems:J
		action:Add, NewIndex:4, NewItems:G, OldIndex:-1, OldItems:
		G, J, I, H, G
		action:Reset, NewIndex:-1, NewItems:, OldIndex:-1, OldItems:
		*/
	}
	public static void Aligner_1(){
		var obsLst = new ObservableCollection<string>(new string[] { "A", "B", "C", "D", });
		obsLst.CollectionChanged += (s, e) => {
			Console.Write($"action:{e.Action}, ");
			Console.Write($"OldIndex:{e.OldStartingIndex}, ");
			Console.Write($"OldItems:{string.Join(" ", e.OldItems?.OfType<string>() ?? Array.Empty<string>())}, ");
			Console.Write($"NewIndex:{e.NewStartingIndex}, ");
			Console.Write($"NewItems:{string.Join(" ", e.NewItems?.OfType<string>() ?? Array.Empty<string>())} \n");
		};

		obsLst.AlignBy("dcba",
			x => x.ToString().ToUpper(),
			(c, s) => Equals(c.ToString().ToUpper(), s));

		Console.WriteLine(string.Join(",", obsLst));
	}

	public static void Aligner_2() {
		var lst = new List<string>(new string[] {"C","O","N","F","I","N"});
		var algnr = new ListAligner<char, string, List<string>>(
			lst,
			c => c.ToString().ToUpper(),
			(c, s) => Equals(c.ToString().ToUpper(), s));

		algnr.AlignBy("finance");
		Console.WriteLine(string.Join(",", lst));

		algnr.AlignBy("finance".Reverse());
		Console.WriteLine(string.Join(",", lst));

		var obsLst = new ObservableCollection<string>(new string[] {"C","O","N","F","I","N"});
		Console.WriteLine(string.Join(",", obsLst));

		obsLst.CollectionChanged += (s, e) => {
			Console.Write($"action:{e.Action}, ");
			Console.Write($"OldIndex:{e.OldStartingIndex}, ");
			Console.Write($"OldItems:{string.Join(" ", e.OldItems?.OfType<string>() ?? Array.Empty<string>())}, ");
			Console.Write($"NewIndex:{e.NewStartingIndex}, ");
			Console.Write($"NewItems:{string.Join(" ", e.NewItems?.OfType<string>() ?? Array.Empty<string>())} \n");
		};
		var algnr2 = new ListAligner<char, string, ObservableCollection<string>>(
			obsLst,
			c => c.ToString().ToUpper(),
			(c, s) => Equals(c.ToString().ToUpper(), s),
			move: (collection, i, j) => collection.Move(i, j));

		algnr2.AlignBy("finance".ToCharArray());
		Console.WriteLine(string.Join(",", obsLst));

		algnr2.AlignBy("infinite");
		Console.WriteLine(string.Join(",", obsLst));

		algnr2.AlignBy("finery");
		Console.WriteLine(string.Join(",", obsLst));
	}
}