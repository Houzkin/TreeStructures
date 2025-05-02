using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures;
using TreeStructures.Events;

namespace SampleConsoleApp{
	public class PersonNode : ObservableGeneralTreeNode<PersonNode> {
		public PersonNode() { }
		string name = string.Empty;
		public string Name {
			get => name;
			set => SetProperty(ref name, value);
		}
		PersonalInfo personalInfo;
		public PersonalInfo PersonalData {
			get => personalInfo;
			set => SetProperty(ref personalInfo, value);
		}
		DateTime appo;
		public DateTime NextAppointment {
			get => appo; set => SetProperty(ref appo, value);
		}
	}
	public class PersonalInfo : INotifyPropertyChanged{
		public PersonalInfo() {
			this.propChangeProxy = new PropertyChangeProxy(this);
		}
		PropertyChangeProxy propChangeProxy { get;}
		public event PropertyChangedEventHandler? PropertyChanged {
			add { this.propChangeProxy.Changed += value; }
			remove { this.propChangeProxy.Changed -= value; }
		}
		int height;
		/// <summary>(cm)</summary>
		public int Height {
			get => height;
			set { if (propChangeProxy.SetWithNotify(ref height, value)) propChangeProxy.Notify(nameof(BMI)); }
		}
		int weight;
		/// <summary>(kg)</summary>
		public int Weight {
			get => weight;
			set { if (propChangeProxy.SetWithNotify(ref weight, value)) propChangeProxy.Notify(nameof(BMI)); }
		}
		public double BMI => Weight / Math.Pow(Height * 0.01, 2);

		string state = String.Empty;
		public string State {
			get => state; 
			set => propChangeProxy.SetWithNotify(ref state, value);
		}
		string job = String.Empty;
		public string Job {
			get => job; 
			set => propChangeProxy.SetWithNotify(ref job, value);
		}
	}
}
