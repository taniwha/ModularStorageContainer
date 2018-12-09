using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System;
using UnityEngine;

namespace ModularStorageContainer {

	///Interface for container modules.
	///The class implmenting IStorageContainer also needs to implment
	///the following constructor in order to be used by
	///ModuleStorageContainer:
	///
	///contructor (ModuleStorageContainer owner);
	public interface IStorageContainer {

		/// The name of the module as referenced by part config files.
		string name { get; }

		/// Minimum volume of the container. If non-zero, then the container
		/// will always have at least this volume.
		/// \note Enforced only by the user interface.
		double minVolume { get; }

		/// Maximum volume of the container. Zero means there is no maximum.
		/// Useful in conjunction with granularity to represent a maximum
		/// number of same-sized sub-modules.
		/// \note Enforced only by the user interface.
		double maxVolume { get; }

		/// The volume of the container will always be a multiple of the
		/// granularity. 0 granularity means infinitely variable.
		/// \note Enforced only by the user interface.
		double granularity { get; }

		/// The current volume of the container. Use SetVolume() to set
		/// the volume.
		double volume { get; }

		/// The current additional mass of the container.
		double mass { get; }

		/// The current additional cost of the container.
		double cost { get; }


		/// Called when ModularStorageContainer's OnStart is called.
		void OnStart (PartModule.StartState state);

		/// Called when ModularStorageContainer's OnDestroy is called.
		void OnDestroy ();

		/// Called when loading or creating the container. The config node
		/// is the container's own node.
		void Load (ConfigNode node);

		/// Called when saving the container. The config node is the
		/// container's own node and already has the name value set
		/// correctly.
		void Save (ConfigNode node);

		double SetVolume (double volume);

		/// Used for part cloning. Must return a new container suitable for
		/// adding to a new part.
		IStorageContainer Clone (ModuleStorageContainer owner);

		/// Used for GUI updates. Called by the container window's OnGUI.
		void OnGUI (IStorageContainer []counterparts);
	}

}
