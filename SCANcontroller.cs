/* 
 * Scientific Committee on Advanced Navigation S.C.A.N. Satellite
 * SCANcontroller - scenario module that handles all scanning
 * 
 * Copyright (c)2013 damny; see LICENSE.txt for licensing details.
 */

using System;
using System.Collections.Generic;
using UnityEngine;

public class SCANcontroller : ScenarioModule
{
	public static SCANcontroller controller {
		get {
			Game g = HighLogic.CurrentGame;
			if(g == null) return null;
			foreach(ProtoScenarioModule mod in g.scenarios) {
				if(mod.moduleName == typeof(SCANcontroller).Name) {
					return (SCANcontroller)mod.moduleRef;
				}
			}
			return (SCANcontroller)g.AddProtoScenarioModule(typeof(SCANcontroller), GameScenes.FLIGHT).moduleRef;
		}
		private set { }
	}

	public static int minScanAlt = 5000;
	public static int maxScanAlt = 500000;

	[KSPField(isPersistant = true)]
	public int colours = 0;

	[KSPField(isPersistant = true)]
	public bool map_markers = true;

	[KSPField(isPersistant = true)]
	public bool map_orbit = true;

	[KSPField(isPersistant = true)]
	public bool map_grid = true;

	[KSPField(isPersistant = true)]
	public int projection = 0;

	public override void OnLoad(ConfigNode node) {
		//colours = Convert.ToInt32(node.GetValue("colours"));
		ConfigNode node_vessels = node.GetNode("Scanners");
		if(node_vessels != null) {
			print("SCANsat Controller: Loading " + node_vessels.CountValues.ToString() + " known vessels");
			foreach(ConfigNode node_vessel in node_vessels.GetNodes("Vessel")) {
				Guid id = new Guid(node_vessel.GetValue("guid"));
				int sensors = Convert.ToInt32(node_vessel.GetValue("sensors"));
				knownVessels[id] = (SCANdata.SCANtype)sensors;
			}
		}
		ConfigNode node_progress = node.GetNode("Progress");
		if(node_progress != null) {
			foreach(ConfigNode node_body in node_progress.GetNodes("Body")) {
				string body_name = node_body.GetValue("Name");
				print("SCANsat Controller: Loading map for " + body_name);
				SCANdata body_data = getData(body_name);
				try {
					string mapdata = node_body.GetValue("Map");
					body_data.deserialize(mapdata);
				} catch(Exception e) {
					print(e.ToString());
					// fail somewhat gracefully; don't make the save unloadable 
				}
			}
		}
	}

	public override void OnSave(ConfigNode node) {
		//node.AddValue("colours", colours);
		ConfigNode node_vessels = new ConfigNode("Scanners");
		foreach(Guid id in knownVessels.Keys) {
			ConfigNode node_vessel = new ConfigNode("Vessel");
			node_vessel.AddValue("guid", id.ToString());
			node_vessel.AddValue("sensors", (int)knownVessels[id]);
			node_vessels.AddNode(node_vessel);
		}
		node.AddNode(node_vessels);
		ConfigNode node_progress = new ConfigNode("Progress");
		foreach(string body_name in body_data.Keys) {
			ConfigNode node_body = new ConfigNode("Body");
			SCANdata body_scan = body_data[body_name];
			node_body.AddValue("Name", body_name);
			node_body.AddValue("Map", body_scan.serialize());
			node_progress.AddNode(node_body);
		}
		node.AddNode(node_progress);
	}

	protected Dictionary<Guid, SCANdata.SCANtype> knownVessels = new Dictionary<Guid, SCANdata.SCANtype>();
	public void registerVesselID(Guid id, SCANdata.SCANtype sensor) {
		if(!knownVessels.ContainsKey(id)) knownVessels[id] = sensor;
		else knownVessels[id] |= sensor;
	}

	public void unregisterVesselID(Guid id, SCANdata.SCANtype sensor) {
		if(!knownVessels.ContainsKey(id)) return;
		knownVessels[id] = knownVessels[id] & ~sensor;
		if(knownVessels[id] == 0) knownVessels.Remove(id);
	}

	public bool isVesselKnown(Guid id, SCANdata.SCANtype sensor) {
		if(!knownVessels.ContainsKey(id)) return false;
		return (knownVessels[id] & sensor) != 0;
	}

	public bool isVesselKnown(Guid id) {
		if(!knownVessels.ContainsKey(id)) return false;
		return knownVessels[id] != 0;
	}

	public SCANdata.SCANtype activeSensorsOnVessel(Guid id) {
		if(!knownVessels.ContainsKey(id)) return 0;
		return knownVessels[id];
	}

	protected Dictionary<string, SCANdata> body_data = new Dictionary<string, SCANdata>();
	public SCANdata getData(string name) {
		if(!body_data.ContainsKey(name)) {
			body_data[name] = new SCANdata();
			body_data[name].resetImages();
		}
		return body_data[name];
	}
	public SCANdata getData(CelestialBody body) {
		SCANdata data = getData(body.name);
		data.body = body;
		return data;
	}

	public void registerPass(CelestialBody body, float lon, float lat, SCANdata.SCANtype type) {
		getData(body).registerPass(lon, lat, type);
	}

	protected static int last_scan_frame;
	public void scanFromAllVessels() {
		if(last_scan_frame != Time.frameCount) {
			last_scan_frame = Time.frameCount;
		} else {
			return;
		}
		foreach(Vessel v in FlightGlobals.Vessels) {
			if(v == null) continue;
			if(!isVesselKnown(v.id)) continue;
			SCANdata.SCANtype sensors = knownVessels[v.id];
			SCANdata data = getData(v.mainBody);
			if(v.heightFromTerrain < 2000 && v.heightFromTerrain >= 0 && ((int)sensors & (int)SCANdata.SCANtype.AnomalyDetail) != 0) {
				for(int x=-1; x<=1; x++) {
					for(int y=-1; y<=1; y++) {
						data.registerPass((int)v.longitude + x, (int)v.latitude + y, SCANdata.SCANtype.AnomalyDetail | SCANdata.SCANtype.Anomaly);
					}
				}
			}
			sensors = sensors & ~SCANdata.SCANtype.AnomalyDetail;
			SCANdata.SCANtype hires = 0;
			if((sensors & SCANdata.SCANtype.AltimetryHiRes) != 0) {
				sensors = sensors & ~SCANdata.SCANtype.AltimetryHiRes;
				hires = SCANdata.SCANtype.AltimetryHiRes;
			}
			if(v.altitude < minScanAlt || v.altitude > maxScanAlt) continue;
			for(int x=-5; x<=5; x++) {
				for(int y=-5; y<=5; y++) {
					data.registerPass((int)v.longitude + x, (int)v.latitude + y, sensors);
					if(hires != 0 && Math.Abs(x) < 3 && Math.Abs(y) < 3) {
						data.registerPass((int)v.longitude + x, (int)v.latitude + y, hires);
					}
				}
			}
		}
	}
}
