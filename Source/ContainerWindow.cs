using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Collections.ObjectModel;

using KSP.UI.Screens;

namespace ModularStorageContainer
{
	[KSPAddon (KSPAddon.Startup.EditorAny, false)]
    public class ContainerWindow : MonoBehaviour
    {
		internal static EventVoid OnActionGroupEditorOpened = new EventVoid ("OnActionGroupEditorOpened");
		internal static EventVoid OnActionGroupEditorClosed = new EventVoid ("OnActionGroupEditorClosed");

        public int offsetGUIPos = -1;

        private static GUIStyle unchanged;
        private static GUIStyle changed;
        private static GUIStyle greyed;
        private static GUIStyle overfull;
        public static string myToolTip = "";

		static ContainerWindow instance;

        private int counterTT;
        private Vector2 scrollPos;

		bool ActionGroupMode;
		Part selected_part;
		ModuleStorageContainer container_module;

		public static void HideGUI ()
		{
			if (instance != null) {
				instance.container_module = null;
				instance.UpdateGUIState ();
			}
            EditorLogic editor = EditorLogic.fetch;
            if(editor != null)
                editor.Unlock("MSCGUILock");
		}

		public static void ShowGUI (ModuleStorageContainer container_module)
		{
			if (instance != null) {
				instance.container_module = container_module;
				instance.UpdateGUIState ();
			}
		}

