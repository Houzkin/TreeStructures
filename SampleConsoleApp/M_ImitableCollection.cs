using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.Collections;
using TreeStructures.Linq;

namespace SampleConsoleApp;

public static partial class UseageSample {
	public static void MethodM() {
		var collection = new ObservableCollection<string>(new string[] {"a","b"});
		
		//var imit = ImitableCollection.Create(
		var imit = collection.ToImitable(
			//collection,
			x =>{
				var conv = new ObservableNamedNode() { Name = x.ToUpper() };
				Console.WriteLine($"Create {conv.Name}");
				return conv;
			},
			x => Console.WriteLine($"Remove {x.Name}"));
		//Create A
		//Create B

		Console.WriteLine(string.Join(", ", imit));
		//A, B

		collection.Add("c");
		//Create C

		Console.WriteLine(string.Join(", ", imit));
		//A, B, C

		collection.Remove("b");
		//Remove B

		imit.ClearAndPause();
		//RemoveA
		//RemoveC

		Console.WriteLine($"imit is empty : {imit.IsEmpty()}");// true


		collection.Add("d");
		collection.Remove("a");
		Console.WriteLine($"imit is empty : {imit.IsEmpty()}");// true

		imit.Imitate();
		//Create C
		//Create D

		imit.Dispose();
		//Remove C
		//Remove D

		Console.WriteLine($"imit is empty : {imit.IsEmpty()}");// true
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
}
