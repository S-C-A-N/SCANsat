#region license
/*  [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 *
 * SCANsat - SCAN RADAR Altimetry Sensor part (& More)
 * 
 * Copyright (c)2013 damny;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
 */
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using SCANsat.SCAN_UI;
using UnityEngine;
using palette = SCANsat.SCAN_UI.SCANpalette;

namespace SCANsat
{
	public class SCANsat : PartModule, IScienceDataContainer
	{
		protected SCANcontroller con = SCANcontroller.controller;
		protected bool powerIsProblem;
		protected Animation anim = null;
		protected List<ScienceData> storedData = new List<ScienceData>();
		protected ExperimentsResultDialog expDialog = null;

		/* SAT: KSP entry points */
		public override void OnStart(StartState state)
		{
			GameEvents.onVesselSOIChanged.Add(SOIChange);
			if (state == StartState.Editor)
			{
				print("[SCANsat] start: in editor");
				Events["editorExtend"].active = !string.IsNullOrEmpty(animationName);
			}
			else
			{
				print("[SCANsat] start: live");
			}
			if (!string.IsNullOrEmpty(animationName))
			{
				Animation[] a = part.FindModelAnimators(animationName);
				if (a.Length == 0)
				{
					print("[SCANsat] animation '" + animationName + "' not found");
				}
				else
				{
					print("[SCANsat] using animation #1 out of " + a.Length.ToString() + " animations named '" + animationName + "'");
					anim = a[0];
				}
			}
			if (scanName != null)
			{ // Use bitwise operators to check if the part has valid science collection scanners
				if ((sensorType & (Int32)SCANdata.SCANtype.AltimetryLoRes) == 0 && (sensorType & (Int32)SCANdata.SCANtype.AltimetryHiRes) == 0 && (sensorType & (Int32)SCANdata.SCANtype.Biome) == 0)
				{
					Events["startScan"].guiName = "Start " + scanName;
					Events["stopScan"].guiName = "Stop " + scanName;
					Events["analyze"].active = false;
					Actions["startScanAction"].guiName = "Start " + scanName;
					Actions["stopScanAction"].guiName = "Stop " + scanName;
					Actions["toggleScanAction"].guiName = "Toggle " + scanName;
					Actions["analyzeData"].active = false;
				}
				else
				{
					Events["startScan"].guiName = "Start " + scanName;
					Events["stopScan"].guiName = "Stop " + scanName;
					Events["analyze"].active = true;
					Actions["startScanAction"].guiName = "Start " + scanName;
					Actions["stopScanAction"].guiName = "Stop " + scanName;
					Actions["toggleScanAction"].guiName = "Toggle " + scanName;
				}
			}

			if (sensorType == 0)
			{
				// here, we override all event and action labels
				// and we also disable the analyze button (it does nothing)
				Events["startScan"].active = false;
				Events["stopScan"].active = false;
				Events["analyze"].active = false;
				Events["editorExtend"].active = false;
				Actions["startScanAction"].active = false;
				Actions["stopScanAction"].active = false;
				Actions["toggleScanAction"].active = false;
				Actions["analyzeData"].active = false;
			}
			else if (sensorType == 32)
			{
				// here, we only disable analyze; BTDT has good labels
				Events["analyze"].active = false;
				Actions["analyzeData"].active = false;
			}
			if (scanning) animate(1, 1);
			powerIsProblem = false;
			print("[SCANsat] sensorType: " + sensorType.ToString() + " fov: " + fov.ToString() + " min_alt: " + min_alt.ToString() + " max_alt: " + max_alt.ToString() + " best_alt: " + best_alt.ToString() + " power: " + power.ToString());
		}

