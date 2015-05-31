using System;
using System.Collections.Generic;
using System.Linq;
using SCANsat.SCAN_Data;


namespace SCANsat
{
	public class ModuleSCANresourceScanner : SCANsat, IAnimatedModule
	{
		//private List<ModuleOrbitalSurveyor> mSurvey;
		//private List<ModuleResourceScanner> mScanner;
		private ModuleAnimationGroup animGroup;

		public override void OnStart(PartModule.StartState state)
		{
			base.OnStart(state);

			//mSurvey = findSurvey();
			//mScanner = findScanner();
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
			Actions["startResourceScanAction"].active = true;
			Actions["stopResourceScanAction"].active = true;
			Actions["toggleResourceScanAction"].active = true;
		}

		public override void OnUpdate()
		{
			base.OnUpdate();

			if (!HighLogic.LoadedSceneIsFlight)
				return;

			if (SCANcontroller.controller == null)
				return;

			if (!SCANcontroller.controller.easyModeScanning)
				updateEvents();
			else
			{
				base.Events["startScan"].active = false;
				base.Events["stopScan"].active = false;
				Actions["startResourceScanAction"].active = false;
				Actions["stopResourceScanAction"].active = false;
				Actions["toggleResourceScanAction"].active = false;
				if (scanning)
					unregisterScanner();
			}
		}

		[KSPAction("Start Resource Scan")]
		public void startResourceScanAction(KSPActionParam param)
		{
			SCANUtil.SCANlog("Start Scan");
			if (animGroup != null && !scanning && !animGroup.isDeployed)
				animGroup.DeployModule();
			startScan();
		}

		[KSPAction("Stop Resource Scan")]
		public void stopResourceScanAction(KSPActionParam param)
		{
			SCANUtil.SCANlog("Stop Scan");
			stopScan();
		}

		[KSPAction("Toggle Resource Scan")]
		public void toggleResourceScanAction(KSPActionParam param)
		{
			SCANUtil.SCANlog("Toggle Scan");
			if (scanning)
				stopScan();
			else
				startScan();
		}

		public void DisableModule()
		{
			this.isEnabled = false;
			base.Events["startScan"].active = false;
			base.Events["stopScan"].active = false;
			unregisterScanner();
		}

		public void EnableModule()
		{
			this.isEnabled = true;
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
