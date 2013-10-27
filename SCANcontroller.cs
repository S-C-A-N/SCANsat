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

	public override void OnLoad(ConfigNode node) {
		ConfigNode node_vessels = node.GetNode("Scanners");
		if(node_vessels != null) {
			print("SCANsat Controller: Loading " + node_vessels.CountValues.ToString() + " known vessels");
			foreach(string id in node_vessels.GetValues("guid")) {
				knownVessels.Add(new Guid(id));
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
		ConfigNode node_vessels = new ConfigNode("Scanners");
		foreach(Guid id in knownVessels) {
			node_vessels.AddValue("guid", id.ToString());
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

	protected HashSet<Guid> knownVessels = new HashSet<Guid>();
	public void registerVesselID(Guid id) {
		knownVessels.Add(id);
	}

	public void unregisterVesselID(Guid id) {
		knownVessels.Remove(id);
	}

	public bool isVesselKnown(Guid id) {
		return knownVessels.Contains(id);
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
		return getData(body.name);
	}

	public void registerPass(CelestialBody body, float lon, float lat) {
		getData(body).registerPass(body, lon, lat);
	}

	public void scanFromAllVessels() {
		foreach(Vessel v in FlightGlobals.Vessels) {
			if(v == null) continue;
			if(v.Landed) continue;
			if(!isVesselKnown(v.id)) continue;
			if(v.altitude < minScanAlt || v.altitude > maxScanAlt) continue;

			SCANdata data = getData(v.mainBody);
			for(int x=-5; x<=5; x++) {
				for(int y=-5; y<=5; y+=1) {
					data.registerPass(v.mainBody, (int)v.longitude + x, (int)v.latitude + y);
				}
			}
		}
	}

}
