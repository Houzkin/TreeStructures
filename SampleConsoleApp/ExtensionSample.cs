using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.Linq;

namespace SampleConsoleApp;

public static partial class ExtensionSample{

	public static void EnumerableSample(){

		var list = new List<int> { 10, 21, 32, 43, 5, 65, 70, 18, 29 };
		list.AlignBy(list.OrderBy(x => x).TakeWhile(x => x < 50));

		Console.WriteLine(string.Join(", ", list));
		// 5, 10, 18, 21, 29, 32, 43
	}
}