		void UpdateGUIState ()
		{
			enabled = container_module != null;
            EditorLogic editor = EditorLogic.fetch;
            if(!enabled &&  editor != null)
                editor.Unlock("MSCGUILock");
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
						HideGUI ();
						OnActionGroupEditorOpened.Fire ();
					}
					var age = EditorActionGroups.Instance;
					if (selected_part) {
						if (!age.SelectionContains (selected_part)) {
							selected_part = null;
							HideGUI ();
						}
					} else if (age.HasSelectedParts ()) {
						selected_part = age.GetSelectedParts ()[0];
						var container = selected_part.FindContainer ();
						if (container) {
							ShowGUI (container);
						}
					}
					ActionGroupMode = true;
				} else {
					if (ActionGroupMode) {
						HideGUI ();
						OnActionGroupEditorClosed.Fire ();
					}
					ActionGroupMode = false;
				}
				yield return null;
			}
		}

		void Awake ()
		{
			enabled = false;
			instance = this;
			StartCoroutine (CheckActionGroupEditor ());
			GameEvents.onEditorLoad.Add (onEditorLoad);
		}

		void OnDestroy ()
		{
			instance = null;
			GameEvents.onEditorLoad.Remove (onEditorLoad);
		}

        private Rect guiWindowRect = new Rect (0, 0, 0, 0);
        private static Vector3 mousePos = Vector3.zero;
        private bool styleSetup = false;
        public void OnGUI ()
        {
            if (!styleSetup)
            {
                styleSetup = true;
                Styles.InitStyles ();
            }

            EditorLogic editor = EditorLogic.fetch;
            if (!HighLogic.LoadedSceneIsEditor || !editor) {
                return;
            }

            //UpdateMixtures ();
            bool cursorInGUI = false; // nicked the locking code from Ferram
            mousePos = Input.mousePosition; //Mouse location; based on Kerbal Engineer Redux code
            mousePos.y = Screen.height - mousePos.y;

            Rect tooltipRect;
            int posMult = 0;
            if (offsetGUIPos != -1) {
                posMult = offsetGUIPos;
			}
            if (ActionGroupMode) {
                if (guiWindowRect.width == 0) {
                    guiWindowRect = new Rect (430 * posMult, 365, 438, (Screen.height - 365));
                }
                tooltipRect = new Rect (guiWindowRect.xMin + 440, mousePos.y-5, 300, 20);
            } else {
                if (guiWindowRect.width == 0) {
                    guiWindowRect = new Rect (Screen.width - 8 - 430 * (posMult+1), 365, 438, (Screen.height - 365));
				}
                tooltipRect = new Rect (guiWindowRect.xMin - (230-8), mousePos.y - 5, 220, 20);
            }
			cursorInGUI = guiWindowRect.Contains (mousePos);
			if (cursorInGUI) {
				editor.Lock (false, false, false, "MSCGUILock");
                if (KSP.UI.Screens.Editor.PartListTooltipMasterController.Instance != null)
				    KSP.UI.Screens.Editor.PartListTooltipMasterController.Instance.HideTooltip ();
			} else {
				editor.Unlock ("MSCGUILock");
			}
            myToolTip = myToolTip.Trim ();
            if (!String.IsNullOrEmpty(myToolTip))
                GUI.Label(tooltipRect, myToolTip, Styles.styleEditorTooltip);
            guiWindowRect = GUILayout.Window (GetInstanceID (), guiWindowRect, GUIWindow, "Containers for " + container_module.part.partInfo.title, Styles.styleEditorPanel);
        }

		void DisplayMass ()
		{
			GUILayout.BeginHorizontal ();
			//GUILayout.Label ("Mass: " + container_module.massDisplay + ", Cost " + container_module.GetModuleCost (0, ModifierStagingSituation.CURRENT).ToString ("N1"));
			GUILayout.EndHorizontal ();
		}

		bool CheckTankList ()
		{
			//if (container_module.tankList.Count == 0) {
			//	GUILayout.BeginHorizontal ();
			//	GUILayout.Label ("This fuel tank cannot hold resources.");
			//	GUILayout.EndHorizontal ();
			//	return false;
			//}
			return true;
		}

        public void GUIWindow (int windowID)
        {
			InitializeStyles ();

			GUILayout.BeginVertical ();
            GUILayout.Space (20);

			if (CheckTankList ()) {
				GUILayout.BeginHorizontal ();
//				if (Math.Round (container_module.AvailableVolume, 4) < 0) {
//					GUILayout.Label ("Volume: " + container_module.volumeDisplay, overfull);
//				} else {
//					GUILayout.Label ("Volume: " + container_module.volumeDisplay);
//				}
				GUILayout.EndHorizontal ();

                DisplayMass();

				scrollPos = GUILayout.BeginScrollView (scrollPos);

				GUILayout.EndScrollView ();
				GUILayout.Label (ModularStorageContainerVersionReport.GetVersion ());
			}
			GUILayout.EndVertical ();

			if (!(myToolTip.Equals ("")) && GUI.tooltip.Equals ("")) {
				if (counterTT > 4) {
					myToolTip = GUI.tooltip;
					counterTT = 0;
				} else {
					counterTT++;
				}
			} else {
				myToolTip = GUI.tooltip;
				counterTT = 0;
			}
			//print ("GT: " + GUI.tooltip);
			GUI.DragWindow ();
        }

        private static void InitializeStyles ()
        {
            if (unchanged == null) {
                if (GUI.skin == null) {
                    unchanged = new GUIStyle ();
                    changed = new GUIStyle ();
                    greyed = new GUIStyle ();
                    overfull = new GUIStyle ();
                } else {
                    unchanged = new GUIStyle (GUI.skin.textField);
                    changed = new GUIStyle (GUI.skin.textField);
                    greyed = new GUIStyle (GUI.skin.textField);
                    overfull = new GUIStyle (GUI.skin.label);
                }

                unchanged.normal.textColor = Color.white;
                unchanged.active.textColor = Color.white;
                unchanged.focused.textColor = Color.white;
                unchanged.hover.textColor = Color.white;

                changed.normal.textColor = Color.yellow;
                changed.active.textColor = Color.yellow;
                changed.focused.textColor = Color.yellow;
                changed.hover.textColor = Color.yellow;

                greyed.normal.textColor = Color.gray;

                overfull.normal.textColor = Color.red;
            }
        }
    }
}
