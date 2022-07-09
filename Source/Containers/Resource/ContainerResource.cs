using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;

using KodeUI;

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

		private double _ressourceVolume = -1;
		public double resourceVolume
		{
			get {
				if (_ressourceVolume == -1) {
					_ressourceVolume = 0;
					for (int i = 0; i < tanks.Count; i++) {
						var t = tanks[i];
						var res = PartResourceLibrary.Instance.GetDefinition (t.name);
						_ressourceVolume += t.maxAmount * res.volume;
					}
				}
				return _ressourceVolume;
			}
			private set {
				_ressourceVolume = value;
			}
		}

		public List<Tank> tanks { get; private set; }

		void _SetPartResources ()
		{
			owner.ClearPartResources ();
			Part part = owner.part;
			for (int i = 0; i < tanks.Count; i++) {
				var t = tanks[i];
				part.AddResource (t.name, t.amount, t.maxAmount);
			}
			part.ResetSimulationResources ();
		}

		void SetPartResources ()
		{
			_SetPartResources ();
			if (HighLogic.LoadedSceneIsEditor) {
				GameEvents.onEditorShipModified.Fire (EditorLogic.fetch.ship);
				GameEvents.onPartResourceListChange.Fire (owner.part);
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
				node.AddValue (t.name, $"{t.amount:G17}, {t.maxAmount:G17}");
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

		internal int FindTank (string resource)
		{
			for (int i = tanks.Count; i-- > 0; ) {
				if (tanks[i].name == resource) {
					return i;
				}
			}
			return -1;
		}

		void addTank (string name, double amount, double maxAmount)
		{
			var t = new Tank (name, amount, maxAmount);
			tanks.Add (t);
			SetPartResources ();
			resourceVolume = -1;
		}

		void removeTank (int tankInd)
		{
			tanks.RemoveAt(tankInd);
			SetPartResources ();
			resourceVolume = -1;
		}

		void removeAll ()
		{
			tanks.Clear ();
			SetPartResources ();
			resourceVolume = -1;
		}

		IStorageContainer []counterparts;

		public void RemoveAll ()
		{
			removeAll ();
			for (int i = counterparts.Length; i-- > 0; ) {
				var c = (ContainerResource) counterparts[i];
				c.removeAll ();
			}
		}

		public void AddTank (PartResourceDefinition res)
		{
			double vol = volume * 1000 - resourceVolume;
			double maxAmount = vol / res.volume;
			double amount = maxAmount;
			if (!res.isTweakable) {
				amount = 0;
			}
			if (maxAmount <= 0) {
				return;
			}
			addTank (res.name, amount, maxAmount);
			for (int i = counterparts.Length; i-- > 0; ) {
				var c = (ContainerResource) counterparts[i];
				c.addTank (res.name, amount, maxAmount);
			}
		}

		public void RemoveTank (PartResourceDefinition res)
		{
			int tankInd = FindTank (res.name);
			removeTank (tankInd);
			for (int i = counterparts.Length; i-- > 0; ) {
				var c = (ContainerResource) counterparts[i];
				c.removeTank (tankInd);
			}
		}

		void UpdateTank (int tankInd, double amount, double maxAmount)
		{
			Tank tank = tanks[tankInd];
			tank.maxAmount = maxAmount;
			tank.amount = amount;
			resourceVolume = -1;
			SetPartResources ();
		}

		public bool UpdateTank (PartResourceDefinition res, string newAmount)
		{
			int tankInd = FindTank (res.name);
			Tank tank = tanks[tankInd];
			double maxAmount;
			if (!double.TryParse (newAmount, out maxAmount)) {
				return false;
			}
			double limit = (volume * 1000 - resourceVolume) / res.volume;
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

		public void CreateUI (Layout content, IStorageContainer []counterparts)
		{
			this.counterparts = counterparts;
			var panel = UIKit.CreateUI<ResourcePanel> (content.rectTransform, "MSC.ResourcePanel");
			panel.SetContainer (this, counterparts);
		}
	}

}
