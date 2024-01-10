using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleConsoleApp;
public static partial class UseageSample{
	public static void MethodF(){
		var nodeA = new MemberNode("A");
		var wrprA = new MemberWrapper<MemberNode>(nodeA);
		var wrprAA = new MemberWrapper<MemberNode>(nodeA);

		var NullWrpr1 = new MemberWrapper<MemberNode>(null);
		var NullWrpr2 = new MemberWrapper<MemberNode>(null);

		var result1 = wrprA == wrprAA;
		Console.WriteLine(result1);//true

		var result2 = NullWrpr1 == NullWrpr2;
		Console.WriteLine(result2);//true

		var resutl3 = wrprA == new MemberWrapper<MemberNode>(new MemberNode("A"));
		Console.WriteLine(resutl3);
		
	}
}
