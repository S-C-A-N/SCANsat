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
		[KSPField]
		public bool activeModule = false;

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
		}

		public override string GetInfo()
		{
			string info = base.GetInfo();
			info += "Resource Scan: " + (SCANtype)sensorType + "\n";
			info += "Active Scanner: " + RUIutils.GetYesNoUIString(activeModule) + "\n";

			return info;
		}

		private List<ModuleResourceScanner> findScanner()
		{
			return part.Modules.GetModules<ModuleResourceScanner>();
		}

		private List<ModuleOrbitalSurveyor> findSurvey()
		{
			return part.Modules.GetModules<ModuleOrbitalSurveyor>();
		}

		private ModuleAnimationGroup findAnimator()
		{
			return part.Modules.GetModules<ModuleAnimationGroup>().FirstOrDefault();
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

		public override void startScanAction(KSPActionParam param)
		{
			if (animGroup != null && !scanning && !animGroup.isDeployed)
				animGroup.DeployModule();
			base.startScanAction(param);
		}

		public override void toggleScanAction(KSPActionParam param)
		{
			if (animGroup != null && !scanning && !animGroup.isDeployed)
				animGroup.DeployModule();
			base.toggleScanAction(param);
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
