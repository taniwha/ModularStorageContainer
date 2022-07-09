using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.ObjectModel;

using KSP.UI.Screens;
using KodeUI;

namespace ModularStorageContainer
{
	public class ContainerWindow : Window
	{
		ScrollView scrollView;
		UIScrollbar scrollbar;

		public override void CreateUI ()
		{
			base.CreateUI ();
			this.Title (ModularStorageContainerVersionReport.GetVersion ())
				.Vertical ()
				.ControlChildSize (true, true)
				.ChildForceExpand (false, false)
				.PreferredSizeFitter (true, true)
				.Anchor (AnchorPresets.MiddleCenter)
				.Pivot (PivotPresets.TopLeft)
				.PreferredWidth (365)
				.SetSkin ("MSC.Default")

				.Add<ScrollView> (out scrollView)
					.Horizontal (false)
					.Vertical (true)
					.Horizontal ()
					.ControlChildSize (true, true)
					.ChildForceExpand (false, true)
					.FlexibleLayout (true, false)
					.PreferredHeight (400)
					.Add<UIScrollbar> (out scrollbar, "Scrollbar")
						.Direction (Scrollbar.Direction.BottomToTop)
						.PreferredWidth (15)
						.Finish ()
					.Finish ()

				.Finish();

			titlebar
				.Add<UIButton> ()
					.OnClick (CloseWindow)
					.Anchor (AnchorPresets.TopRight)
					.Pivot (new Vector2 (1.25f, 1.25f))
					.SizeDelta (16, 16)
					.Finish();
				;

			scrollView.VerticalScrollbar = scrollbar;
			scrollView.Viewport.FlexibleLayout (true, true);
			scrollView.Content
				.Vertical ()
				.ControlChildSize (true, true)
				.ChildForceExpand (false, false)
				.Anchor (AnchorPresets.HorStretchTop)
				.PreferredSizeFitter (true, false)
				.SizeDelta (0, 0)
				.Finish ();
		}

		public override void Style ()
		{
			base.Style ();
		}

		void CloseWindow ()
		{
			MSCWindowManager.HideGUI ();
		}

		void ClearContent ()
		{
			var rect = scrollView.Content.rectTransform;
			for (int i = rect.childCount; i--> 0; ) {
				var go = rect.GetChild (i).gameObject;
				UnityEngine.Object.Destroy (go);
			}
		}

		ModuleStorageContainer container_module;
		ModuleStorageContainer []symmetry_container_modules;
		IStorageContainer [][]symmetry_containers;

		void RebuildContainers (ModuleStorageContainer msc_container)
		{
			container_module = msc_container;

			ClearContent ();

			Part p = container_module.part;
			int contCount = msc_container.containers.Count;
			int symCount = p.symmetryCounterparts.Count;
			symmetry_container_modules = new ModuleStorageContainer[symCount];
			symmetry_containers = new IStorageContainer[contCount][];
			for (int j = 0; j < contCount; j++) {
				symmetry_containers[j] = new IStorageContainer[symCount];
				for (int i = 0; i < symCount; i++) {
					Part sym = p.symmetryCounterparts[i];
					var cont = sym.FindContainer ();
					symmetry_container_modules[i] = cont;
					symmetry_containers[j][i] = cont.containers[j];
				}
				var container = msc_container.containers[j];
				container.CreateUI (scrollView.Content, symmetry_containers[j]);
			}
		}

		public void SetContainer (ModuleStorageContainer container)
		{
			if (container == null) {
				gameObject.SetActive (false);
			} else {
				gameObject.SetActive (true);
				RebuildContainers (container);
			}
		}
	}
}
