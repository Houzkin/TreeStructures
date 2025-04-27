using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using TreeStructures;
using TreeStructures.Collections;
using TreeStructures.Linq;
using TreeStructures.Tree;

namespace SampleConsoleApp;
public static partial class UseageSample {
	public static void MethodP() {
		var ctmAnys = new List<Anniversary>() {
			new Anniversary() { Name = "A", Date = new DateTime(2020, 2, 4) },
			new Anniversary() { Name = "B", Date = new DateTime(2020, 9, 15) },
			new Anniversary() { Name = "C", Date = new DateTime(2020, 2, 5) },
			new Anniversary() { Name = "D", Date = new DateTime(2020, 2, 3) },
			new Anniversary() { Name = "E", Date = new DateTime(2020, 2, 3) },
		};

		var ctgTree = new CategoryTree<Anniversary, int>(x=>x.Date.Year,x=>x.Date.Month,x=>x.Date.Day);
		foreach (var anv in ctmAnys) {
			ctgTree.Add(anv);
		}
		Console.WriteLine(ctgTree.Root.ToTreeDiagram(x => x.HasItem ? x.Item.ToString() : x.Category.ToString()));

		ctgTree.Remove(ctmAnys[1]);
		Console.WriteLine(ctgTree.Root.ToTreeDiagram(x => x.HasItem ? x.Item.ToString() : x.Category.ToString()));


		//var adpTree = new ReactiveCategoryTree<Anniversary, int>(ctmAnys, new Expression<Func<Anniversary,object>>[] {x=>x.Date}, EqualityComparer<int>.Default, x => x.Date.Year, x => x.Date.Month);

		//var adpTree = new ReactiveCategoryTree<Anniversary, int>(ctmAnys, PropertyHelper.ToExpression<Anniversary>(x=>x.Date), EqualityComparer<int>.Default, x => x.Date.Year, x => x.Date.Month);
		var adpTree = new ReactiveCategoryTree<Anniversary, int>(ctmAnys, new ExpressionList<Anniversary>(x => x.Date), EqualityComparer<int>.Default, x => x.Date.Year);
		var adpTree2 = new ReactiveCategoryTree<Anniversary, int>(new(x => x.Date), x => x.IsHoliday ? 0 : 1, x => x.Date.Year);
		var adpTree3 = new ReactiveCategoryTree<Anniversary, int>(new(x => x.Date), x => x.Date.Year);
	}
	public static void MethodPP() {
		var rdm = new RandomDateTime(new DateTime(2021, 1, 1), new DateTime(2022, 1, 1));
		var anys = "ABCDEFG".ToCharArray().Select(x => new Anniversary() { Name = x.ToString(), Date = rdm.Next() });
		var ctgTree = new CategoryTree<Anniversary, int>(
			anys,
			x => x.Date.Month < 4 ? x.Date.Year - 1 : x.Date.Year,
			x =>x.Date.Month < 4 ? x.Date.Month + 12 : x.Date.Month,
			x =>x.Date.Day);
		Console.WriteLine(ctgTree.Root.ToTreeDiagram(x => x.HasItem ? x.Item.ToString() : x.Category.ToString()));
		var ctgWrpr = new DateCategoryWrapper(ctgTree.Root);
		Console.WriteLine(ctgWrpr.ToTreeDiagram(x => x.HeaderString));

	}
	public static void MethodPPP() {
		var heads = new List<HeadOfState>() {
			new HeadOfState{ Name = "Naruhito",State ="Japan", Birthday = new DateTime(1960,2,23), IsRepublic = false,IsEmpress = false },
			new HeadOfState{Name = "Elizabeth2nd",State="UK",Birthday = new DateTime(1926,4,21),IsRepublic = false,IsEmpress = true },
			new HeadOfState{Name = "Charlse3rd",State="UK",Birthday=new DateTime(1948,11,14), IsRepublic=false,IsEmpress=false },
			new HeadOfState{Name = "BidenJr",State = "USA",Birthday = new DateTime(1942,11,20),IsRepublic=true,IsEmpress = false },
			new HeadOfState{Name = "Putin",State="Russia",Birthday = new DateTime(1952,10,7),IsRepublic = true,IsEmpress=false },
			new HeadOfState{Name ="Raisi",State="Iran",Birthday = new DateTime(1960,12,14),IsRepublic = true,IsEmpress=false },
			new HeadOfState{Name = "Margrethe2nd",State="Danmark",Birthday = new DateTime(1940,4,16),IsRepublic =false,IsEmpress = true },
			new HeadOfState{Name = "Erdogan",State = "Turkey",Birthday =new DateTime(1954,2,26),IsRepublic=true,IsEmpress=false },
			new HeadOfState{Name = "Charlse3rd",State="Australia",Birthday=new DateTime(1948,11,14),IsRepublic=false, IsEmpress=false },
			new HeadOfState{Name = "Charlse3rd",State="Canada",Birthday=new DateTime(1948,11,14), IsRepublic=false,IsEmpress=false }, };

		var ctgTree = new ReactiveCategoryTree<HeadOfState, string>(heads, new(x => x.Name, x => x.State), EqualityComparer<string>.Default, x => x.State, x => x.Name);
		Console.WriteLine(ctgTree.Root.ToTreeDiagram(x => x.HasItem ? $"State:{x.Item.State}, Name:{x.Item.Name}, Birthday:{x.Item.Birthday.ToString("yyyy/MM/dd")}, IsRepublic:{x.Item.IsRepublic}, IsEmpress:{x.Item.IsEmpress}" : x.Category));
		heads[0].State = "Gibli";
		heads[1].State = "Gibli";
		Console.WriteLine(ctgTree.Root.ToTreeDiagram(x => x.HasItem ? $"State:{x.Item.State}, Name:{x.Item.Name}, Birthday:{x.Item.Birthday.ToString("yyyy/MM/dd")}, IsRepublic:{x.Item.IsRepublic}, IsEmpress:{x.Item.IsEmpress}" : x.Category));
	}
}
public class DateCategoryWrapper : TreeNodeWrapper<CategoryTree<Anniversary, int>.Node, DateCategoryWrapper> {
	public DateCategoryWrapper(CategoryTree<Anniversary, int>.Node sourceNode) : base(sourceNode) {
	}

	protected override DateCategoryWrapper GenerateChild(CategoryTree<Anniversary, int>.Node sourceChildNode) {
		return new DateCategoryWrapper(sourceChildNode);
	}
	public string HeaderString {
		get {
			if (this.Source.HasItem) return this.Source.Item.ToString();
			var str = this.Source.Depth() switch {
				0 => string.Empty,
				1 => $"FY{Source.Category}",
				2 => toMonth(Source.Category > 12 ? Source.Category - 12 : Source.Category),
				3 => Source.Category.ToString() + "th",
				_ => Source.ToString()
			};
			return str;
		}
	}
	string toMonth(int month) {
		string str = month switch {
			1 => "Jan.",
			2 => "Feb.",
			3 => "Mar.",
			4 => "Apr.",
			5 => "May ",
			6 => "Jun.",
			7 => "Jul.",
			8 => "Aug.",
			9 => "Sep.",
			10 => "Oct.",
			11 => "Nov.",
			12 => "Dec.",
			_ => string.Empty,
		};
		return str;
	}
}
