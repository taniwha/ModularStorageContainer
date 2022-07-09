/*
This file is part of Modular Storage Container.

Modular Storage Container is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Modular Storage Container is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with Modular Storage Container.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using KSP.UI.Screens;
using KodeUI;

namespace ModularStorageContainer
{
	[KSPAddon (KSPAddon.Startup.EditorAny, false)]
	public class MSCWindowManager : MonoBehaviour
	{
		static Canvas appCanvas;
		public static RectTransform appCanvasRect { get; private set; }

		void Awake ()
		{
			appCanvas = DialogCanvasUtil.DialogCanvas;
			appCanvasRect = appCanvas.transform as RectTransform;
		}

		void Start ()
		{
		}

		void OnDestroy ()
		{
		}

		static ContainerWindow container_window;

		public static void HideGUI ()
		{
			if (!container_window) {
				return;
			}
			container_window.SetContainer (null);
		}

		public static void ShowGUI (ModuleStorageContainer container)
		{
			if (!container_window) {
				container_window = UIKit.CreateUI<ContainerWindow> (appCanvasRect, "MSC.ContainerWindow");
			}
			container_window.SetContainer (container);
			container_window.rectTransform.SetAsLastSibling ();
		}

		private void onEditorLoad (ShipConstruct ship, CraftBrowserDialog.LoadType loadType)
		{
			Debug.LogFormat ("[ContainerWindow] onEditorLoad: {0}", loadType);
			for (int i = 0, c = ship.parts.Count; i < c; ++i) {
				Part part = ship.parts[i];
				for (int j = 0, d = part.Modules.Count; j < d; ++j) {
					PartModule module = part.Modules[j];
					if (module is ModuleStorageContainer) {
						//(module as ModuleStorageContainer).UpdateUsedBy ();
					}
				}
			}
		}

		internal static EventVoid OnActionGroupEditorOpened = new EventVoid ("OnActionGroupEditorOpened");
		internal static EventVoid OnActionGroupEditorClosed = new EventVoid ("OnActionGroupEditorClosed");
		bool ActionGroupMode;
		Part selected_part;
		private IEnumerator CheckActionGroupEditor ()
		{
			while (EditorLogic.fetch == null) {
				yield return null;
			}
			EditorLogic editor = EditorLogic.fetch;
			while (editor != null) {
				//FIXME check for events
				if (editor.editorScreen == EditorScreen.Actions) {
					if (!ActionGroupMode) {
//						HideGUI ();
//						OnActionGroupEditorOpened.Fire ();
					}
					var age = EditorActionGroups.Instance;
					if (selected_part) {
						if (!age.SelectionContains (selected_part)) {
							selected_part = null;
//							HideGUI ();
						}
					} else if (age.HasSelectedParts ()) {
						selected_part = age.GetSelectedParts ()[0];
						var container = selected_part.FindContainer ();
						if (container) {
//							ShowGUI (container);
						}
					}
					ActionGroupMode = true;
				} else {
					if (ActionGroupMode) {
//						HideGUI ();
//						OnActionGroupEditorClosed.Fire ();
					}
					ActionGroupMode = false;
				}
				yield return null;
			}
		}
	}

}
