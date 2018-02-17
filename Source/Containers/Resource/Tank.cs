using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;

namespace ModularStorageContainer.Containers.Resource
{
	public class Tank
	{
		public double amount;
		public double maxAmount;
		public string name;

		public Tank(Tank t)
		{
			name = t.name;
			amount = t.amount;
			maxAmount = t.maxAmount;
		}

		public Tank(string name, string amt, string maxAmt)
		{
			this.name = name;
			amount = double.Parse (amt);
			maxAmount = double.Parse (maxAmt);
		}

		public Tank(string name, double amt, double maxAmt)
		{
			this.name = name;
			amount = amt;
			maxAmount = maxAmt;
		}

		public void UpdateAmount (Part part)
		{
			amount = part.Resources[name].amount;
		}
	}
}
