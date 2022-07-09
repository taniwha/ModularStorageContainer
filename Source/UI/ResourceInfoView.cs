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

	public class ResourceInfoView : LayoutPanel, IPointerEnterHandler, IPointerExitHandler
	{
		new UIText name;
		HorizontalLayout editTankLine;
		HorizontalLayout addTankLine;
		InputLine tankInput;

		ResourceInfo resource;
		Tank tank;

		public override void CreateUI()
		{
			base.CreateUI ();

			this.Horizontal ()
				.ControlChildSize (true, true)
				.ChildForceExpand (false, false)
				.Padding (3)
				.FlexibleLayout (true, false)
				.Add<HorizontalLayout> (out editTankLine)
					.Add<InputLine> (out tankInput)
						.Label("")
						.OnSubmit (SetTankVolume)
						.OnFocusLost (RestoreTankVolume)
						.InputWidth (100)
						.Finish ()
					.Add<UIButton> ()
						.Text ("Remove")
						.OnClick (RemoveTank)
						.Finish ()
					.Finish ()
				.Add<HorizontalLayout> (out addTankLine)
					.Add<UIText> (out name)
						.Alignment (TextAlignmentOptions.Left)
						.Size (18)
						.Finish ()
					.Add<FlexibleSpace> () .Finish ()
					.Add<UIButton> ()
						.Text ("Add")
						.OnClick (AddTank)
						.Finish ()
					.Finish ()
				;
			//
			editTankLine.SetActive (false);
			addTankLine.SetActive (false);
		}

		void SetTankVolume (string text)
		{
			resource.UpdateTank (text);
			RestoreTankVolume ();
		}

		void RestoreTankVolume (string text = null)
		{
			tankInput.text = $"{tank.maxAmount}";
		}

		void AddTank ()
		{
			resource.AddTank ();
		}

		void RemoveTank ()
		{
			resource.RemoveTank ();
		}

		public ResourceInfoView Resource (ResourceInfo resource)
		{
			this.resource = resource;
			name.Text ($"{resource.name}:");
			tankInput.Label (resource.name);
			tank = resource.FindTank ();
			if (tank == null) {
				addTankLine.SetActive (true);
				editTankLine.SetActive (false);
			} else {
				addTankLine.SetActive (false);
				editTankLine.SetActive (true);
				RestoreTankVolume ();
			}
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
