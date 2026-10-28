using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;
using TreeStructures;
using TreeStructures.Events;

namespace SampleConsoleApp{
	public class ItemData : NotifyObjA {

		public string Name {
			get => Properties.GetValue("No name");
			set => Properties.TrySetValue(value);
		}	
		public int Number {
			get => Properties.GetValue(0);
			set => Properties.TrySetValue(value);
		}
		public bool IsActive {
			get => Properties.GetValue<bool>();
			set => Properties.TrySetValue(value);
		}
		public override string ToString() {
			return $"Name:{this.Name}, Number:{Number}, IsActive:{IsActive}";
		}
	}
}
