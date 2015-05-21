#region license
/*  [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 *
 * SCANresourceScanner - Resource scanner part module
 * 
 * Copyright (c)2013 damny;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 DMagic
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
 */
#endregion

using System.Collections.Generic;
using System.Linq;
using SCANsat.SCAN_Data;

namespace SCANsat
{
	public class ModuleSCANresourceScanner : SCANsat, IAnimatedModule
	{
		private List<ModuleOrbitalSurveyor> mSurvey;
		private List<ModuleResourceScanner> mScanner;
		private ModuleAnimationGroup animGroup;

		public override void OnStart(PartModule.StartState state)
		{
			base.OnStart(state);

			mSurvey = findSurvey();
			mScanner = findScanner();
			animGroup = findAnimator();

			if (animGroup == null)
				this.isEnabled = true;

			Actions["startScanAction"].active = false;
			Actions["stopScanAction"].active = false;
			Actions["toggleScanAction"].active = false;
			Actions["startResourceScanAction"].guiName = "Start " + scanName;
			Actions["stopResourceScanAction"].guiName = "Stop " + scanName;
			Actions["toggleResourceScanAction"].guiName = "Toggle " + scanName;
		}

		public override string GetInfo()
		{
			string info = base.GetInfo();
			info += "Resource Scan: " + (SCANtype)sensorType + "\n";

			return info;
		}

		private List<ModuleResourceScanner> findScanner()
		{
			return part.FindModulesImplementing<ModuleResourceScanner>();
		}

		private List<ModuleOrbitalSurveyor> findSurvey()
		{
			return part.FindModulesImplementing<ModuleOrbitalSurveyor>();
		}

		private ModuleAnimationGroup findAnimator()
		{
			return part.FindModulesImplementing<ModuleAnimationGroup>().FirstOrDefault();
		}

		private void updateEvents()
		{
			base.Events["startScan"].active = !scanning;
			base.Events["stopScan"].active = scanning;
		}

		public override void OnUpdate()
		{
			base.OnUpdate();

			if (!HighLogic.LoadedSceneIsFlight)
				return;

			if (!FlightGlobals.ready)
				return;

			if (SCANcontroller.controller == null)
				return;

			if (!SCANcontroller.controller.easyModeScanning || SCANcontroller.controller.disableStockResource)
				updateEvents();
			else
			{
				base.Events["startScan"].active = false;
				base.Events["stopScan"].active = false;
				if (scanning)
					unregisterScanner();
			}
		}

		[KSPAction("Start Resource Scan")]
		public void startResourceScanAction(KSPActionParam param)
		{
			if (animGroup != null && !scanning && !animGroup.isDeployed)
				animGroup.DeployModule();
			startScan();
		}

		[KSPAction("Stop Resource Scan")]
		public void stopResourceScanAction(KSPActionParam param)
		{
			stopScan();
		}

		[KSPAction("Toggle Resource Scan")]
		public void toggleResourceScanAction(KSPActionParam param)
		{
			if (scanning)
				stopScan();
			else
			{
				if (animGroup != null && !animGroup.isDeployed)
					animGroup.DeployModule();
				startScan();
			}
		}

		public void DisableModule()
		{
			this.isEnabled = false;
			unregisterScanner();
			if (mSurvey != null && SCANcontroller.controller.disableStockResource)
			{
				foreach (ModuleOrbitalSurveyor m in mSurvey)
				{
					m.DisableModule();
				}
			}
		}

		public void EnableModule()
		{
			this.isEnabled = true;
			if (mSurvey != null && SCANcontroller.controller.disableStockResource)
			{
				foreach (ModuleOrbitalSurveyor m in mSurvey)
				{
					m.DisableModule();
				}
			}
		}

		public bool IsSituationValid()
		{
			return true;
		}

		public bool ModuleIsActive()
		{
			return isEnabled;
		}
	}
}
