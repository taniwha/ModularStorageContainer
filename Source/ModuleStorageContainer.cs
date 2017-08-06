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
			if (part.partInfo != null && part.partInfo.partPrefab != null) {
				int moduleIndex;
				for (moduleIndex = 0; moduleIndex < part.Modules.Count;
					 moduleIndex++) {
					if (part.Modules[moduleIndex] == this) {
						break;
					}
				}
				Part partPrefab = part.partInfo.partPrefab;
				OnCopy (partPrefab.Modules[moduleIndex]);
			} else {
				containers = new List<IStorageContainer> ();
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

		void DumpPartResources ()
		{
			foreach (var r in part.Resources) {
				Debug.LogFormat ("    {0}: {1}/{2}", r.resourceName, r.amount, r.maxAmount);
			}
		}

		public override void OnCopy (PartModule fromModule)
		{
			Debug.LogFormat ("[ModuleStorageContainer] OnCopy: {0}", fromModule);
			var mscPrefab = fromModule as ModuleStorageContainer;

			containers = new List<IStorageContainer> ();
			foreach (var c in mscPrefab.containers) {
				containers.Add (c.Clone (this));
			}
		}

		IStorageContainer CreateContainer (string name)
		{
			if (!StorageModules.ContainsKey (name)) {
				Debug.Log ("ModuleStorageContainer: unknown container type: " + name);
				return null;
			}
			var parms = new object[] {this};
			return (IStorageContainer) StorageModules[name].Invoke (parms);
		}

		void BuildContainers (ConfigNode []container_nodes)
		{
			foreach (var node in container_nodes) {
				var name = node.GetValue ("name");
				IStorageContainer sc = CreateContainer (name);
				if (sc == null) {
					continue;
				}
				sc.Load (node);
				containers.Add (sc);
			}
		}

		void LoadContainers (ConfigNode []container_nodes)
		{
		}

		public override void OnLoad (ConfigNode node)
		{
			ClearPartResources ();

			ConfigNode []container_nodes = node.GetNodes ("Container");

			if (part.partInfo != null && part.partInfo.partPrefab != null) {
				LoadContainers (container_nodes);
			} else {
				BuildContainers (container_nodes);
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
