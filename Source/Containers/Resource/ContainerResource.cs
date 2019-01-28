using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;

namespace ModularStorageContainer.Containers.Resource
{
	public class ContainerResource: IStorageContainer
	{
		ModuleStorageContainer owner;

		public string name { get { return "Resource"; } }
		public double minVolume { get { return 0; } }
		public double maxVolume { get { return 0; } }
		public double granularity { get { return 0; } }
		public double volume { get; private set; }
		public double mass { get; private set; }
		public double cost { get; private set; }

		List<Tank> tanks;

		void _SetPartResources ()
		{
			owner.ClearPartResources ();
			Part part = owner.part;
			for (int i = 0; i < tanks.Count; i++) {
				var t = tanks[i];
				part.AddResource (t.name, t.amount, t.maxAmount);
			}
		}

		void SetPartResources ()
		{
			_SetPartResources ();
			if (HighLogic.LoadedSceneIsEditor) {
				GameEvents.onEditorShipModified.Fire (EditorLogic.fetch.ship);
			}
		}

		public void OnStart (PartModule.StartState state)
		{
		}

		public void OnDestroy ()
		{
		}

		public void Load (ConfigNode node)
		{
			if (node.HasValue ("volume")) {
				volume = double.Parse (node.GetValue ("volume"));
			}
			for (int i = 0; i < node.values.Count; i++) {
				string name = node.values[i].name;
				if (name == "volume" || name == "name") {
					continue;
				}
				string []amounts = ParseExtensions.ParseArray (node.values[i].value);
				var t = new Tank (name, amounts[0], amounts[1]);
				tanks.Add (t);
			}
			SetPartResources ();
		}

		public void Save (ConfigNode node)
		{
			node.AddValue ("volume", volume.ToString ("G17"));
			foreach (var t in tanks) {
				t.UpdateAmount (owner.part);
				node.AddValue (t.name, String.Format ("{0:G17}, {1:G17}", t.amount, t.maxAmount));
			}
		}

		public double SetVolume (double newVolume)
		{
			double delta;
			if (newVolume > 0) {
				if (volume > 0) {
					double scale = newVolume / volume;
					for (int i = 0; i < tanks.Count; i++) {
						tanks[i].maxAmount *= scale;
						tanks[i].amount *= scale;
					}	
				}
				delta = newVolume - volume;
				volume = newVolume;
			} else {
				delta = -volume;
				volume = 0;
				tanks.Clear ();
			}
			return delta;
		}

		ContainerResource ()
		{
		}

		public ContainerResource (ModuleStorageContainer owner)
		{
			this.owner = owner;
			tanks = new List<Tank> ();
		}

		public IStorageContainer Clone (ModuleStorageContainer owner)
		{
			ContainerResource clone = new ContainerResource ();
			clone.volume = volume;
			clone.owner = owner;
			clone.tanks = new List<Tank> ();
			foreach (var t in tanks) {
				clone.tanks.Add (new Tank(t));
			}
			clone._SetPartResources ();
			return clone;
		}

		int FindTank (string resource)
		{
			for (int i = tanks.Count; i-- > 0; ) {
				if (tanks[i].name == resource) {
					return i;
				}
			}
			return -1;
		}

		double resourceVolume = -1;

		void AddTank (string name, double amount, double maxAmount)
		{
			var t = new Tank (name, amount, maxAmount);
			tanks.Add (t);
			resourceVolume = -1;
			oldAmounts = null;
			SetPartResources ();
		}

		void AddTank_GUI (PartResourceDefinition res)
		{
			if (GUILayout.Button ("Add")) {
				double vol = (volume - resourceVolume) * 1000;
				double maxAmount = vol / res.volume;
				double amount = maxAmount;
				if (!res.isTweakable) {
					amount = 0;
				}
				AddTank (res.name, amount, maxAmount);
				for (int i = counterparts.Length; i-- > 0; ) {
					var c = (ContainerResource) counterparts[i];
					c.AddTank (res.name, amount, maxAmount);
				}
			}
		}

		void RemoveTank (int tankInd)
		{
			tanks.RemoveAt(tankInd);
			resourceVolume = -1;
			oldAmounts = null;
			SetPartResources ();
		}

		void RemoveTank_GUI (int tankInd)
		{
			if (GUILayout.Button ("Remove")) {
				RemoveTank (tankInd);
				for (int i = counterparts.Length; i-- > 0; ) {
					var c = (ContainerResource) counterparts[i];
					c.RemoveTank (tankInd);
				}
			}
		}

		void RemoveAll ()
		{
			tanks.Clear ();
			resourceVolume = -1;
			oldAmounts = null;
			SetPartResources ();
		}