		public override void OnUpdate()
		{
			if (sensorType != 0)
			{
				Events["reviewEvent"].active = storedData.Count > 0;
				Events["EVACollect"].active = storedData.Count > 0;
				Events["startScan"].active = !scanning;
				Events["stopScan"].active = scanning;
				if (sensorType != 32)
					Fields["alt_indicator"].guiActive = scanning;
				if (scanning)
				{
					if (sensorType == 0 || SCANcontroller.controller.isVesselKnown(vessel.id, (SCANdata.SCANtype)sensorType))
					{
						if (TimeWarp.CurrentRate < 1500)
						{
							float p = power * TimeWarp.deltaTime;
							float e = part.RequestResource("ElectricCharge", p);
							if (e < p)
							{
								unregisterScanner();
								powerIsProblem = true;
							}
							else
							{
								registerScanner();
								powerIsProblem = false;
							}
						}
						else if (powerIsProblem)
						{
							registerScanner();
							powerIsProblem = false;
						}
					}
					else
						unregisterScanner();
					alt_indicator = scanAlt();
				}
				//SCANcontroller.controller.scanFromAllVessels ();
				if (vessel == FlightGlobals.ActiveVessel)
				{
					//SCANui.gui_ping(powerIsProblem);
					if (powerIsProblem)
					{
						addStatic();
						registerScanner();
						//} else if (sensorType == 0 && scanning) {
						//    SCANui.gui_ping_maptraq ();
					}
				}
			}
		}

		public override void OnLoad(ConfigNode node)
		{
			if (node.HasNode("ScienceData"))
			{
				foreach (ConfigNode storedDataNode in node.GetNodes("ScienceData"))
				{
					ScienceData data = new ScienceData(storedDataNode);
					storedData.Add(data);
				}
			}
			if (node.HasNode("SCANsatRPM"))
			{
				ConfigNode RPMPersistence = node.GetNode("SCANsatRPM");
				foreach (ConfigNode RPMNode in RPMPersistence.GetNodes("Prop"))
				{
					string id = RPMNode.GetValue("Prop ID");
					int Mode = Convert.ToInt32(RPMNode.GetValue("Mode"));
					int Color = Convert.ToInt32(RPMNode.GetValue("Color"));
					int Zoom = Convert.ToInt32(RPMNode.GetValue("Zoom"));
					bool Lines = Convert.ToBoolean(RPMNode.GetValue("Lines"));
					RPMList.Add(new RPMPersistence(id, Mode, Color, Zoom, Lines));
				}
			}
		}

		public override void OnSave(ConfigNode node)
		{
			node.RemoveNodes("ScienceData"); //** Prevent duplicates
			foreach (ScienceData SCANData in storedData)
			{
				ConfigNode storedDataNode = node.AddNode("ScienceData");
				SCANData.Save(storedDataNode);
			}
			if (RPMList.Count > 0)
			{
				ConfigNode RPMPersistence = new ConfigNode("SCANsatRPM");
				foreach (RPMPersistence RPMMFD in RPMList)
				{
					ConfigNode RPMProp = new ConfigNode("Prop");
					RPMProp.AddValue("Prop ID", RPMMFD.RPMID);
					RPMProp.AddValue("Mode", RPMMFD.RPMMode);
					RPMProp.AddValue("Color", RPMMFD.RPMColor);
					RPMProp.AddValue("Zoom", RPMMFD.RPMZoom);
					RPMProp.AddValue("Lines", RPMMFD.RPMLines);
					RPMPersistence.AddNode(RPMProp);
				}
				node.AddNode(RPMPersistence);
			}
		}

		public override string GetInfo()
		{
			string str = base.GetInfo();
			if (min_alt != 0)
			{
				str += "Altitude ( min): " + (min_alt / 1000).ToString("F0") + " km\n";
			}
			if (best_alt != min_alt)
			{
				str += "Altitude (best): " + (best_alt / 1000).ToString("F0") + " km\n";
			}
			if (max_alt != 0)
			{
				str += "Altitude ( max): " + (max_alt / 1000).ToString("F0") + " km\n";
			}
			if (fov != 0)
			{
				str += "FOV: " + fov.ToString("F0") + " °\n";
			}
			str += "Power usage: " + power.ToString("F1") + " charge/s\n";
			return str;
		}

		private void OnDestroy()
		{
			GameEvents.onVesselSOIChanged.Remove(SOIChange);
		}

