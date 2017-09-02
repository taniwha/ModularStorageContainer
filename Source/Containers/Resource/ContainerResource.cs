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

		void SetPartResources (Part part)
		{
			for (int i = 0; i < tanks.Count; i++) {
				var t = tanks[i];
				part.AddResource (t.name, t.amount, t.maxAmount);
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
			SetPartResources (owner.part);
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
			return clone;
		}
	}

}
