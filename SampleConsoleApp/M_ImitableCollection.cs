using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.Collections;
using TreeStructures.Events;
using TreeStructures.Linq;

namespace SampleConsoleApp;

public static partial class UseageSample {
	public static void MethodM() {
		var collection = new ObservableCollection<string>(new string[] {"a","b"});
		
		//var imit = ImitableCollection.Create(
		ImitableCollection imit = collection.ToImitable(
			//collection,
			x =>{
				var conv = new ObservableNamedNode() { Name = x.ToUpper() };
				Console.WriteLine($"Create {conv.Name}");
				return conv;
			},
			x => Console.WriteLine($"Remove {x.Name}"));
		//Create A
		//Create B

		Console.WriteLine(string.Join(", ", imit.OfType<object>()));
		//A, B

		collection.Add("c");
		//Create C

		Console.WriteLine(string.Join(", ", imit.OfType<object>()));
		//A, B, C

		collection.Remove("b");
		//Remove B

		//imit.ClearAndPause();
		////RemoveA
		////RemoveC

		//Console.WriteLine($"imit is empty : {imit.IsEmpty()}");// true


		//collection.Add("d");
		//collection.Remove("a");
		//Console.WriteLine($"imit is empty : {imit.IsEmpty()}");// true

		//imit.Imitate();
		////Create C
		////Create D

		//imit.Dispose();
		////Remove C
		////Remove D

		//Console.WriteLine($"imit is empty : {imit.IsEmpty()}");// true
	}
	public static void MethodMM() {

		var collection = new ObservableCollection<string>(new string[] {"a","b"});
		//var imit = ImitableCollection.Create(
		//	collection,
		var imit = collection.ToImitable(
			x => {
				return new ObservableNamedNode() { Name = x.ToUpper() };
			});
		var readOnly = imit;

		(readOnly as INotifyCollectionChanged).CollectionChanged += (s, e) => {
			Console.WriteLine($"{e.Action},{string.Join(',',e.NewItems?.OfType<ObservableNamedNode>().Select(x=>x.ToString()) ?? new string[] { })},{string.Join(',',e.OldItems?.OfType<ObservableNamedNode>().Select(x=>x.ToString()) ?? new string[] { })}");
			Console.WriteLine($"{string.Join(',',imit)}");
		};
		//collection.AlignBy(collection.Skip(1));

		collection.Add("c");
		//Add,C,
		//A,B,C

		collection.Remove("b");
		//Remove,,B
		//A,C

		imit.ClearAndPause();
		//Reset,,

		collection.Add("d");
		collection.Remove("a");
		imit.Imitate();
		//Add,C,
		//C
		//Add,D,
		//C,D

		imit.Dispose();
		//Reset,

	}
	public static void MethodMMM() {
		var rnd = new RandomDateTime(new DateTime(2001,1,1),new DateTime(2100,12,31));
		var collection = new ObservableCollection<char>("abcd");
		var imit = collection.ToImitable(x => rnd.Next().Year);

		Console.WriteLine(string.Join("   , ", collection));
		Console.WriteLine(string.Join(", ", imit.Select(x => x.ToString())));
		//a   , b   , c   , d
		//2094, 2044, 2005, 2002

		collection.Insert(2, 'a');
		collection.Insert(4, 'c');
		Console.WriteLine(string.Join("   , ", collection));
		Console.WriteLine(string.Join(", ", imit.Select(x => x.ToString())));
		//a   , b   , a   , c   , c   , d
		//2094, 2044, 2008, 2005, 2096, 2002

		collection[3] = 'f';
		collection.Move(2, 0);
		Console.WriteLine(string.Join("   , ", collection));
		Console.WriteLine(string.Join(", ", imit.Select(x => x.ToString())));
		//a   , a   , b   , f   , c   , d
		//2008, 2094, 2044, 2029, 2096, 2002

	}
	public static void MethodMMMM() {
		var list = new ObservableCollection<string>(new string[] {"A","B","C"});
		IDisposable listener = list.Observe()
			.Added(x => Console.WriteLine($"added : {x}"))
			.Removed(x=>Console.WriteLine($"removed : {x}"));

		list.Add("D"); 
		// added : D
		Console.WriteLine(string.Join(",", list));
		//A,B,C,D

		list[0] = "E";
		//adeed : E
		//removed : A
		Console.WriteLine(string.Join(",", list));
		//E,B,C,D

		list.Move(1, 2);
		Console.WriteLine(string.Join(",", list));
		//E,C,B,D

		list.RemoveAt(0);
		//removed : E
		Console.WriteLine(string.Join(",", list));
		//C,B,D

		list.Clear();
		//removed : C
		//removed : B
		//removed : D
		Console.WriteLine(string.Join(",", list));
		//

		listener.Dispose();
		list.Add("G");
	}
}
