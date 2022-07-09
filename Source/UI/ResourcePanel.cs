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
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using KSP.Localization;
using TMPro;

using KodeUI;

namespace ModularStorageContainer {
	using Containers.Resource;

	public class ResourcePanel : LayoutPanel, IPointerEnterHandler, IPointerExitHandler
	{
		ResourceInfo.List resourceInfos;
		InfoLine volumeInfo;
		InfoLine massInfo;
		InfoLine costInfo;
		VerticalLayout resources;
		UIButton removeAll;

		public ContainerResource container { get; private set; }

		public override void CreateUI()
		{
			base.CreateUI ();

			this.Vertical ()
				.ControlChildSize (true, true)
				.ChildForceExpand (false, false)
				.Padding (3)
				.FlexibleLayout (false, true)
				.Add<InfoLine> (out volumeInfo) .Label ("Volume") .Finish ()
				.Add<InfoLine> (out massInfo) .Label ("Mass") .Finish ()
				.Add<InfoLine> (out costInfo) .Label ("Cost") .Finish ()
				.Add<UIButton> (out removeAll)
					.Text ("Remove All")
					.OnClick (RemoveAll)
					.Finish ()
				.Add<VerticalLayout> (out resources) .Finish ()
				.Finish ();
			//
			resourceInfos = new ResourceInfo.List ();
			resourceInfos.Content = resources;
		}

		void RemoveAll ()
		{
			container.RemoveAll ();
			UpdateStats ();
		}

		public void UpdateStats ()
		{
			double volume = container.resourceVolume / 1000;
			volumeInfo.Info ($"{volume}/{container.volume}kL");
			massInfo.Info ($"{container.mass}t");
			costInfo.Info ($"{container.cost}");
			removeAll.interactable = container.tanks.Count > 0;
			resourceInfos.Update ();
		}

		void BuildResources ()
		{
			resourceInfos.Clear ();
			foreach (var res in PartResourceLibrary.Instance.resourceDefinitions) {
				if (res.isVisible) {
					resourceInfos.Add (new ResourceInfo(this, res));
				}
			}
			UIKit.UpdateListContent (resourceInfos);
		}

		public ResourcePanel SetContainer (ContainerResource container, IStorageContainer []counterparts)
		{
			this.container = container;
			BuildResources ();
			UpdateStats ();
			return this;
		}

#region OnPointerEnter/Exit
		public void OnPointerEnter (PointerEventData eventData)
		{
		}

		public void OnPointerExit (PointerEventData eventData)
		{
		}
#endregion
	}
}
