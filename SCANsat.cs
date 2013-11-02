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

	[KSPField]
	public float power;

	[KSPField(isPersistant = true)]
	protected bool scanning = false;

	[KSPEvent(guiActive = true, guiName = "Start RADAR Scan", active = true)]
	public void startScan() {
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

	[KSPEvent(guiActive = false, guiName = "Trigger Explosive Charge", active = false)]
	public void explode() {
		part.explode();
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
		if(sensorType < 0) {
			Events["startScan"].active = false;
			Events["stopScan"].active = false;
			Events["explode"].active = true;
			Events["explode"].guiActive = true;
			return;
		}
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
		if(scanning) {
			float p = power * TimeWarp.deltaTime;
			float e = part.RequestResource("ElectricCharge", p);
			if(e < p) {
				stopScan();
				scanning = true;
			} else {
				startScan();
			}
		}
		SCANcontroller.controller.scanFromAllVessels();
		if(scanning && vessel == FlightGlobals.ActiveVessel) SCANui.gui_ping();
	}

	public override string GetInfo() {
		string str = base.GetInfo();
		str += "Power usage: " + power.ToString("F1") + "/s\n";
		return str;
	}
}
