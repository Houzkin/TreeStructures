using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures;
using TreeStructures.Events;
using TreeStructures.Utilities;

namespace SampleConsoleApp;
public static partial class  UseageSample {
	public static void MethodL() {

		var obj = new ObservableNamedNode();

		var listener1 = new EventListener<EventHandler>(
			h => obj.Disposed += h,
			h => obj.Disposed -= h,
			(s, e) => { });

		var listener2 = new EventListener<PropertyChangedEventHandler>(
			h => obj.PropertyChanged += h,
			h => obj.PropertyChanged -= h,
			(s, e) => { });

		var listener3 = new EventListener<EventHandler<StructureChangedEventArgs<ObservableNamedNode>>>(
			h => obj.StructureChanged += h,
			h => obj.StructureChanged -= h,
			(s, e) => { });

		var listener4 = new EventListener<EventHandler<StructureChangedEventArgs<ObservableNamedNode>>, PropertyChangedEventArgs>(
			h => obj.StructureChanged += h,
			h => obj.StructureChanged -= h,
			h => (s, e) => h(s, new PropertyChangedEventArgs(e.Target.Name)),
			propchanged);

		var listeners = new LumpedDisopsables(new IDisposable[] { listener1, listener2, listener3, listener4 });
		// Unsbscribe
		listeners.Dispose();
		
	}
	static void propchanged(object? s, PropertyChangedEventArgs e) { }
}

