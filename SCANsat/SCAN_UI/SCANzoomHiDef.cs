using System;
using System.Linq;
using SCANsat;
using SCANsat.SCAN_PartModules;
using SCANsat.SCAN_UI.UI_Framework;
using SCANsat.SCAN_Data;
using SCANsat.SCAN_Map;
using palette = SCANsat.SCAN_UI.UI_Framework.SCANpalette;
using UnityEngine;

namespace SCANsat.SCAN_UI
{
	class SCANzoomHiDef : SCANzoomWindow
	{
		private Color32[] mapPix;
		private float[,] mapValues;
		private SCANhiDefCamera cameraModule;
		private bool mapGenerated;

		private static Rect sessionRectHiDef = defaultRect;

		protected override void Awake()
		{
			base.Awake();

			WindowRect = sessionRectHiDef;
		}

		protected override void Startup()
		{
			//Initialize the map object
			Visible = false;

			v = FlightGlobals.ActiveVessel;
			b = FlightGlobals.ActiveVessel.mainBody;
			data = SCANcontroller.controller.getData(b.name);

			if (spotmap == null)
			{
				spotmap = new SCANmap(b, false, true);
				spotmap.setSize(320, 240);
			}

			showOrbit = SCANcontroller.controller.map_orbit;
			showAnomaly = SCANcontroller.controller.map_markers;
			showWaypoints = SCANcontroller.controller.map_waypoints;
			resourceOverlay = true;

			if (SCANconfigLoader.GlobalResource)
			{
				loadedResources = SCANcontroller.setLoadedResourceList();
				resource = SCANcontroller.getResourceNode(SCANcontroller.controller.resourceSelection);
				if (resource == null)
					resource = SCANcontroller.GetFirstResource;
				resource.CurrentBodyConfig(b.name);
				spotmap.Resource = resource;
			}

			TooltipsEnabled = SCANcontroller.controller.toolTips;

			spotmap.setBody(b);
		}

		protected override void DrawWindowPost(int id)
		{
			sessionRectHiDef = WindowRect;

			if (SCANcontroller.controller.TargetSelecting && Event.current.type == EventType.mouseDown && !TextureRect.Contains(Event.current.mousePosition))
			{
				SCANcontroller.controller.TargetSelecting = false;
				SCANcontroller.controller.TargetSelectingActive = false;
				data.removeTargetWaypoint();
			}

			if (dropDown && Event.current.type == EventType.mouseUp && !ddRect.Contains(Event.current.mousePosition))
				dropDown = false;
		}

		protected override void resetMap(bool checkScanner = false, double lon = 0, double lat = 0, bool withCenter = false)
		{
			mapGenerated = false;

			base.resetMap(false, lon, lat, withCenter);
		}

		public override void setMapCenter(double lat, double lon, bool centering, SCANmap big = null, SCANhiDefCamera camera = null)
		{
			highDetail = centering;
			Visible = true;

			SCANcontroller.controller.TargetSelecting = false;
			SCANcontroller.controller.TargetSelectingActive = false;

			if (v.mainBody != b)
			{
				SCANdata dat = SCANUtil.getData(v.mainBody);
				if (dat == null)
					dat = new SCANdata(v.mainBody);

				data = dat;
				b = data.Body;

				spotmap.setBody(b);
			}

			if (camera != null)
			{
				cameraModule = camera;
				minZoom = camera.minZoom;
				maxZoom = camera.maxZoom;
			}

			spotmap.MapScale = 10;

			spotmap.centerAround(lon, lat);
			spotmap.resetMap(mapType.Altimetry, false, resourceOverlay);

			mapGenerated = false;
		}

		protected override void resyncMap()
		{
			if (v.mainBody != b)
			{
				SCANdata dat = SCANUtil.getData(v.mainBody);
				if (dat == null)
					dat = new SCANdata(v.mainBody);

				data = dat;
				b = data.Body;

				spotmap.setBody(b);
			}

			spotmap.centerAround(SCANUtil.fixLonShift(v.longitude), SCANUtil.fixLatShift(v.latitude));

			spotmap.resetMap(spotmap.MType, false, resourceOverlay);

			mapGenerated = false;
		}

		protected override Texture2D getMap()
		{
			if (highDetail)
				return base.getMap();
			else
			{
				if (mapGenerated)
					return spotmap.Map;
				else
				{
					mapGenerated = true;
					return SCANuiUtil.drawLoDetailMap(ref mapPix, ref mapValues, spotmap, data, spotmap.MapWidth, spotmap.MapHeight, 4, resourceOverlay);
				}
			}
		}

		public override void closeMap()
		{
			base.closeMap();

			if (cameraModule == null)
				return;

			cameraModule.Events["resetCenter"].active = false;
		}
	}
}