		private void SOIChange(GameEvents.HostedFromToAction<Vessel, CelestialBody> VC)
		{
			if (scanning)
				if (!SCANcontroller.controller.Body_Data.ContainsKey(VC.to.name))
					SCANcontroller.controller.Body_Data.Add(VC.to.name, new SCANdata(VC.to));
		}

		/* SAT: KSP fields */
		[KSPField]
		public int sensorType;
		[KSPField]
		public float fov;
		[KSPField]
		public float min_alt;
		[KSPField]
		public float max_alt;
		[KSPField]
		public float best_alt;
		[KSPField]
		public float power;
		[KSPField]
		public string scanName;
		[KSPField]
		public string animationName;
		[KSPField(guiName = "SCANsat Altitude", guiActive = false)]
		public string alt_indicator;
		internal List<RPMPersistence> RPMList = new List<RPMPersistence>();

		/* SCAN: all of these fields and only scanning is persistant */
		[KSPField(isPersistant = true)]
		protected bool scanning = false;
		public bool scanningNow() { return scanning; }

		/* SCAN: context (right click) buttons in FLIGHT */
		[KSPEvent(guiActive = true, guiName = "Start RADAR Scan", active = true)]
		public void startScan()
		{
			if (!ToolbarManager.ToolbarAvailable)
				SCANcontroller.controller.mainMap.Visible = true;
#if DEBUG
			//SCANui.minimode = (SCANui.minimode > 0 ? 2 : -SCANui.minimode);
#endif
			if (!SCANcontroller.controller.Body_Data.ContainsKey(vessel.mainBody.name))
				SCANcontroller.controller.Body_Data.Add(vessel.mainBody.name, new SCANdata(vessel.mainBody));
			registerScanner();
			animate(1, 0);
		}

		[KSPEvent(guiActive = true, guiName = "Stop RADAR Scan", active = true)]
		public void stopScan()
		{
			unregisterScanner();
			powerIsProblem = false;
			animate(-1, 1);
		}

		[KSPEvent(guiActive = true, guiName = "Analyze Data", active = true)]
		public void analyze()
		{
			makeScienceData(true);
			ReviewData();
		}

		[KSPEvent(guiActive = true, guiName = "Review Data", active = false)]
		public void reviewEvent()
		{
			ReviewData();
		}

		[KSPEvent(guiActiveUnfocused = true, guiName = "Collect Stored Data", externalToEVAOnly = true, unfocusedRange = 1.5f, active = false)]
		public void EVACollect()
		{
			List<ModuleScienceContainer> EVACont = FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleScienceContainer>();
			if (storedData.Count > 0)
			{
				if (EVACont.First().StoreData(new List<IScienceDataContainer>() { this }, false))
				{
					foreach (ScienceData data in storedData)
						DumpData(data);
				}
			}
		}

		/* SCAN: context (right click) buttons in EDTIOR */
		[KSPEvent(guiActiveEditor = true, guiName = "Extend", active = true)]
		public void editorExtend()
		{
			Events["editorExtend"].active = false;
			Events["editorRetract"].active = true;
			animate(1, 0);
		}

		[KSPEvent(guiActiveEditor = true, guiName = "Retract", active = false)]
		public void editorRetract()
		{
			Events["editorExtend"].active = true;
			Events["editorRetract"].active = false;
			animate(-1, 1);
		}

		/* SCAN: trivial function to do animation */
		public void animate(float speed, float time)
		{
			if (anim != null && anim[animationName] != null)
			{
				anim[animationName].speed = speed;
				if (anim.IsPlaying(animationName))
				{
					if (anim[animationName].normalizedTime <= 0)
					{
						anim[animationName].normalizedTime = time;
					}
					else if (anim[animationName].normalizedTime >= 1 - float.Epsilon)
					{
						anim[animationName].normalizedTime = time;
					}
				}
				else
				{
					anim[animationName].wrapMode = WrapMode.ClampForever;
					anim[animationName].normalizedTime = time;
					anim.Play(animationName);
				}
			}
		}

