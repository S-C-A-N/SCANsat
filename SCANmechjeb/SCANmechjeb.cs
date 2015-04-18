

using System;
using System.Collections.Generic;
using System.Linq;
using SCANsat;
using SCANsat.SCAN_Data;
using SCANsat.SCAN_Platform;
using log = SCANsat.SCAN_Platform.Logging.ConsoleLogger;
using palette = SCANsat.SCAN_UI.UI_Framework.SCANpalette;
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
		private DisplayModule guidanceModule;
		private SCANwaypoint way;
		private SCANdata data;
		private Vector2d coords = new Vector2d();
		private bool selectingTarget, selectingInMap, shutdown;

		protected override void LateUpdate()
		{
			if (shutdown)
				return;

			if (!HighLogic.LoadedSceneIsFlight || !FlightGlobals.ready)
				return;

			if (SCANcontroller.controller == null)
			{
				way = null;
				return;
			}

			v = FlightGlobals.ActiveVessel;

			if (v == null)
			{
				SCANcontroller.controller.MechJebLoaded = false;
				way = null;
				return;
			}

			if (v.mainBody != SCANcontroller.controller.MechJebTargetBody)
				SCANcontroller.controller.MechJebTargetBody = v.mainBody;

			data = SCANUtil.getData(v.mainBody);

			if (data == null)
			{
				SCANcontroller.controller.MechJebLoaded = false;
				way = null;
				return;
			}

			if (v.FindPartModulesImplementing<MechJebCore>().Count <= 0)
			{
				SCANcontroller.controller.MechJebLoaded = false;
				way = null;
				return;
			}

			core = v.GetMasterMechJeb();

			if (core == null)
			{
				SCANcontroller.controller.MechJebLoaded = false;
				way = null;
				return;
			}

			if (HighLogic.CurrentGame.Mode != Game.Modes.SANDBOX)
			{
				if (guidanceModule == null)
					guidanceModule = (DisplayModule)core.GetComputerModule("MechJebModuleLandingGuidance");

				if (guidanceModule == null)
				{
					SCANcontroller.controller.MechJebLoaded = false;
					way = null;
					return;
				}

				if (!guidanceModule.unlockChecked)
					return;

				if (guidanceModule.hidden)
				{
					SCANcontroller.controller.MechJebLoaded = false;
					shutdown = true;
					way = null;
					return;
				}
			}

			target = core.target;

			if (target == null)
			{
				SCANcontroller.controller.MechJebLoaded = false;
				way = null;
				return;
			}

			if (!SCANcontroller.controller.MechJebLoaded)
			{
				SCANcontroller.controller.MechJebLoaded = true;
				RenderingManager.AddToPostDrawQueue(1, drawTarget);
			}

			if (SCANcontroller.controller.MechJebTarget != null)
			{
				way = SCANcontroller.controller.MechJebTarget;
			}

			if (SCANcontroller.controller.MechJebSelecting)
			{
				way = null;
				selectingTarget = true;
				if (SCANcontroller.controller.MechJebSelectingActive)
					selectingInMap = true;
				else
					selectingInMap = false;
				coords = SCANcontroller.controller.MechJebTargetCoords;
				return;
			}
			else if (selectingTarget)
			{
				selectingTarget = false;
				if (selectingInMap)
				{
					selectingInMap = false;
					coords = SCANcontroller.controller.MechJebTargetCoords;
					way = new SCANwaypoint(coords.y, coords.x, siteName);
					target.SetPositionTarget(SCANcontroller.controller.MechJebTargetBody, way.Latitude, way.Longitude);
				}
			}

			selectingInMap = false;
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
			if (!selectingInMap)
				return;

			target.pickingPositionTarget = false;

			if (!MapView.MapIsEnabled)
				return;
			if (!v.isActiveVessel || v.GetMasterMechJeb() != core)
				return;

			GLUtils.DrawMapViewGroundMarker(SCANcontroller.controller.MechJebTargetBody, coords.y, coords.x, palette.mechjebYellow);
		}
	}
}