		void RemoveAll_GUI ()
		{
			bool gui_enabled = GUI.enabled;
			GUI.enabled = tanks.Count > 0;
			if (GUILayout.Button ("Remove All")) {
				RemoveAll ();
				for (int i = counterparts.Length; i-- > 0; ) {
					var c = (ContainerResource) counterparts[i];
					c.RemoveAll ();
				}
			}
			GUI.enabled = gui_enabled;
		}

		void UpdateTank (int tankInd, double amount, double maxAmount)
		{
			Tank tank = tanks[tankInd];
			tank.maxAmount = maxAmount;
			tank.amount = amount;
			resourceVolume = -1;
			SetPartResources ();
		}

		bool UpdateTank_GUI (int tankInd, string newAmount)
		{
			Tank tank = tanks[tankInd];
			var res = PartResourceLibrary.Instance.GetDefinition (tank.name);
			double maxAmount;
			if (!double.TryParse (newAmount, out maxAmount)) {
				return false;
			}
			double limit = (volume - resourceVolume) * 1000 / res.volume;
			limit += tank.maxAmount;
			if (maxAmount > limit) {
				maxAmount = limit;
			}
			double amount = maxAmount;
			if (!res.isTweakable) {
				amount = 0;
			}
			UpdateTank (tankInd, amount, maxAmount);
			for (int i = counterparts.Length; i-- > 0; ) {
				var c = (ContainerResource) counterparts[i];
				c.UpdateTank (tankInd, amount, maxAmount);
			}
			return true;
		}

		IStorageContainer []counterparts;
		string []oldAmounts;
		string []newAmounts;
		GUILayoutOption inputWidth;

		void EditTank_GUI (int tankInd)
		{
			Tank tank = tanks[tankInd];

			string name = "" + tankInd + ".ContainerResource.MSC";
			GUI.SetNextControlName (name);
			if (GUI.GetNameOfFocusedControl () == name) {
				if (Event.current.isKey) {
					switch (Event.current.keyCode) {
						case KeyCode.Return:
						case KeyCode.KeypadEnter:
							Event.current.Use ();
							if (newAmounts[tankInd] != oldAmounts[tankInd]) {
								if (UpdateTank_GUI (tankInd, newAmounts[tankInd])) {
									oldAmounts[tankInd] = null;
								}
							}
							break;
					}
				}
			} else {
				newAmounts[tankInd] = oldAmounts[tankInd];
			}
			if (oldAmounts[tankInd] == null) {
				oldAmounts[tankInd] = tank.maxAmount.ToString ();
				newAmounts[tankInd] = oldAmounts[tankInd];
			}
			GUIStyle style = ContainerWindow.unchanged;
			if (newAmounts[tankInd] != oldAmounts[tankInd]) {
				style = ContainerWindow.changed;
			}
			newAmounts[tankInd] = GUILayout.TextField (newAmounts[tankInd], style, inputWidth);
		}

		void ResourceLine (PartResourceDefinition res)
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Label (res.name);
			GUILayout.FlexibleSpace ();
			if (resourceVolume >= 0) {
				int tankInd = FindTank (res.name);
				if (tankInd != -1) {
					EditTank_GUI (tankInd);
					RemoveTank_GUI (tankInd);
				} else {
					if (resourceVolume < volume) {
						AddTank_GUI (res);
					}
				}
			}
			GUILayout.EndHorizontal ();
		}

		static List<PartResourceDefinition> resources;
		public void OnGUI (IStorageContainer []counterparts)
		{
			this.counterparts = counterparts;

			if (inputWidth == null) {
				inputWidth = GUILayout.Width (127);
			}
			if (resources == null) {
				resources = new List<PartResourceDefinition> ();
				foreach (var res in PartResourceLibrary.Instance.resourceDefinitions) {
					if (res.isVisible) {
						resources.Add (res);
					}
				}
			}
			if (resourceVolume == -1) {
				resourceVolume = 0;
				for (int i = 0; i < tanks.Count; i++) {
					Tank t = tanks[i];
					var res = PartResourceLibrary.Instance.GetDefinition (t.name);
					resourceVolume += t.maxAmount * res.volume;;
				}
				resourceVolume /= 1000;
			}
			if (oldAmounts == null) {
				oldAmounts = new string[tanks.Count];
				newAmounts = new string[tanks.Count];
			}
			GUILayout.BeginVertical ();
			GUILayout.Label ("Volume: " + resourceVolume + "/" + volume + "kL");
			GUILayout.Label ("Mass: " + mass + "t");
			GUILayout.Label ("Cost: " + cost);
			RemoveAll_GUI ();
			for (int i = 0; i < resources.Count; i++) {
				ResourceLine (resources[i]);
			}
			GUILayout.EndVertical ();
		}
	}

}
