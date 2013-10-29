/* 
 * Scientific Committee on Advanced Navigation S.C.A.N. Satellite
 * SCANsat - SCAN RADAR Altimetry Sensor part
 * 
 * Copyright (c)2013 damny; see LICENSE.txt for licensing details.
 */

using System;
using System.Collections.Generic;
using UnityEngine;

public class SCANsat : PartModule
{
	protected SCANcontroller con = SCANcontroller.controller;
	protected bool initialized = false;

	public override void OnStart(StartState state) {
		if(state == StartState.Editor) {
			print("[SCANsat] start: in editor");
		} else {
			print("[SCANsat] start: live");
		}
	}

	[KSPField]
	public int sensorType;

	[KSPField(isPersistant = true)]
	protected bool scanning = false;

	[KSPEvent(guiActive = true, guiName = "Start RADAR Scan", active = true)]
	public void startScan() {
		print("Starting RADAR scan, sensor type = " + sensorType.ToString());
		scanning = true;
		if(sensorType > 0) {
			SCANcontroller.controller.registerVesselID(vessel.id, (SCANdata.SCANtype)sensorType);
		}
	}

	[KSPEvent(guiActive = true, guiName = "Stop RADAR Scan", active = true)]
	public void stopScan() {
		scanning = false;
		if(sensorType > 0) {
			SCANcontroller.controller.unregisterVesselID(vessel.id, (SCANdata.SCANtype)sensorType);
		}
	}

	[KSPAction("Start RADAR Scan")]
	public void startScanAction(KSPActionParam param) {
		startScan();
	}

	[KSPAction("Stop RADAR Scan")]
	public void stopScanAction(KSPActionParam param) {
		stopScan();
	}

	[KSPAction("Toggle RADAR Scan")]
	public void toggleScanAction(KSPActionParam param) {
		if(scanning) stopScan();
		else startScan();
	}

	public override void OnUpdate() {
		if(!initialized) {
			if(sensorType == 0) {
				Events["startScan"].guiName = "Open Map";
				Events["stopScan"].guiName = "Close Map";
			}
			if(scanning) {
				startScan();
			}
			initialized = true;
		}
		Events["startScan"].active = !scanning;
		Events["stopScan"].active = scanning;
		SCANcontroller.controller.scanFromAllVessels();
		if(scanning && vessel == FlightGlobals.ActiveVessel) SCANui.gui_ping();
	}
}
