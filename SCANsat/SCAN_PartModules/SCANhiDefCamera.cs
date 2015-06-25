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

		public override void OnStart(PartModule.StartState state)
		{
			base.OnStart(state);

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

		[KSPEvent(guiActive = true, active = true)]
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
				foreach (ModuleHighDefCamera m in stockCameras)
				{
					m.DisableModule();
				}
			}
		}

		public void EnableModule()
		{
			isEnabled = true;
			if (SCANcontroller.controller != null && SCANcontroller.controller.disableStockResource)
				disableConnectedModules();
		}

		public void DisableModule()
		{
			isEnabled = false;
			if (SCANcontroller.controller != null && SCANcontroller.controller.disableStockResource)
				disableConnectedModules();
		}

		public bool ModuleIsActive()
		{
			return isEnabled;
		}

		public bool IsSituationValid()
		{
			return true;
		}

	}
}
