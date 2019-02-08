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

		public void MarkWindowDirty ()
		{
			if (UIPartActionController.Instance == null) {
				// no controller means no window to mark dirty
				return;
			}
			var action_window = UIPartActionController.Instance.GetItem(part);
			if (action_window == null) {
				return;
			}
			action_window.displayDirty = true;
		}

		public void ClearPartResources ()
		{
			part.Resources.Clear ();
			part.SimulationResources.Clear ();
			GameEvents.onPartResourceListChange.Fire (part);
			MarkWindowDirty ();
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

		bool isEditor { get { return HighLogic.LoadedSceneIsEditor; } }

		public override void OnStart (PartModule.StartState state)
		{
			foreach (var sc in containers) {
				sc.OnStart (state);
			}

			Events["HideUI"].active = false;
			Events["ShowUI"].active = true;

			if (isEditor) {
				GameEvents.onPartActionUIDismiss.Add (OnPartActionGuiDismiss);
				ContainerWindow.OnActionGroupEditorOpened.Add (OnActionGroupEditorOpened);
				ContainerWindow.OnActionGroupEditorClosed.Add (OnActionGroupEditorClosed);
			}

		}

		void OnDestroy ()
		{
			foreach (var sc in containers) {
				sc.OnDestroy ();
			}
			if (isEditor) {
				GameEvents.onPartActionUIDismiss.Remove (OnPartActionGuiDismiss);
				ContainerWindow.OnActionGroupEditorOpened.Remove (OnActionGroupEditorOpened);
				ContainerWindow.OnActionGroupEditorClosed.Remove (OnActionGroupEditorClosed);
			}
		}

		public void DumpPartResources ()
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
			containers.Clear ();
			BuildContainers (container_nodes);
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
			MarkWindowDirty ();
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

		private void OnPartActionGuiDismiss(Part p)
		{
			if (p == part) {
				HideUI ();
			}
		}

		void OnActionGroupEditorOpened ()
		{
			Events["HideUI"].active = false;
			Events["ShowUI"].active = false;
		}

		void OnActionGroupEditorClosed ()
		{
			Events["HideUI"].active = false;
			Events["ShowUI"].active = true;
		}

		[KSPEvent (guiActiveEditor = true, guiName = "Hide UI", active = false)]
		public void HideUI ()
		{
			ContainerWindow.HideGUI ();
			UpdateMenus (false);
		}

		[KSPEvent (guiActiveEditor = true, guiName = "Show UI", active = false)]
		public void ShowUI ()
		{
			ContainerWindow.ShowGUI (this);
			UpdateMenus (true);
		}

		void UpdateMenus (bool visible)
		{
			Events["HideUI"].active = visible;
			Events["ShowUI"].active = !visible;
		}
	}

}
