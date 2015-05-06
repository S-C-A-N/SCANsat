using System;
using System.Collections.Generic;
using System.Linq;
using SCANsat.SCAN_Data;


namespace SCANsat
{
	public class ModuleSCANresourceScanner : SCANsat, IAnimatedModule
	{
		[KSPField]
		public bool activeModule = false;
		[KSPField]
		public bool forceActive = false;

		private ModuleOrbitalSurveyor mSurvey;
		private ModuleResourceScanner mScanner;

		public override void OnStart(PartModule.StartState state)
		{
			base.OnStart(state);

			mSurvey = findSurvey();
			mScanner = findScanner();

			if (!forceActive)
				this.isEnabled = false;
			else
				this.isEnabled = true;
		}

		public override string GetInfo()
		{
			string info = base.GetInfo();
			info += "Resource Scan: " + (SCANtype)sensorType + "\n";
			info += "Active Scanner: " + activeModule + "\n";

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
			base.Events["startScan"].active = !scanning;
			base.Events["stopScan"].active = scanning;
		}

		public override void OnUpdate()
		{
			if (activeModule)
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
					if (scanning)
						unregisterScanner();
				}
			}
			else
			{
				base.Events["startScan"].active = false;
				base.Events["stopScan"].active = false;
			}
		}

		public void DisableModule()
		{
			if (!forceActive)
			{
				this.isEnabled = false;
				unregisterScanner();
			}
		}

		public void EnableModule()
		{
			if (!forceActive)
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
