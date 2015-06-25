using System;
using System.Linq;
using SCANsat.SCAN_Platform;
using SCANsat;
using SCANsat.SCAN_UI.UI_Framework;
using SCANsat.SCAN_Data;
using SCANsat.SCAN_Map;
using palette = SCANsat.SCAN_UI.UI_Framework.SCANpalette;
using UnityEngine;

namespace SCANsat.SCAN_UI
{
	class SCANzoomHiDef : SCANzoomWindow
	{
		protected override void Startup()
		{
			//Initialize the map object
			Visible = false;
			if (HighLogic.LoadedSceneIsFlight)
			{
				v = FlightGlobals.ActiveVessel;
				data = SCANcontroller.controller.getData(v.mainBody.name);
			}

			if (spotmap == null)
			{
				spotmap = new SCANmap(b, false, true);
				spotmap.setSize(320, 240);
			}

			showOrbit = SCANcontroller.controller.map_orbit;
			showAnomaly = SCANcontroller.controller.map_markers;
			showWaypoints = SCANcontroller.controller.map_waypoints;

			TooltipsEnabled = SCANcontroller.controller.toolTips;

			spotmap.setBody(b);
		}

		public override void setMapCenter(double lat, double lon, bool centering, SCANmap big = null)
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

			spotmap.MapScale = 10;

			spotmap.centerAround(lon, lat);
			spotmap.resetMap(mapType.Altimetry, false);
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

			spotmap.resetMap(spotmap.MType, false);
		}

		protected override Texture2D getMap()
		{
			if (highDetail)
				return base.getMap();
			else
			{
				return null;//return SCANuiUtil.drawLoDetailMap(data, MapTexture);
			}
		}

		private Texture2D lowDetailMap()
		{
			return null;
		}
	}
}
