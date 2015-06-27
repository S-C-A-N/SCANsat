using System;
using System.Collections.Generic;
using System.Linq;
using SCANsat.SCAN_UI;
using UnityEngine;

namespace SCANsat.SCAN_PartModules
{
	public class SCANhiDefCamera : PartModule, IAnimatedModule
	{
		[KSPField]
		public float minZoom = 2;
		[KSPField]
		public float maxZoom = 1000;
		[KSPField]
		public bool hiDetailOnly;

		private List<ModuleHighDefCamera> stockCameras;

		private bool activated;
		private bool refreshState;

		public override void OnStart(PartModule.StartState state)
		{
			if (state == StartState.Editor)
				return;

			SCANUtil.SCANdebugLog("Starting Hi Def Module");

			part.force_activate();
			this.isEnabled = true;
			activated = true;
			refreshState = true;

			stockCameras = findCameras();

			minZoom = clampValue(minZoom, 1, 10);
			maxZoom = clampValue(maxZoom, 10, 1000);

			Events["toggleSCANHiDef"].guiName = "Toggle Map";
			Events["resetCenter"].guiName = "Reset Map Center";
		}

		private List<ModuleHighDefCamera> findCameras()
		{
			return part.FindModulesImplementing<ModuleHighDefCamera>().ToList();
		}

		private float clampValue (float value, float min, float max)
		{
			return Mathf.Clamp(value, min, max);
		}

		private void Update()
		{
			if (!activated)
			{
				Events["toggleSCANHiDef"].active = false;
				return;
			}

			if (!HighLogic.LoadedSceneIsFlight || !FlightGlobals.ready)
				return;

			if (SCANcontroller.controller == null)
				return;

			if (refreshState)
			{
				if (SCANcontroller.controller.disableStockResource)
					disableConnectedModules();
				refreshState = false;
			}

			if (!SCANcontroller.controller.disableStockResource)
			{
				Events["toggleSCANHiDef"].active = false;
				return;
			}

			Events["toggleSCANHiDef"].active = true;
		}

		[KSPEvent(guiActive = true, active = false)]
		public void toggleSCANHiDef()
		{
			if (SCANcontroller.controller.hiDefMap == null)
			{
				SCANcontroller.controller.hiDefMap = gameObject.AddComponent<SCANzoomHiDef>();
			}

			if (!SCANcontroller.controller.hiDefMap.Visible)
			{
				SCANcontroller.controller.hiDefMap.setMapCenter(SCANUtil.fixLatShift(vessel.latitude), SCANUtil.fixLonShift(vessel.longitude), hiDetailOnly || SCANcontroller.controller.hiDetailZoomMap, null, this);
			}
			else
				SCANcontroller.controller.hiDefMap.Visible = false;

			Events["resetCenter"].active = SCANcontroller.controller.hiDefMap.Visible;
		}

		[KSPEvent(guiActive = true, active = false)]
		public void resetCenter()
		{
			if (SCANcontroller.controller.hiDefMap == null)
				return;

			SCANcontroller.controller.hiDefMap.setMapCenter(SCANUtil.fixLatShift(vessel.latitude), SCANUtil.fixLonShift(vessel.longitude), hiDetailOnly || SCANcontroller.controller.hiDetailZoomMap, null, this);
		}

		private void enableConnectedModules()
		{
			if (stockCameras != null)
			{
				SCANUtil.SCANdebugLog("Enabling Connected Hi Defs");
				foreach (ModuleHighDefCamera m in stockCameras)
				{
					m.EnableModule();
				}
			}
		}

		private void disableConnectedModules()
		{
			if (stockCameras != null)
			{
				SCANUtil.SCANdebugLog("Disabling Connected Hi Defs");
				foreach (ModuleHighDefCamera m in stockCameras)
				{
					m.DisableModule();
				}
			}
		}

		public void EnableModule()
		{
			SCANUtil.SCANdebugLog("Enable Hi Def");

			activated = true;
			if (SCANcontroller.controller != null && SCANcontroller.controller.disableStockResource)
				disableConnectedModules();
		}

		public void DisableModule()
		{
			SCANUtil.SCANdebugLog("Disable Hi Def");

			activated = false;
			if (SCANcontroller.controller != null && SCANcontroller.controller.disableStockResource)
				disableConnectedModules();
		}

		public bool ModuleIsActive()
		{
			return activated;
		}

		public bool IsSituationValid()
		{
			return true;
		}

	}
}
