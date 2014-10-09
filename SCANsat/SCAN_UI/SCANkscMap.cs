

using System;
using System.Collections.Generic;
using System.Linq;
using SCANsat.Platform;
using SCANsat;
using UnityEngine;

namespace SCANsat.SCAN_UI
{
	class SCANkscMap: MBW
	{
		private SCANmap bigmap; //, spotmap;
		private CelestialBody b;
		private SCANdata data;
		//private double startUT;
		private bool overlay_static_dirty; //, notMappingToday, bigmap_dragging;
		private Texture2D overlay_static;
		//private Rect rc = new Rect(0, 0, 0, 0);
		//private Rect pos_spotmap = new Rect(10f, 10f, 10f, 10f);
		//private Rect pos_spotmap_x = new Rect(10f, 10f, 25f, 25f);

		protected override void Awake()
		{
			WindowCaption = string.Format("Map of {0}", b.theName);
			WindowRect = new Rect(250, 120, 740, 420);
			WindowOptions = new GUILayoutOption[2] { GUILayout.Width(740), GUILayout.Height(420) };
			WindowStyle = SCANskins.SCAN_window;
			Visible = true;
			DragEnabled = true;

			SCAN_SkinsLibrary.SetCurrent("SCAN_Unity");
		}

		internal override void Start()
		{
			b = Planetarium.fetch.Home;
			if (bigmap == null)
			{
				bigmap = new SCANmap();
				bigmap.setProjection((SCANmap.MapProjection)SCANcontroller.controller.projection);
				bigmap.setWidth(720);
				WindowRect.x = SCANcontroller.controller.map_x;
				WindowRect.y = SCANcontroller.controller.map_y;
			}
			else
			{
				SCANcontroller.controller.map_x = (int)WindowRect.x;
				SCANcontroller.controller.map_y = (int)WindowRect.y;
			}
			bigmap.setBody(b);
		}

		internal override void OnDestroy()
		{
			
		}

		protected override void DrawWindowPre(int id)
		{
			data = SCANUtil.getData(b);
		}

		protected override void DrawWindow(int id)
		{
			growS();

			Texture2D map = bigmap.getPartialMap(); 

			Rect maprect = GUILayoutUtility.GetLastRect();
			maprect.width = bigmap.mapwidth;
			maprect.height = bigmap.mapheight;

			if (overlay_static == null)
			{
				overlay_static = new Texture2D((int)bigmap.mapwidth, (int)bigmap.mapheight, TextureFormat.ARGB32, false);
				overlay_static_dirty = true;
			}

			if (overlay_static_dirty)
			{
				SCANuiUtil.clearTexture(overlay_static);
				if (SCANcontroller.controller.map_grid)
				{
					SCANuiUtil.drawGrid(maprect, bigmap, overlay_static);
				}
				overlay_static.Apply();
				overlay_static_dirty = false;
			}

			stopS();
		}

	}
}
