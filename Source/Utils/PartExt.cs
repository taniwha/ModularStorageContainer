using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;

namespace ModularStorageContainer
{

	static class PartExt
	{
		public static PartResource AddResource (this Part part, string name, double amt, double maxAmt)
		{
			ConfigNode node = new ConfigNode ("RESOURCE");
			node.AddValue ("name", name);
			node.AddValue ("amount", amt);
			node.AddValue ("maxAmount", maxAmt);

			return part.AddResource(node);
		}

		public static ModuleStorageContainer FindContainer (this Part part)
		{
			return part.FindModuleImplementing<ModuleStorageContainer> ();
		}
	}

}
