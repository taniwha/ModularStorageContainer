/*
This file is part of Advanced Input.

Advanced Input is free software: you can redistribute it and/or
modify it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Advanced Input is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Advanced Input.  If not, see
<http://www.gnu.org/licenses/>.
*/
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using KSP.UI.Screens;

using KodeUI;

namespace ModularStorageContainer {
	using Containers.Resource;

	public class ResourceInfo
	{
		public PartResourceDefinition resource { get; private set; }
		public ContainerResource container { get; private set; }
		public ResourcePanel panel { get; private set; }

		public string name
		{
			get {
				return resource.name;
			}
		}

		public Tank FindTank ()
		{
			int ind = container.FindTank (name);
			if (ind >= 0) {
				return container.tanks[ind];
			}
			return null;
		}

		public void AddTank ()
		{
			container.AddTank (resource);
			panel.UpdateStats ();
		}

		public void RemoveTank ()
		{
			container.RemoveTank (resource);
			panel.UpdateStats ();
		}

		public void UpdateTank (string amount)
		{
			container.UpdateTank (resource, amount);
			panel.UpdateStats ();
		}

		public ResourceInfo (ResourcePanel panel, PartResourceDefinition resource)
		{
			this.panel = panel;
			container = panel.container;
			this.resource = resource;
		}

		public class List : List<ResourceInfo>, UIKit.IListObject
		{
			public UnityAction<ResourceInfo> onSelected { get; set; }
			public Layout Content { get; set; }
			public RectTransform RectTransform
			{
				get { return Content.rectTransform; }
			}

			public void Create (int index)
			{
				Content
					.Add<ResourceInfoView> ()
						.Resource (this[index])
						.Finish ()
					;
			}

			public void Update (GameObject obj, int index)
			{
				var view = obj.GetComponent<ResourceInfoView> ();
				view.Resource (this[index]);
			}

			public List ()
			{
			}

			public void Update ()
			{
				for (int i = 0; i < Count; i++) {
					var obj = Content.rectTransform.GetChild(i).gameObject;
					var view = obj.GetComponent<ResourceInfoView> ();
					view.Resource (this[i]);
				}
			}
		}
	}
}
