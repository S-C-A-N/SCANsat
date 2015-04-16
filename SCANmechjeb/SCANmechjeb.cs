

using System;
using System.Collections.Generic;
using System.Linq;
using SCANsat;
using SCANsat.SCAN_Data;
using SCANsat.SCAN_Platform;
using MuMech;

using UnityEngine;

namespace SCANmechjeb
{
	class SCANmechjeb : SCAN_MBE
	{
		private const string siteName = "MechJeb Landing Target";
		private Vessel v;
		private MechJebCore core;
		private MechJebModuleTargetController target;
		private SCANwaypoint way;
		private SCANdata data;
		private Vector2d coords = new Vector2d();
		private bool selectingTarget;

		protected override void LateUpdate()
		{
			if (SCANcontroller.controller == null)
			{
				way = null;
				return;
			}

			if (!SCANcontroller.controller.MechJebLoaded)
			{
				way = null;
				return;
			}

			if (SCANcontroller.controller.MechJebTarget != null)
			{
				way = SCANcontroller.controller.MechJebTarget;
			}

			v = FlightGlobals.ActiveVessel;

			if (v == null)
			{
				way = null;
				return;
			}

			data = SCANUtil.getData(v.mainBody);

			if (data == null)
			{
				way = null;
				return;
			}

			if (v.FindPartModulesImplementing<MechJebCore>().Count <= 0)
			{
				way = null;
				return;
			}

			core = v.GetMasterMechJeb();

			if (core == null)
			{
				way = null;
				return;
			}

			target = core.target;

			if (target == null)
			{
				way = null;
				return;
			}

			if (SCANcontroller.controller.MechJebSelecting)
			{
				way = null;
				selectingTarget = true;
				coords = SCANcontroller.controller.MechJebTargetCoords;
				drawTarget();
				return;
			}
			else if (selectingTarget)
			{
				selectingTarget = false;
				way = new SCANwaypoint(SCANcontroller.controller.MechJebTargetCoords.y, SCANcontroller.controller.MechJebTargetCoords.x, siteName);
				coords = SCANcontroller.controller.MechJebTargetCoords;
				target.SetPositionTarget(SCANcontroller.controller.MechJebTargetBody, way.Latitude, way.Longitude);
			}

			selectingTarget = false;

			if (target.Target == null)
			{
				way = null;
				return;
			}

			if (target.targetBody != v.mainBody)
			{
				way = null;
				return;
			}

			if (!(target.Target is PositionTarget))
			{
				way = null;
				return;
			}

			coords.x = target.targetLongitude;
			coords.y = target.targetLatitude;

			if (way != null)
			{
				if (!SCANUtil.ApproxEq(coords.x, way.Longitude) || !SCANUtil.ApproxEq(coords.y, way.Latitude))
				{
					way = new SCANwaypoint(coords.y, coords.x, siteName);
					SCANcontroller.controller.MechJebTarget = way;
					data.addToWaypoints();
				}
			}
			else
			{
				way = new SCANwaypoint(coords.y, coords.x, siteName);
				SCANcontroller.controller.MechJebTarget = way;
				data.addToWaypoints();
			}
		}

		//Draw the mapview MechJeb target arrows
		private void drawTarget()
		{
			if (SCANcontroller.controller.MechJebSelectingActive)
			{
				target.pickingPositionTarget = false;

				if (!MapView.MapIsEnabled)
					return;
				if (MapView.fetch.scaledVessel.GetOrbit().referenceBody != SCANcontroller.controller.MechJebTargetBody)
					return;
				if (!v.isActiveVessel || v.GetMasterMechJeb() != core)
					return;

				if (target.Target == null)
					return;
				if (!(target.Target is PositionTarget))
					return;

				GLUtils.DrawMapViewGroundMarker(SCANcontroller.controller.MechJebTargetBody, coords.y, coords.x, Color.red);
			}
		}
	}
}
