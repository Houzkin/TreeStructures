using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TreeStructures.Linq;
using TreeStructures.Xml.Serialization;

namespace SampleConsoleApp;

public static partial class UseageSample{

	public static void MethodI(){
		string path = "Test.xml";
		var tree = "ABCDEFG".ToCharArray().Select(x => new NamedNode() { Name = x.ToString() }).AssembleAsNAryTree(2);
		var seri = tree.ToSerializableNodeMap(x=>x.Name);

		//var xs = new XmlSerializer(seri.GetType());
		//using (StreamWriter sw = new StreamWriter("C:\\Users\\Owner\\Desktop.Test.xml",false,Encoding.UTF8)){
		//	xs.Serialize(sw, seri);
		//}
		var serializer = new XmlSerializer(typeof(SerializableNodeMap<string>));
		using (StreamWriter fs = new StreamWriter(path)){
			serializer.Serialize(fs, seri);
		}

		SerializableNodeMap<string> nd;
		using (StreamReader sr = new StreamReader(path)){
			nd = (SerializableNodeMap<string>)serializer.Deserialize(sr);
		}
		var tr = nd.AssembleTree(x => new NamedNode() { Name = x.ToString() });
		Console.WriteLine(tr.ToTreeDiagram(x => x.Name));
	}
}
