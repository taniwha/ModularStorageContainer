using System;
using UnityEngine;

namespace ModularStorageContainer {

	public class ScrollView
	{
		public delegate void ScrollFunc ();
		public ScrollFunc Begin { get; private set; }
		public ScrollFunc End { get; private set; }

		Rect rect;
		public Vector2 scroll;
		public bool mouseOver { get; private set; }
		GUILayoutOption width;
		GUILayoutOption height;
		GUILayoutOption sbWidth;

		public ScrollView (int width, int height)
		{
			this.width = GUILayout.Width (width);
			this.height = GUILayout.Height (height);
			this.sbWidth = GUILayout.Width (15);

			Begin = BeginWidthHeight;
			End = EndWidthHeight;
		}

		public ScrollView (int height)
		{
			this.height = GUILayout.Height (height);

			Begin = BeginHeight;
			End = EndHeight;
		}

		void BeginWidthHeight ()
		{
			scroll = GUILayout.BeginScrollView (scroll, width, height);
			GUILayout.BeginHorizontal ();
			GUILayout.BeginVertical ();
		}

		void BeginHeight ()
		{
			scroll = GUILayout.BeginScrollView (scroll, height);
			GUILayout.BeginVertical ();
		}

		void EndWidthHeight ()
		{
			GUILayout.EndVertical ();
			GUILayout.Label ("", sbWidth);
			GUILayout.EndHorizontal ();
			GUILayout.EndScrollView ();
			if (Event.current.type == EventType.Repaint) {
				rect = GUILayoutUtility.GetLastRect();
				mouseOver = rect.Contains(Event.current.mousePosition);
			}
		}

		void EndHeight ()
		{
			GUILayout.EndVertical ();
			GUILayout.EndScrollView ();
			if (Event.current.type == EventType.Repaint) {
				rect = GUILayoutUtility.GetLastRect();
				mouseOver = rect.Contains(Event.current.mousePosition);
			}
		}

		public void Reset ()
		{
			scroll = Vector2.zero;
		}
	}

}
