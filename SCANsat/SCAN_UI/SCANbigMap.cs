

using System;
using System.Collections.Generic;
using System.Linq;
using SCANsat.Platform;
using SCANsat;
using UnityEngine;

using palette = SCANsat.SCANpalette;

namespace SCANsat.SCAN_UI
{
	class SCANbigMap: MBW
	{
		private SCANmap bigmap, spotmap;
		private Vessel v;
		private CelestialBody b;

		protected override void Awake()
		{
			WindowCaption = string.Format("Map of {0}", b.theName);
			WindowRect = new Rect(250, 55, 380, 180);
			WindowOptions = new GUILayoutOption[2] { GUILayout.Width(360), GUILayout.Height(180) };
			WindowStyle = SCANskins.SCAN_window;
			Visible = true;
			DragEnabled = true;

			SCAN_SkinsLibrary.SetCurrent("SCAN_Unity");
		}

		internal override void OnDestroy()
		{
			
		}

		protected override void DrawWindowPre(int id)
		{
			
		}

		protected override void DrawWindow(int id)
		{

		}

		protected override void DrawWindowPost(int id)
		{
			
		}

	}
}
