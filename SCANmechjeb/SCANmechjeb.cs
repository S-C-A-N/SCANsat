#region license
/* 
 * [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 *
 * SCANmechjeb - A monobehaviour for watching SCANsat and MechJeb for the addition of a landing target
 * 
 * Copyright (c)2014 David Grandy <david.grandy@gmail.com>;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
 */
#endregion

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

			if (!SCANcontroller.controller.mechJebTargetSelection)
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

			if (v.mainBody != SCANcontroller.controller.LandingTargetBody)
				SCANcontroller.controller.LandingTargetBody = v.mainBody;

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

			if (SCANcontroller.controller.LandingTarget != null)
			{
				way = SCANcontroller.controller.LandingTarget;
			}

			if (SCANcontroller.controller.TargetSelecting)
			{
				way = null;
				selectingTarget = true;
				if (SCANcontroller.controller.TargetSelectingActive)
					selectingInMap = true;
				else
					selectingInMap = false;
				coords = SCANcontroller.controller.LandingTargetCoords;
				return;
			}
			else if (selectingTarget)
			{
				selectingTarget = false;
				if (selectingInMap)
				{
					selectingInMap = false;
					coords = SCANcontroller.controller.LandingTargetCoords;
					way = new SCANwaypoint(coords.y, coords.x, siteName);
					target.SetPositionTarget(SCANcontroller.controller.LandingTargetBody, way.Latitude, way.Longitude);
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
					SCANcontroller.controller.LandingTarget = way;
					data.addToWaypoints();
				}
			}
			else
			{
				way = new SCANwaypoint(coords.y, coords.x, siteName);
				SCANcontroller.controller.LandingTarget = way;
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

			GLUtils.DrawMapViewGroundMarker(SCANcontroller.controller.LandingTargetBody, coords.y, coords.x, palette.mechjebYellow);
		}
	}
}