		/* SCAN: actions for ... something ... */
		[KSPAction("Start Scan")]
		public void startScanAction(KSPActionParam param)
		{
			startScan();
		}

		[KSPAction("Stop Scan")]
		public void stopScanAction(KSPActionParam param)
		{
			stopScan();
		}

		[KSPAction("Toggle Scan")]
		public void toggleScanAction(KSPActionParam param)
		{
			if (scanning)
				stopScan();
			else
				startScan();
		}

		[KSPAction("Analyze Data")]
		public void analyzeData(KSPActionParam param)
		{
			//if (scanning) ** Always available
			analyze();
		}

		/* SCAN: add static (a warning that we're low on electric charge) */
		public void addStatic()
		{
			SCANdata data = SCANUtil.getData(vessel.mainBody);
			Texture2D map = data.Map;
			if (map != null)
			{
				for (int i = 0; i < 1000; ++i)
				{
					map.SetPixel(UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 180), palette.lerp(palette.black, palette.white, UnityEngine.Random.value));
				}
			}
		}

		/* SCAN: register scanners without going through animation */
		public void registerScanner()
		{
			scanning = true;
			if (sensorType > 0)
				SCANcontroller.controller.registerSensor(vessel, (SCANdata.SCANtype)sensorType, fov, min_alt, max_alt, best_alt);
		}

		public void unregisterScanner()
		{
			scanning = false;
			if (sensorType > 0)
				SCANcontroller.controller.unregisterSensor(vessel, (SCANdata.SCANtype)sensorType);
		}

		private string scanAlt()
		{
			string altitude = "Unknown";
			if (vessel.altitude < min_alt)
				altitude = "Too low";
			else if (vessel.altitude < best_alt)
				altitude = "Sub-optimal";
			else if (vessel.altitude > best_alt && vessel.altitude < max_alt)
				altitude = "Ideal";
			else if (vessel.altitude > max_alt)
				altitude = "Too high";
			return altitude;
		}

		/* SCAN: SCIENCE! make, store, transmit, keep
		 * 	discard, review, count DATA */
		public void makeScienceData(bool notZero)
		{
			if (expDialog != null)
				DestroyImmediate(expDialog);
			storedData.Clear();
			ScienceData sd = SCANUtil.getAvailableScience(vessel, (SCANdata.SCANtype)sensorType, notZero);
			if (sd == null)
				return;
			storedData.Add(sd);
		}

		public ScienceData[] GetData()
		{
			return storedData.ToArray();
		}

		public void KeepData(ScienceData data)
		{
			expDialog = null;
		}

		public void TransmitData(ScienceData data)
		{
			expDialog = null;
			List<IScienceDataTransmitter> tranList = vessel.FindPartModulesImplementing<IScienceDataTransmitter>();
			if (tranList.Count > 0 && storedData.Count > 0)
			{
				makeScienceData(false);
				tranList.OrderBy(ScienceUtil.GetTransmitterScore).First().TransmitData(storedData);
				DumpData(storedData[0]);
			}
			else ScreenMessages.PostScreenMessage("No transmitters available on this vessel.", 4f, ScreenMessageStyle.UPPER_LEFT);
		}

		public void DumpData(ScienceData data)
		{
			expDialog = null;
			while (storedData.Contains(data))
			{
				storedData.Remove(data);
			}
		}

		public void ReviewDataItem(ScienceData sd)
		{
			ReviewData();
		}

		public void ReviewData()
		{
			if (storedData.Count < 1)
				return;
			if (expDialog != null)
				DestroyImmediate(expDialog);
			ScienceData sd = storedData[0];
			expDialog = ExperimentsResultDialog.DisplayResult(new ExperimentResultDialogPage(part, sd, 1f, 0f, false, "", true, false, DumpData, KeepData, TransmitData, null));
		}

		public bool IsRerunnable()
		{
			return true;
		}

		public int GetScienceCount()
		{
			return storedData.Count;
		}
	}
}

