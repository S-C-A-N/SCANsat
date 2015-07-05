#region license
/*  [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 *
 * SCANresourceDisplay - Resource abundance display
 * 
 * Copyright (c)2013 damny;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 DMagic
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using SCANsat.SCAN_Data;

namespace SCANsat.SCAN_PartModules
{
	public class SCANresourceDisplay : PartModule, IAnimatedModule
	{
		[KSPField]
		public int sensorType;
		[KSPField(guiActive = true, guiName = "Abundance")]
		public string abundanceField;
		[KSPField]
		public string ResourceName;
		[KSPField]
		public float MaxAbundanceAltitude;
		[KSPField]
		public bool RequiresUnlock;

		private float abundanceValue;

		private List<ModuleResourceScanner> stockScanners;
		private ModuleAnimationGroup animGroup;
		private Dictionary<string, ResourceCache.AbundanceSummary> abundanceSummary;
		private CelestialBody body;
		private bool tooHigh;
		private bool fuzzy;
		private bool refreshState;
		private bool activated;

		public override void OnStart(PartModule.StartState state)
		{
			if (state == StartState.Editor)
				return;

			GameEvents.onVesselSOIChanged.Add(onSOIChange);

			part.force_activate();
			this.isEnabled = true;
			activated = true;
			refreshState = true;

			stockScanners = findScanners();
			animGroup = findAnimator();

			if (animGroup == null || animGroup.isDeployed)
				enableConnectedScanners();

			setupFields(stockScanners.FirstOrDefault());

			body = FlightGlobals.currentMainBody;
			refreshAbundance(body.flightGlobalsIndex);
		}

		private List<ModuleResourceScanner> findScanners()
		{
			return part.FindModulesImplementing<ModuleResourceScanner>().Where(r => r.ScannerType == 0 && r.ResourceName == ResourceName).ToList();
		}

		private ModuleAnimationGroup findAnimator()
		{
			return part.FindModulesImplementing<ModuleAnimationGroup>().FirstOrDefault();
		}

		private void setupFields(ModuleResourceScanner m)
		{
			if (m != null)
			{
				SCANUtil.SCANlog("{0} Resource Display Module set to Max Alt: {1} ; Unlock: {2}", m.ResourceName, m.MaxAbundanceAltitude, m.RequiresUnlock);
				MaxAbundanceAltitude = m.MaxAbundanceAltitude;
				RequiresUnlock = m.RequiresUnlock;
			}
			else
			{
				MaxAbundanceAltitude = 250000;
				RequiresUnlock = true;
			}

			Fields["abundanceField"].guiName = string.Format("{0}[Surf]", ResourceName);
		}

		private void OnDestroy()
		{
			GameEvents.onVesselSOIChanged.Remove(onSOIChange);
		}

		private void Update()
		{
			if (!activated)
			{
				Fields["abundanceField"].guiActive = false;
				return;
			}

			if (!HighLogic.LoadedSceneIsFlight || !FlightGlobals.ready)
				return;

			if (SCANcontroller.controller == null)
				return;

			if (refreshState)
			{
				if (SCANcontroller.controller.disableStockResource)
					disableConnectedScanners();
				refreshState = false;
			}

			if (!SCANcontroller.controller.disableStockResource)
			{
				Fields["abundanceField"].guiActive = false;
				return;
			}

			Fields["abundanceField"].guiActive = true;

			if (tooHigh)
			{
				abundanceField = "Too High";
				return;
			}
			else if (abundanceValue < 0)
			{
				abundanceField = "No Data";
				return;
			}

			string biome = "Landed";

			if (body.BiomeMap != null)
				biome = SCANUtil.getBiomeName(body, SCANUtil.fixLonShift(vessel.longitude), SCANUtil.fixLatShift(vessel.latitude));

			if (checkBiome(biome) || !SCANcontroller.controller.resourceBiomeLock)
			{
				if (fuzzy)
					abundanceField = abundanceValue.ToString("P0");
				else
					abundanceField = abundanceValue.ToString("P2");
			}
			else
			{
				float biomeAbundance = abundanceSummary.ContainsKey(biome) ? abundanceSummary[biome].Abundance : 0f;
				if (fuzzy)
					abundanceField = biomeAbundance.ToString("P0") + "avg.";
				else
					abundanceField = biomeAbundance.ToString("P2") + "avg.";
			}
		}

		private bool checkBiome (string b)
		{
			return ResourceMap.Instance.IsBiomeUnlocked(body.flightGlobalsIndex, b);
		}

		private void FixedUpdate()
		{
			if (!activated)
			{
				abundanceValue = -1f;
				return;
			}

			if (ResourceUtilities.GetAltitude(vessel) > MaxAbundanceAltitude)
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

		private void onSOIChange(GameEvents.HostedFromToAction<Vessel, CelestialBody> VB)
		{
			body = VB.to;
			refreshAbundance(body.flightGlobalsIndex);
		}

		private void refreshAbundance(int bodyID)
		{
			abundanceSummary = new Dictionary<string, ResourceCache.AbundanceSummary>();

			abundanceSummary = ResourceCache.Instance.AbundanceCache.
				Where(a => a.ResourceName == ResourceName && a.HarvestType == HarvestTypes.Planetary && a.BodyId == bodyID).
				GroupBy(a => a.BiomeName).
				ToDictionary(b => b.Key, b => b.First());
		}

		private void disableConnectedScanners()
		{
			if (stockScanners != null)
			{
				foreach (ModuleResourceScanner m in stockScanners)
				{
					m.DisableModule();
				}
			}
		}

		private void enableConnectedScanners()
		{
			if (stockScanners != null)
			{
				foreach (ModuleResourceScanner m in stockScanners)
				{
					m.EnableModule();
				}
			}
		}

		public void EnableModule()
		{
			activated = true;
			if (SCANcontroller.controller != null && SCANcontroller.controller.disableStockResource)
				disableConnectedScanners();
		}

		public void DisableModule()
		{
			activated = false;
			if (SCANcontroller.controller != null && SCANcontroller.controller.disableStockResource)
				disableConnectedScanners();
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
