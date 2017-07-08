using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System;
using UnityEngine;

namespace ModularStorageContainer {

	public interface IStorageContainer {
		string name { get; }
		double minVolume { get; }
		double granularity { get; }
		double volume { get; }
		double mass { get; }
		double cost { get; }

		void OnStart (PartModule.StartState state);
		void OnDestroy ();
		void Load (ConfigNode node);
		void Save (ConfigNode node);

		double SetVolume (double volume);

		IStorageContainer Clone (ModuleStorageContainer owner);

		//contructor (ModuleStorageContainer owner);
	}

}
