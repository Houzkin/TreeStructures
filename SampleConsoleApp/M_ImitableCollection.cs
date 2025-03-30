using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.Collections;

namespace SampleConsoleApp;

public static partial class UseageSample {
	public static void MethodM() {
		var collection = new ObservableCollection<string>(new string[] {"a","b"});
		var imit = ImitableCollection.Create(
			collection,
			x =>{
				var conv = new ObservableNamedNode() { Name = x.ToUpper() };
				Console.WriteLine($"Create {conv.Name}");
				return conv;
			},
			x => Console.WriteLine($"Removed {x.Name}"));
		//Create A
		//Create B

		Console.WriteLine(string.Join(", ", imit));
		//A, B

		collection.Add("c");
		//Create C

		Console.WriteLine(string.Join(", ", imit));
		//A, B, C

		collection.Remove("b");
		//Removed B

		imit.PauseImitateAndClear();
		//RemoveA
		//RemoveC

		Console.WriteLine($"imit is empty : {imit.IsEmpty()}");// true


		collection.Add("d");
		collection.Remove("a");
		Console.WriteLine($"imit is empty : {imit.IsEmpty()}");// true

		imit.Imitate();
		//Create C
		//Create D


	}
}
