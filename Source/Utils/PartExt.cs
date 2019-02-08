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
			var resDef = PartResourceLibrary.Instance.GetDefinition (name);

			PartResource partResource = new PartResource (part);
			partResource.resourceName = name;
			partResource.SetInfo (resDef);

			partResource.amount = amt;
			partResource.maxAmount = maxAmt;
			partResource._flowState = true;

			partResource.isTweakable = resDef.isTweakable;
			partResource.isVisible = resDef.isVisible;
			partResource.hideFlow = false;
			partResource._flowMode = PartResource.FlowMode.Both;

			part.Resources.dict.Add (resDef.id, partResource);

			return partResource;
		}

		public static ModuleStorageContainer FindContainer (this Part part)
		{
			return part.FindModuleImplementing<ModuleStorageContainer> ();
		}
	}
}
