/* 
 *  [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 *
 * SCANsat - SCAN RADAR Altimetry Sensor part (& More)
 * 
 * Copyright (c)2013 damny;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
 */
using System;
using System.Collections.Generic;
using System.Linq; //Needed for the OrderBy() method for data transmission
using UnityEngine;

namespace SCANsat
{
	public class SCANsat : PartModule, IScienceDataContainer
	{
		protected SCANcontroller con = SCANcontroller.controller;
		protected bool powerIsProblem;  //initialized = false, *Unused now
		protected Animation anim = null;
		protected List<ScienceData> storedData = new List<ScienceData> ();
		protected ExperimentsResultDialog expDialog = null;

		/* SAT: KSP entry points */
		public override void OnStart ( StartState state )
        {
			if (state == StartState.Editor)
            {
				print ("[SCANsat] start: in editor");
				Events["editorExtend"].active = !string.IsNullOrEmpty(animationName);
            } else
            {
				print ("[SCANsat] start: live");
            }
			if (animationName != null)
            {
				Animation[] a = part.FindModelAnimators (animationName);
				if (a.Length == 0)
                {
					print ("[SCANsat] animation '" + animationName + "' not found");
                } else
                {
					print ("[SCANsat] using animation #1 out of " + a.Length.ToString () + " animations named '" + animationName + "'");
					anim = a [0];
					// maybe use this later for advanced animation...
					Transform modeltransform = part.transform.FindChild ("model");
					foreach (Transform t in modeltransform.GetComponentsInChildren<Transform>())
                    {
						//print("[SCANsat] transform " + t.name + ": " + t);
                    }
                }
            }
			print ("[SCANsat] sensorType: " + sensorType.ToString () + " fov: " + fov.ToString () + " min_alt: " + min_alt.ToString () + " max_alt: " + max_alt.ToString () + " best_alt: " + best_alt.ToString () + " power: " + power.ToString ());
        }
		public override void OnUpdate () {
            //** No need to include these references to the deprecated slope scanner part
            //if (sensorType < 0) {
            //    Events ["startScan"].active = false;
            //    Events ["stopScan"].active = false;
            //    Events ["analyze"].active = false;
            //    Events ["explode"].active = true;
            //    Events ["explode"].guiActive = true;
            //    return;
            //}
            Events ["reviewEvent"].active = storedData.Count > 0;
            Events ["EVACollect"].active = storedData.Count > 0;
			Events ["startScan"].active = !scanning;
			Events ["stopScan"].active = scanning;
            //Events ["analyze"].active = scanning; //** No reason to make sciene analysis dependent on scanning
			if (scanning) { //&& sensorType >= 0) { //** All extant scanners have sensorType >= 0
				if (sensorType == 0 || SCANcontroller.controller.isVesselKnown (vessel.id , (SCANdata.SCANtype)sensorType)) {
					if (TimeWarp.CurrentRate < 1500) { // would need large buffer batteries, just not very smooth
						float p = power * TimeWarp.deltaTime;
						float e = part.RequestResource ("ElectricCharge" , p);
						if (e < p) {
							unregisterScanner (); //** I'm thinking a dedicated start/stop method should be used for these
							powerIsProblem = true;
						} else {
							registerScanner ();
							powerIsProblem = false;
						}
					} else if (powerIsProblem) {
						registerScanner ();
						powerIsProblem = false;
					}
				} else {
					unregisterScanner ();
				}
			}
			SCANcontroller.controller.scanFromAllVessels ();
			if (vessel == FlightGlobals.ActiveVessel) {
				SCANui.gui_ping (powerIsProblem);
				if (powerIsProblem) {
					addStatic ();
					registerScanner ();
				} else if (sensorType == 0 && scanning) {
					SCANui.gui_ping_maptraq ();
				}
			}
		}
        public override void OnInitialize() //** Maybe this should all just go at the end of OnStart?
        {
            if (sensorType == 0)
            {
                Events["startScan"].guiName = "Open Map";
                Events["stopScan"].guiName = "Close Map";
                Actions["startScanAction"].guiName = "Open Map";
                Actions["stopScanAction"].guiName = "Close Map";
                Actions["toggleScanAction"].guiName = "Toggle Map";
                Events["analyze"].active = false;
            }
            else if (scanName != null)
            {
                Events["startScan"].guiName = "Start " + scanName;
                Events["stopScan"].guiName = "Stop " + scanName;
                Actions["startScanAction"].guiName = "Start " + scanName;
                Actions["stopScanAction"].guiName = "Stop " + scanName;
                Actions["toggleScanAction"].guiName = "Toggle " + scanName;
            }
            if (scanning) startScan();
        }
        //** These are what I use for saving and loading science data
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
        }
        public override void OnSave(ConfigNode node)
        {
            node.RemoveNodes("ScienceData"); //** Prevent duplicates
            foreach (ScienceData SCANData in storedData)
            {
                ConfigNode storedDataNode = node.AddNode("ScienceData");
                SCANData.Save(storedDataNode);
            }
        }
		public override string GetInfo () {
			string str = base.GetInfo ();
			if (min_alt != 0) {
			str += "Altitude ( min): " + (min_alt / 1000).ToString ("F0") + " km\n"; }
			if (best_alt != min_alt) {
			str += "Altitude (best): " + (best_alt / 1000).ToString ("F0") + " km\n"; }
			if (max_alt != 0) {
			str += "Altitude ( max): " + (max_alt / 1000).ToString ("F0") + " km\n"; }
			if (fov != 0) {
			str += "FOV: " + fov.ToString("F0") + " °\n"; }
			str += "Power usage: " + power.ToString ("F1") + " charge/s\n";
			return str;
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

		/* SCAN: all of these fields and only scanning is persistant */
		[KSPField(isPersistant = true)]
		protected bool scanning = false;

		public bool scanningNow () { return scanning; }

		/* SCAN: context (right click) buttons in FLIGHT */
		[KSPEvent(guiActive = true, guiName = "Start RADAR Scan", active = true)]
		public void startScan () {
			if (!ToolbarManager.ToolbarAvailable)
				SCANui.minimode = (SCANui.minimode > 0 ? 2 : -SCANui.minimode);
            registerScanner ();
            //scanning = true;
            //if (sensorType > 0) {
            //    SCANcontroller.controller.registerSensor (vessel , (SCANdata.SCANtype)sensorType , fov , min_alt , max_alt , best_alt);
            //}
			animate (1);
		}
		[KSPEvent(guiActive = true, guiName = "Stop RADAR Scan", active = true)]
		public void stopScan () {
            unregisterScanner ();
            //scanning = false;
            //if (sensorType > 0) {
            //    SCANcontroller.controller.unregisterSensor (vessel , (SCANdata.SCANtype)sensorType);
            //}
			animate (-1);
		}
        //** No need to include these references to the deprecated slope scanner part
        //[KSPEvent(guiActive = false, guiName = "Trigger Explosive Charge", active = false)]
        //public void explode () {
        //    part.explode ();
        //}
		[KSPEvent(guiActive = true, guiName = "Analyze Data", active = true)] //** Always active
		public void analyze () {
			makeScienceData (true);
			ReviewData ();
		}
        //** Allows us to review stored science data
        [KSPEvent(guiActive = true, guiName = "Review Data", active = false)]
        public void reviewEvent() {
            ReviewData();
        }
        //** EVA data collection
        [KSPEvent(guiActiveUnfocused = true, guiName = "Collect Stored Data", externalToEVAOnly = true, unfocusedRange = 1.5f, active = false)]
        public void EVACollect()
        {
            List<ModuleScienceContainer> EVACont = FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleScienceContainer>();
            if (storedData.Count > 0) {
                if (EVACont.First().StoreData(new List<IScienceDataContainer>() { this }, false)) {  
                    DumpData(storedData[0]);
                }
            }
        }


		/* SCAN: context (right click) buttons in EDTIOR */
		[KSPEvent(guiActiveEditor = true, guiName = "Extend", active = true)]
		public void editorExtend () {
			Events ["editorExtend"].active = false;
			Events ["editorRetract"].active = true;
			animate (1);
		}
		[KSPEvent(guiActiveEditor = true, guiName = "Retract", active = false)]
		public void editorRetract () {
			Events ["editorExtend"].active = true;
			Events ["editorRetract"].active = false;
			animate (-1);
		}

		/* SCAN: trivial function to do animation */
		public void animate ( float speed ) {
			if (anim != null && anim [animationName] != null) {
				anim [animationName].speed = speed;
				if (anim.IsPlaying (animationName)) {
					if (anim [animationName].normalizedTime <= 0) {
						anim [animationName].normalizedTime = 0;
					} else if (anim [animationName].normalizedTime >= 1 - float.Epsilon) {
						anim [animationName].normalizedTime = 1;
					}
				} else {
					anim [animationName].wrapMode = WrapMode.ClampForever;
					anim.Play (animationName);
				}
			}
		}

		/* SCAN: actions for ... something ... */
		[KSPAction("Start RADAR Scan")]
		public void startScanAction ( KSPActionParam param ) {
			startScan ();
		}
		[KSPAction("Stop RADAR Scan")]
		public void stopScanAction ( KSPActionParam param ) {
			stopScan ();
		}
		[KSPAction("Toggle RADAR Scan")]
		public void toggleScanAction ( KSPActionParam param ) {
			if (scanning)
				stopScan ();
			else
				startScan ();
		}
		[KSPAction("Analyze Data")]
		public void analyzeData ( KSPActionParam param ) {
            //if (scanning) ** Always available
				analyze ();
        }

		/* SCAN: add static (a warning that we're low on electric charge) */
		public void addStatic () {
			SCANdata data = SCANcontroller.controller.getData (vessel.mainBody.bodyName);
			Texture2D map = data.map_small;
			if (map != null) {
				for (int i=0; i<1000; ++i) {
					map.SetPixel (UnityEngine.Random.Range (0 , 360) , UnityEngine.Random.Range (0 , 180) , Color.Lerp (Color.black , Color.white , UnityEngine.Random.value));
				}
			}
		}

        //** Register scanners without going through animations too
        public void registerScanner() {
            scanning = true;
            if (sensorType > 0) 
                SCANcontroller.controller.registerSensor(vessel, (SCANdata.SCANtype)sensorType, fov, min_alt, max_alt, best_alt);
        }
        public void unregisterScanner() {
            scanning = false;
            if (sensorType > 0) 
                SCANcontroller.controller.unregisterSensor (vessel , (SCANdata.SCANtype)sensorType);
        }


		/* SCAN: SCIENCE! make, store, transmit, keep
		 * 	discard, review, count DATA */
		public void makeScienceData ( bool notZero ) {
			if (expDialog != null)
				DestroyImmediate (expDialog);
			storedData.Clear ();
			ScienceData sd = SCANcontroller.controller.getAvailableScience (vessel , (SCANdata.SCANtype)sensorType , notZero);
			if (sd == null)
				return;
			storedData.Add (sd);
		}
		public ScienceData[] GetData () {
			return storedData.ToArray ();
		}
		public void KeepData ( ScienceData data ) {
			print ("[SCANsat] keeping data");
			expDialog = null;
		}
        //** This method has issues, nothing happens if all transmitters are busy
        //public void TransmitData ( ScienceData data ) {
        //    print ("[SCANsat] transmitting data");
        //    expDialog = null;
        //    if (!storedData.Contains (data))
        //        return;
        //    foreach (IScienceDataTransmitter t in vessel.FindPartModulesImplementing<IScienceDataTransmitter>()) {
        //        if (t.CanTransmit ()) {
        //            if (!t.IsBusy ()) {
        //                makeScienceData (false); // just to update values...
        //                t.TransmitData (storedData);
        //                storedData = new List<ScienceData> ();
        //                break;
        //            }
        //        }
        //    }
        //}
        
        //** This is closer to the stock method of data transmission
        public void TransmitData(ScienceData data) {
            print("[SCANsat] transmitting data");
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
		public void DumpData ( ScienceData data ) {
			print ("[SCANsat] dumping data");
			expDialog = null;
			while (storedData.Contains(data)) {
				storedData.Remove (data);
			}
		}
		public void ReviewDataItem ( ScienceData sd ) {
            //expDialog = ExperimentsResultDialog.DisplayResult (new ExperimentResultDialogPage (part , sd , 1f , 0f , false , "" , true , false , DumpData , KeepData , TransmitData , null));
            ReviewData(); //** Just go directly to the regular method: fairly certain this is never used anyway
        }
		public void ReviewData () {
			if (storedData.Count < 1)
				return;
			if (expDialog != null)
				DestroyImmediate (expDialog);
			ScienceData sd = storedData [0];
			expDialog = ExperimentsResultDialog.DisplayResult (new ExperimentResultDialogPage (part , sd , 1f , 0f , false , "" , true , false , DumpData , KeepData , TransmitData , null));
		}
		public bool IsRerunnable () {
			return true; //** true? I don't think it matters, but might gum up other science tracking mods
		}
		public int GetScienceCount () {
			return storedData.Count;
		}
	}
}

