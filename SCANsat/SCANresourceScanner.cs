using System;
using System.Collections.Generic;
using System.Linq;
using SCANsat.SCAN_Data;


namespace SCANsat
{
	public class ModuleSCANresourceScanner : SCANsat, IAnimatedModule
	{
		[KSPField]
		public bool scanner = false;

		private ModuleOrbitalSurveyor mSurvey;
		private ModuleResourceScanner mScanner;

		public override void OnStart(PartModule.StartState state)
		{
			base.OnStart(state);

			mSurvey = findSurvey();
			mScanner = findScanner();
		}

		public override string GetInfo()
		{
			string info = base.GetInfo();
			info += "Resource Scan: " + ((SCANtype)sensorType).ToString() + "\n";

			return info;
		}

		private ModuleResourceScanner findScanner()
		{
			ModuleResourceScanner r = vessel.FindPartModulesImplementing<ModuleResourceScanner>().FirstOrDefault();
			return r;
		}

		private ModuleOrbitalSurveyor findSurvey()
		{
			ModuleOrbitalSurveyor s = vessel.FindPartModulesImplementing<ModuleOrbitalSurveyor>().FirstOrDefault();
			return s;
		}

		private void updateEvents()
		{
			base.Events["startScan"].active = !scanning && isEnabled;
			base.Events["stopScan"].active = scanning && isEnabled;
		}

		private void Update()
		{
			if (!HighLogic.LoadedSceneIsFlight)
				return;

			if (SCANcontroller.controller == null)
				return;

			if (!SCANcontroller.controller.easyModeScanning)
				updateEvents();
		}

		public void DisableModule()
		{
			this.isEnabled = false;
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
