using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;

namespace ModularStorageContainer.Containers.Crew
{
	public class ContainerCrew: IStorageContainer
	{
		ModuleStorageContainer owner;

		public string name { get { return "Crew"; } }
		public double minVolume { get { return 0; } }
		public double granularity { get { return 0; } }
		public double volume { get; private set; }
		public double mass { get; private set; }
		public double cost { get; private set; }

		public void OnStart (PartModule.StartState state)
		{
		}

		public void OnDestroy ()
		{
		}

		public void Load (ConfigNode node)
		{
			volume = double.Parse (node.GetValue ("volume"));
			for (int i = 0; i < node.values.Count; i++) {
				string name = node.values[i].name;
				string []amounts = ParseExtensions.ParseArray (node.values[i].value);
				var t = new Tank (name, amounts[0], amounts[1]);
				tanks.Add (t);
			}
			SetPartResources (owner.part);
		}

		public void Save (ConfigNode node)
		{
			node.AddValue ("volume", volume.ToString ("R"));
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

		public ContainerCrew (ModuleStorageContainer owner)
		{
			this.owner = owner;
			tanks = new List<Tank> ();
		}
	}

}
