using System;
using System.Collections.Generic;
using System.Linq;
using SCANsat.SCAN_Data;


namespace SCANsat.SCAN_PartModules
{
	class SCANresourceDisplay : ModuleResourceScanner, IAnimatedModule
	{
		[KSPField]
		public int sensorType;

		private List<ModuleResourceScanner> stockScanners;
		private bool tooHigh;
		private bool fuzzy;

		public override void OnStart(PartModule.StartState state)
		{
			if (state == StartState.Editor)
				return;

			this.enabled = true;

			stockScanners = findScanners();

			setupFields(stockScanners.FirstOrDefault());

			if (stockScanners != null && SCANcontroller.controller.disableStockResource)
			{
				foreach (ModuleResourceScanner m in stockScanners)
				{
					m.DisableModule();
				}
			}
		}

		private List<ModuleResourceScanner> findScanners()
		{
			return part.FindModulesImplementing<ModuleResourceScanner>().Where(r => r.ScannerType == 0 && r.ResourceName == ResourceName).ToList();
		}

		private void setupFields(ModuleResourceScanner m)
		{
			if (m != null)
			{
				MaxAbundanceAltitude = m.MaxAbundanceAltitude;
				RequiresUnlock = m.RequiresUnlock;
			}
			else
			{
				MaxAbundanceAltitude = 250000;
				RequiresUnlock = true;
			}
		}

		public override void OnUpdate()
		{
			if (!HighLogic.LoadedSceneIsFlight || !FlightGlobals.ready)
				return;

			if (!SCANcontroller.controller.disableStockResource)
			{
				Fields["abundanceDisplay"].guiActive = false;
				return;
			}

			Fields["abundanceDisplay"].guiActive = true;

			if (tooHigh)
				abundanceDisplay = string.Format("{0}[Surf]: Too High", ResourceName);
			else if (abundanceValue < 0)
				abundanceDisplay = string.Format("{0}[Surf]: No Data", ResourceName);
			else if (fuzzy)
				abundanceDisplay = string.Format("{0}[Surf]: {1:P0}", ResourceName, abundanceValue);
			else
				abundanceDisplay = string.Format("{0}[Surf]: {1:P2}", ResourceName, abundanceValue);
		}

		public override void OnFixedUpdate()
		{
			if (vessel.altitude > MaxAbundanceAltitude)
			{
				tooHigh = true;
				return;
			}

			tooHigh = false;
			double lat = SCANUtil.fixLatShift(vessel.latitude);
			double lon = SCANUtil.fixLonShift(vessel.longitude);
			if (SCANUtil.isCovered(lon, lat, vessel.mainBody, sensorType))
			{
				abundanceValue = SCANUtil.ResourceOverlay(lat, lon, ResourceName, vessel.mainBody, RequiresUnlock && SCANcontroller.controller.resourceBiomeLock);
				fuzzy = false;
			}
			else if (SCANUtil.isCovered(lon, lat, vessel.mainBody, 524288))
			{
				abundanceValue = SCANUtil.ResourceOverlay(lat, lon, ResourceName, vessel.mainBody, RequiresUnlock && SCANcontroller.controller.resourceBiomeLock);
				fuzzy = true;
			}
			else
			{
				abundanceValue = -1;
			}
		}

		void IAnimatedModule.EnableModule()
		{
			this.enabled = true;
			if (stockScanners != null && SCANcontroller.controller.disableStockResource)
			{
				foreach (ModuleResourceScanner m in stockScanners)
				{
					m.DisableModule();
				}
			}
		}

		void IAnimatedModule.DisableModule()
		{
			this.enabled = false;
			if (stockScanners != null && SCANcontroller.controller.disableStockResource)
			{
				foreach (ModuleResourceScanner m in stockScanners)
				{
					m.DisableModule();
				}
			}
		}

		bool IAnimatedModule.ModuleIsActive()
		{
			return isEnabled;
		}

		bool IAnimatedModule.IsSituationValid()
		{
			return true;
		}
	}
}
