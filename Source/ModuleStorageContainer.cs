using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System;
using UnityEngine;

namespace ModularStorageContainer {

	public class ModuleStorageContainer: PartModule, IPartMassModifier, IPartCostModifier
	{
		static Dictionary<string, ConstructorInfo> StorageModules;

		[KSPField (isPersistant = true)]
		public double availableVolume;
		public List<IStorageContainer> containers;

		bool massDirty;
		bool costDirty;

		double totalMass;
		double totalCost;

		void ClearPartResources ()
		{
			part.Resources.dict.Clear ();
		}

		public void ContainerMassModified ()
		{
			massDirty = true;
		}

		public void ContainerCostModified ()
		{
			costDirty = true;
		}

		void FindStorageModules ()
		{
			StorageModules = new Dictionary<string, ConstructorInfo> ();
			var modules = AssemblyLoader.GetModulesImplementingInterface<IStorageContainer> (new Type[] {typeof (ModuleStorageContainer)});
			var parms = new object[] {this};
			foreach (var mod in modules) {
				IStorageContainer sc;
				// create a dummy module in order to get its name
				sc = (IStorageContainer) mod.Invoke (parms);
				Debug.LogFormat("[ModuleStorageContainer] FindStorageModules: {0}", sc.name);
				StorageModules[sc.name] = mod;
			}
		}

		public override void OnAwake ()
		{
			if (StorageModules == null) {
				FindStorageModules ();
			}
			containers = new List<IStorageContainer> ();
			if (part.partInfo != null && part.partInfo.partPrefab != null) {
				int moduleIndex;
				for (moduleIndex = 0; moduleIndex < part.Modules.Count;
					 moduleIndex++) {
					if (part.Modules[moduleIndex] == this) {
						break;
					}
				}
				Part partPrefab = part.partInfo.partPrefab;
				var mscPrefab = partPrefab.Modules[moduleIndex] as ModuleStorageContainer;

				foreach (var c in mscPrefab.containers) {
					containers.Add (c.Clone (this));
				}
			}
		}

		public override void OnStart (PartModule.StartState state)
		{
			foreach (var sc in containers) {
				sc.OnStart (state);
			}
		}

		void OnDestroy ()
		{
			foreach (var sc in containers) {
				sc.OnDestroy ();
			}
		}

		public override void OnCopy (PartModule fromModule)
		{
			Debug.LogFormat ("[ModuleStorageContainer] OnCopy: {0}", fromModule);
		}

		public override void OnLoad (ConfigNode node)
		{
			ClearPartResources ();

			var parms = new object[] {this};

			foreach (var container_node in node.GetNodes("Container")) {
				var name = container_node.GetValue ("name");
				if (!StorageModules.ContainsKey (name)) {
					Debug.Log ("ModuleStorageContainer: unknown container type: " + name);
					continue;
				}
				IStorageContainer sc;
				sc = (IStorageContainer) StorageModules[name].Invoke (parms);
				sc.Load (container_node);
				containers.Add (sc);
			}
			massDirty = true;
			costDirty = true;
		}

		public override void OnSave (ConfigNode node)
		{
			foreach (var sc in containers) {
				var container_node = node.AddNode ("Container");
				container_node.AddValue ("name", sc.name);
				sc.Save (container_node);
			}
		}

		public float GetModuleMass (float defaultMass, ModifierStagingSituation sit)
		{
			if (massDirty) {
				massDirty = false;
				totalMass = 0;
				for (int i = 0; i < containers.Count; i++) {
					totalMass += containers[i].mass;
				}
			}
			return (float) totalMass;
		}

		public ModifierChangeWhen GetModuleMassChangeWhen ()
		{
			return ModifierChangeWhen.CONSTANTLY;
		}

		public float GetModuleCost (float defaultCost, ModifierStagingSituation sit)
		{
			if (costDirty) {
				costDirty = false;
				totalCost = 0;
				for (int i = 0; i < containers.Count; i++) {
					totalCost += containers[i].cost;
				}
			}
			return (float) totalCost;
		}

		public ModifierChangeWhen GetModuleCostChangeWhen ()
		{
			return ModifierChangeWhen.CONSTANTLY;
		}
	}

}
