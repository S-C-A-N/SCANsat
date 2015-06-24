using System;
using System.Collections.Generic;
using System.Linq;
using SCANsat.SCAN_UI;

namespace SCANsat.SCAN_PartModules
{
	public class SCANhiDefCamera : PartModule
	{
		[KSPField]
		public bool hiDetailOnly;

		public override void OnStart(PartModule.StartState state)
		{
			base.OnStart(state);

			Events["toggleSCANHiDef"].guiName = "Toggle Map";
			Events["resetCenter"].guiName = "Reset Map Center";
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
				SCANcontroller.controller.hiDefMap.setMapCenter(SCANUtil.fixLatShift(vessel.latitude), SCANUtil.fixLonShift(vessel.longitude), hiDetailOnly || SCANcontroller.controller.hiDetailZoomMap);
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

			SCANcontroller.controller.hiDefMap.setMapCenter(SCANUtil.fixLatShift(vessel.latitude), SCANUtil.fixLonShift(vessel.longitude), hiDetailOnly || SCANcontroller.controller.hiDetailZoomMap);
		}
	}
}
