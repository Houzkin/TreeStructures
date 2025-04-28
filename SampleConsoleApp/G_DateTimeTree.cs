using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TreeStructures;
using TreeStructures.Collections;
using TreeStructures.Linq;
using TreeStructures.Tree;

namespace SampleConsoleApp;
public static partial class UseageSample {

	public static void MethodG() {
		#region debug test
		//var rmdtst = new RandomDateTime(new DateTime(2020, 1, 1), new DateTime(2020, 3, 1));
		//var srccoll = new ObservableCollection<Anniversary>();
		//var srcAnnys = "ABCDEFG".ToCharArray().Select(x => new Anniversary() { Name = x.ToString(), Date = rmdtst.Next() });
		//var wrprcoll = new ReadOnlySortFilterObservableCollection<Anniversary>(srccoll);
		//wrprcoll.SortBy(x => x.Date);

		//foreach (var e in srcAnnys) srccoll.Add(e);

		//Console.WriteLine("src collection");
		//Console.WriteLine(string.Join("\n", srccoll));
		//Console.WriteLine("sorted collection");
		//Console.WriteLine(string.Join("\n", wrprcoll));

		////Console.WriteLine("Add X");
		////srccoll.Add(new Anniversary() { Name = "X", Date = new DateTime(2020, 2, 2) });

		//Console.WriteLine("Remove at 0");
		//srccoll.RemoveAt(0);

		//Console.WriteLine("src collection");
		//Console.WriteLine(string.Join("\n", srccoll));
		//Console.WriteLine("sorted collection");
		//Console.WriteLine(string.Join("\n", wrprcoll));


		#endregion
		//var rmd = new RandomDateTime(new DateTime(2021, 1, 1), new DateTime(2022, 1, 1));
		//var Anniversarys = new ObservableCollection<Anniversary>();
		//var anys = "ABCDEF".ToCharArray().Select(x => new Anniversary() { Name = x.ToString(), Date = rmd.Next() });

		//foreach (var item in anys) Anniversarys.Add(item);
		//var tree = new DateTimeTree<Anniversary>(Anniversarys,
		//	x => x.Date, d => d.Month < 4 ? d.Year - 1 : d.Year,
		//	d => d.Month < 4 ? d.Month + 12 : d.Month, d => d.Day);
		//Console.WriteLine(tree.Root.ToTreeDiagram(x => x.HasItemAndValue ? x.Item.ToString() : x.NodeClass.ToString()));

		//var DispTree = new AnnivWrapper(tree.Root);
		//Console.WriteLine(DispTree.ToTreeDiagram(x => x.HeaderString));

		////Anniversarys.RemoveAt(0);
		//Anniversarys.Add(new Anniversary() { Name = "X", Date = rmd.Next() });
		//Anniversarys.Move(1, 0);
		//Console.WriteLine(tree.Root.ToTreeDiagram(x => x.HasItemAndValue ? x.Item.ToString() : x.NodeClass.ToString()));
		//Console.WriteLine(DispTree.ToTreeDiagram(x => x.HeaderString));

	}

}
class RandomDateTime {
	DateTime start;
	Random gen;
	int range;
	public RandomDateTime(DateTime start, DateTime until) {
		this.start = start;
		this.gen = new Random();
		range = (until - start).Days;
	}
	public DateTime Next() {
		return start.AddDays(gen.Next(range)).AddHours(gen.Next(0, 24)).AddMinutes(gen.Next(0, 60)).AddSeconds(gen.Next(0, 60));
	}
}
public class Anniversary {
	public DateTime Date { get; set; }
	public string Name { get; set; }

	public override string ToString() {
		return $"{Name} : {Date}";
	}
	public bool IsHoliday { get; set; }
}
//public class AnnivWrapper : TreeNodeWrapper<DateTimeTree<Anniversary>.Node, AnnivWrapper> {
//	public AnnivWrapper(DateTimeTree<Anniversary>.Node node) : base(node) { }
//	protected override AnnivWrapper GenerateChild(DateTimeTree<Anniversary>.Node sourceChildNode) {
//		return new AnnivWrapper(sourceChildNode);
//	}
//	public string HeaderString {
//		get{
//			if(this.Source.HasItemAndValue){
//				return this.Source.Item.ToString();
//			}
//			var str = this.Source.Depth() switch {
//				0 => string.Empty,
//				1 => $"FY{Source.NodeClass}",
//				2 => toMonth(Source.NodeClass > 12 ? Source.NodeClass - 12 : Source.NodeClass),
//				3 => Source.NodeClass.ToString()+"th",
//				_ => this.Source.ToString()
//			}; ;
//			return str;
//		}
//	}
//	private string toMonth(int month){
//		string str = month switch {
//			1 => "Jan.",
//			2 => "Feb.",
//			3 => "Mar.",
//			4 => "Apr.",
//			5 => "May ",
//			6 => "Jun.",
//			7 => "Jul.",
//			8 => "Aug.",
//			9 => "Sep.",
//			10 => "Oct.",
//			11 => "Nov.",
//			12 => "Dec.",
//			_ => string.Empty,
//		};
//		return str;
//	}
//}
