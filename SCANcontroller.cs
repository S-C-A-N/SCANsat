/* 
 * [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 *
 * SCANcontroller - scenario module that handles all scanning
 * 
 * Copyright (c)2013 damny;
 * Copyright (c)2014 David Grandy <david.grandy@gmail.com>;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace SCANsat
{
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
		public bool map_flags = true;
		[KSPField(isPersistant = true)]
		public bool map_orbit = true;
		[KSPField(isPersistant = true)]
		public bool map_asteroids = true;
		[KSPField(isPersistant = true)]
		public bool map_grid = true;
        [KSPField(isPersistant = true)]
        public bool map_ResourceOverlay = false; //Is the overlay activated for the selected resource
		[KSPField(isPersistant = true)]
		public int projection = 0;
		[KSPField(isPersistant = true)]
		public int map_width = 0;
		[KSPField(isPersistant = true)]
		public int map_x = 100;
		[KSPField(isPersistant = true)]
		public int map_y = 50;
		[KSPField(isPersistant = true)]
		public string anomalyMarker = "✗";
		[KSPField(isPersistant = true)]
		public string closeBox = "✖";
		[KSPField(isPersistant = true)]
		public bool legend = false;
		[KSPField(isPersistant = true)]
		public bool scan_background = true;
		[KSPField(isPersistant = true)]
		public int timeWarpResolution = 20;
        [KSPField(isPersistant = true)]
        public bool globalOverlay = false; //Global resource overlay toggle
        [KSPField(isPersistant = true)]
        public int gridSelection = 0; //Which resource type is selected in the settings menu
        [KSPField(isPersistant = true)]
        public int resourceOverlayType = 0; //0 for ORS, 1 for Kethane

		public override void OnLoad(ConfigNode node) {
			ConfigNode node_vessels = node.GetNode("Scanners");
			if(node_vessels != null) {
				print("SCANsat Controller: Loading " + node_vessels.CountNodes.ToString() + " known vessels");
				foreach(ConfigNode node_vessel in node_vessels.GetNodes("Vessel")) {
					Guid id = new Guid(node_vessel.GetValue("guid"));
                    //string stext = node_vessel.GetValue("sensors");
                    //if(stext != null && stext != "") {
                    //    int sensors = Convert.ToInt32(stext);
                    //    if(sensors != 0) registerSensor(id, (SCANdata.SCANtype)sensors, 0, 0, 0, 0);
                    //}
					foreach(ConfigNode node_sensor in node_vessel.GetNodes("Sensor")) {
						int sensor = Convert.ToInt32(node_sensor.GetValue("type"));
                        int rscSensor = Convert.ToInt32(node_sensor.GetValue("rscType"));
						double fov = Convert.ToDouble(node_sensor.GetValue("fov"));
						double min_alt = Convert.ToDouble(node_sensor.GetValue("min_alt"));
						double max_alt = Convert.ToDouble(node_sensor.GetValue("max_alt"));
						double best_alt = Convert.ToDouble(node_sensor.GetValue("best_alt"));
						registerSensor(id, (SCANdata.SCANtype)sensor, (SCANdata.SCANResourceType)rscSensor, fov, min_alt, max_alt, best_alt);
					}
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
						body_data.deserialize(mapdata, 0);
                        string resourceMapData = node_body.GetValue("Resource Map");
                        body_data.deserialize(resourceMapData, 1);
					} catch(Exception e) {
						print(e.ToString());
						print(e.StackTrace);
						// fail somewhat gracefully; don't make the save unloadable 
					}
					body_data.disabled = Convert.ToBoolean(node_body.GetValue("Disabled"));
				}
			}
            OverlayResources();
            if (ResourcesList.Count == 0) {
                globalOverlay = false;
                SCANui.noResources = true;
            }
		}

		public override void OnSave(ConfigNode node) {
			ConfigNode node_vessels = new ConfigNode("Scanners");
			foreach(Guid id in knownVessels.Keys) {
				ConfigNode node_vessel = new ConfigNode("Vessel");
				node_vessel.AddValue("guid", id.ToString());
				if(knownVessels[id].vessel != null) node_vessel.AddValue("name", knownVessels[id].vessel.vesselName); // not read
				foreach(SCANsensor sensor in knownVessels[id].sensors.Values) {
					ConfigNode node_sensor = new ConfigNode("Sensor");
					node_sensor.AddValue("type", (int)sensor.sensor);
                    node_sensor.AddValue("rscType", (int)sensor.resourceSensor);
					node_sensor.AddValue("fov", sensor.fov);
					node_sensor.AddValue("min_alt", sensor.min_alt);
					node_sensor.AddValue("max_alt", sensor.max_alt);
					node_sensor.AddValue("best_alt", sensor.best_alt);
					node_vessel.AddNode(node_sensor);
				}
				node_vessels.AddNode(node_vessel);
			}
			node.AddNode(node_vessels);
			ConfigNode node_progress = new ConfigNode("Progress");
			foreach(string body_name in body_data.Keys) {
				ConfigNode node_body = new ConfigNode("Body");
				SCANdata body_scan = body_data[body_name];
				node_body.AddValue("Name", body_name);
				node_body.AddValue("Disabled", body_scan.disabled);
				node_body.AddValue("Map", body_scan.serialize(0));
                node_body.AddValue("Resource Map", body_scan.serialize(1));
				node_progress.AddNode(node_body);
			}
			node.AddNode(node_progress);
		}

		public class SCANsensor
		{
			public SCANdata.SCANtype sensor;
			public double fov;
			public double min_alt, max_alt, best_alt;
            public SCANdata.SCANResourceType resourceSensor; //additional variable to track resource scanners

			public bool inRange;
			public bool bestRange;
		}

		public class SCANvessel 
		{
			public Guid id;
			public Vessel vessel;
			public Dictionary<SCANdata.SCANtype, SCANsensor> sensors = new Dictionary<SCANdata.SCANtype, SCANsensor>();
            
			public CelestialBody body;
			public double latitude, longitude;
			public int frame;
			public double lastUT;
		}
        
        public List<string> ResourcesList = new List<string>(); //The list of all relevant resource names
        
        internal void OverlayResources() //Grab the resource name out of the relevant config node
        {
            ResourcesList.Clear();
            if (resourceOverlayType == 0)
            {
                foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("PLANETARY_RESOURCE_DEFINITION"))
                {
                    if (node != null)
                    {
                        string resourceName = node.GetValue("name");
                        if (!ResourcesList.Contains(resourceName)) ResourcesList.Add(resourceName); //ORS resources are defined per planet, this avoids repeats of the same type
                    }
                }
            }
            else if (resourceOverlayType == 1)
            {
                foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("KethaneResource"))
                {
                    if (node != null)
                    {
                        string resourceName = node.GetValue("Resource");
                        if (!ResourcesList.Contains(resourceName)) ResourcesList.Add(resourceName); //Just in case there are also Kethane duplicates
                    }
                }
            }
        }

        
        public SCANdata.SCANResourceType OverlayResourceType(string s) //Assign the proper resource type depending on the selection in the settings menu
        {
            if (resourceOverlayType == 0) {
                switch(s)
                {
                    case "Uranium": return SCANdata.SCANResourceType.ORS_1;
                    case "Thorium": return SCANdata.SCANResourceType.ORS_2;
                    case "Alumina": return SCANdata.SCANResourceType.ORS_3;
                    case "Water": return SCANdata.SCANResourceType.ORS_4;
                    case "Ore": return SCANdata.SCANResourceType.ORS_5;
                    case "Minerals": return SCANdata.SCANResourceType.ORS_6;
                    case "Substrate": return SCANdata.SCANResourceType.ORS_7;
                    default: return SCANdata.SCANResourceType.Nothing;
                }
            }
            else if (resourceOverlayType == 1) {
                switch(s)
                {
                    case "Kethane": return SCANdata.SCANResourceType.Kethane_1;
                    case "Ore": return SCANdata.SCANResourceType.Kethane_2;
                    case "Minerals": return SCANdata.SCANResourceType.Kethane_3;
                    case "Water": return SCANdata.SCANResourceType.Kethane_4;
                    case "Substrate": return SCANdata.SCANResourceType.Kethane_5;
                    default: return SCANdata.SCANResourceType.Nothing;
                }
            }
            return SCANdata.SCANResourceType.Nothing;
        }

        internal Color gridColor(string resource, int i) //Get the resource color
        {
            Color gridcolor = Color.white;
            if (resourceOverlayType == 0) //ORS resources might need to be manually set
            {
                gridcolor = Color.magenta;
            }
            else if (resourceOverlayType == 1)
            {
                foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("KethaneResource")) //Kethane resource colors stored in their config nodes
                {
                    if (node != null)
                    {
                        if (node.GetValue("Resource") == resource)
                        {
                            if (i == 0)
                            {
                                var color = node.GetValue("ColorFull");
                                gridcolor = ConfigNode.ParseColor(color);
                            }
                            else if (i == 1)
                            {
                                var color = node.GetValue("ColorEmpty");
                                gridcolor = ConfigNode.ParseColor(color);
                            }
                        }
                    }
                }
            }
            return gridcolor;
        }

        internal double ORSScalar(string s, CelestialBody b)
        {
            double scalar = 1f;
            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("PLANETARY_RESOURCE_DEFINITION"))
            {
                if (node != null)
                {
                    string resourceName = node.GetValue("name");
                    string bodyName = node.GetValue("celestialBodyName");
                    if (resourceName == s && bodyName == b.name)
                    {
                        if (double.TryParse(node.GetValue("scaleFactor"), out scalar)) return scalar;
                    }
                }
            }
            return scalar;
        }

        internal double ORSMultiplier(string s, CelestialBody b)
        {
            double mult = 1f;
            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("PLANETARY_RESOURCE_DEFINITION"))
            {
                if (node != null)
                {
                    string resourceName = node.GetValue("name");
                    string bodyName = node.GetValue("celestialBodyName");
                    if (resourceName == s && bodyName == b.name)
                    {
                        if (double.TryParse(node.GetValue("scaleMultiplier"), out mult)) return mult;
                    }
                }
            }
            return mult;
        }

		protected Dictionary<Guid, SCANvessel> knownVessels = new Dictionary<Guid, SCANvessel>();

        //Add the additional resource type variable to registered sensors
		public void registerSensor(Vessel v, SCANdata.SCANtype sensors, SCANdata.SCANResourceType resourceSensor, double fov, double min_alt, double max_alt, double best_alt) {
			registerSensor(v.id, sensors, resourceSensor, fov, min_alt, max_alt, best_alt);
			knownVessels[v.id].vessel = v;
			knownVessels[v.id].latitude = fixLatitude(v.latitude);
			knownVessels[v.id].longitude = fixLongitude(v.longitude);
		}

		public void registerSensor(Guid id, SCANdata.SCANtype sensors, SCANdata.SCANResourceType resourceSensor, double fov, double min_alt, double max_alt, double best_alt) {
			if(!knownVessels.ContainsKey(id)) knownVessels[id] = new SCANvessel();
			SCANvessel sv = knownVessels[id];
			sv.id = id;
			foreach(SCANdata.SCANtype sensor in Enum.GetValues(typeof(SCANdata.SCANtype))) {
				if(countBits((int)sensor) != 1) continue;
				if((sensor & sensors) == SCANdata.SCANtype.Nothing) continue;
				double this_fov = fov, this_min_alt = min_alt, this_max_alt = max_alt, this_best_alt = best_alt;
				if(this_max_alt <= 0) {
					this_min_alt = 5000;
					this_max_alt = 500000;
					this_best_alt = 200000;
					this_fov = 5;
					if((sensor & SCANdata.SCANtype.AltimetryHiRes) != SCANdata.SCANtype.Nothing) this_fov = 3;
					if((sensor & SCANdata.SCANtype.AnomalyDetail) != SCANdata.SCANtype.Nothing) {
						this_min_alt = 0;
						this_max_alt = 2000;
						this_best_alt = 0;
						this_fov = 1;
					}
				}
				if(!sv.sensors.ContainsKey(sensor)) sv.sensors[sensor] = new SCANsensor();
				SCANsensor s = sv.sensors[sensor];
				s.sensor = sensor;
                s.resourceSensor = resourceSensor; //Record resource type for the SCANsensor in question
				s.fov = this_fov;
				s.min_alt = this_min_alt;
				s.max_alt = this_max_alt;
				s.best_alt = this_best_alt;
			}
		}

		public void unregisterSensor(Vessel v, SCANdata.SCANtype sensors) {
			if(!knownVessels.ContainsKey(v.id)) return;
			SCANvessel sv = knownVessels[v.id];
			sv.id = v.id;
			sv.vessel = v;
			foreach(SCANdata.SCANtype sensor in Enum.GetValues(typeof(SCANdata.SCANtype))) {
				if((sensors & sensor) == SCANdata.SCANtype.Nothing) continue;
				if(!sv.sensors.ContainsKey(sensor)) continue;
				sv.sensors.Remove(sensor);
			}
		}

		public static int countBits(int i) {
			int count;
			for(count=0; i!=0; ++count) i &= (i - 1);
			return count;
		}

		public bool isVesselKnown(Guid id, SCANdata.SCANtype sensor) {
			if(!knownVessels.ContainsKey(id)) return false;
			SCANdata.SCANtype all = SCANdata.SCANtype.Nothing;
			foreach(SCANdata.SCANtype s in knownVessels[id].sensors.Keys) all |= s;
			return (all & sensor) != SCANdata.SCANtype.Nothing;
		}

		public bool isVesselKnown(Guid id) {
			if(!knownVessels.ContainsKey(id)) return false;
			return knownVessels[id].sensors.Count > 0;
		}

		public bool isVesselKnown(Vessel v) {
			if(v.vesselType == VesselType.Debris) return false;
			return isVesselKnown(v.id);
		}

		public SCANsensor getSensorStatus(Vessel v, SCANdata.SCANtype sensor) {
			if(!knownVessels.ContainsKey(v.id)) return null;
			if(!knownVessels[v.id].sensors.ContainsKey(sensor)) return null;
			return knownVessels[v.id].sensors[sensor];
		}

		public SCANdata.SCANtype activeSensorsOnVessel(Guid id) {
			if(!knownVessels.ContainsKey(id)) return SCANdata.SCANtype.Nothing;
			SCANdata.SCANtype sensors = SCANdata.SCANtype.Nothing;
			foreach(SCANdata.SCANtype s in knownVessels[id].sensors.Keys) sensors |= s;
			return sensors;
		}

		public ScienceData getAvailableScience(Vessel v, SCANdata.SCANtype sensor, bool notZero) {
			SCANdata data = getData(v.mainBody);
			ScienceData sd = null;
			ScienceExperiment se = null;
			ScienceSubject su = null;
			bool found = false;
			string id = null;
			double coverage = 0f;

			if(v.mainBody.pqsController != null) {
				if(!found && (sensor & SCANdata.SCANtype.AltimetryLoRes) != SCANdata.SCANtype.Nothing) {
					found = true;
					id = "SCANsatAltimetryLoRes";
					coverage = data.getCoveragePercentage(SCANdata.SCANtype.AltimetryLoRes);
				}
				if(!found && (sensor & SCANdata.SCANtype.AltimetryHiRes) != SCANdata.SCANtype.Nothing) {
					found = true;
					id = "SCANsatAltimetryHiRes";
					coverage = data.getCoveragePercentage(SCANdata.SCANtype.AltimetryHiRes);
				}
			}
			if(v.mainBody.BiomeMap != null) {
				if(!found && (sensor & SCANdata.SCANtype.Biome) != SCANdata.SCANtype.Nothing) {
					found = true;
					id = "SCANsatBiomeAnomaly";
					coverage = data.getCoveragePercentage(SCANdata.SCANtype.Biome | SCANdata.SCANtype.Anomaly);
				}
			}

			if(!found) return null;

			se = ResearchAndDevelopment.GetExperiment(id);
			if(se == null) return null;

			su = ResearchAndDevelopment.GetExperimentSubject(se, ExperimentSituations.InSpaceHigh, v.mainBody, "surface");
			if(su == null) return null;

			print("[SCANsat] coverage " + coverage.ToString("F1") + ", science cap " + su.scienceCap.ToString("F1") + ", subject value " + su.subjectValue.ToString("F2") + ", science value " + su.scientificValue.ToString("F2") + ", science " + su.science.ToString("F2"));
			su.scientificValue = 1;

			float science = (float)coverage;
			if(science > 95) science = 100;
			if(science < 30) science = 0;
			science = science / 100f;
			science = Mathf.Max(0, (science * su.scienceCap) - su.science);

			print("[SCANsat] remaining science: " + science.ToString("F1") + ", base = " + (se.baseValue).ToString("F1"));

			science /= Mathf.Max(0.1f, su.scientificValue);
			science /= su.subjectValue;

			print("[SCANsat] result = " + science.ToString("F2"));

			if(notZero && science <= 0) science = 0.00001f;

			sd = new ScienceData(science, 1f, 0f, id, se.experimentTitle + " of " + v.mainBody.theName);
			sd.subjectID = su.id;
			return sd;
		}

		public Dictionary<string, SCANdata> body_data = new Dictionary<string, SCANdata>();

		public SCANdata getData(string name) {
			if(!body_data.ContainsKey(name)) {
				body_data[name] = new SCANdata();
				body_data[name].resetImages();
			}
			return body_data[name];
		}

		public SCANdata getData(CelestialBody body) {
			SCANdata data = getData(body.bodyName);
			data.body = body;
			return data;
		}

        //public void registerPass(CelestialBody body, float lon, float lat, SCANdata.SCANtype type) {
        //    getData(body).registerPass(lon, lat, type);
        //}

		public double fixLatitude(double lat) {
			return (lat + 90 + 180) % 180 - 90;
		}

		public double fixLongitude(double lon) {
			return (lon + 180 + 360) % 360 - 180;
		}

		protected static HashSet<string> disabledBodies = new HashSet<string>();
		public static bool bodyIsDisabled(string name) {
			return disabledBodies.Contains(name);
		}

		public static void setBodyDisabled(string name, bool disabled) {
			if(disabled) disabledBodies.Add(name);
			else disabledBodies.Remove(name);
		}

		protected static int last_scan_frame;
		protected static float last_scan_time;
		protected static double scan_UT;
		public static int activeSensors, activeVessels;
		private static int currentActiveSensor, currentActiveVessel;
		public void scanFromAllVessels() {
			if(Time.realtimeSinceStartup - last_scan_time < 1 && Time.realtimeSinceStartup > last_scan_time) return;
			if(last_scan_frame == Time.frameCount) return;
			last_scan_frame = Time.frameCount;
			last_scan_time = Time.realtimeSinceStartup;
			scan_UT = Planetarium.GetUniversalTime();
			currentActiveSensor = 0;
			currentActiveVessel = 0;
			actualPasses = 0;
			maxRes = 0;
			foreach(SCANdata data in body_data.Values) data.updateCoverage();
			foreach(Vessel v in FlightGlobals.Vessels) {
				if(!knownVessels.ContainsKey(v.id)) continue;
				SCANvessel vessel = knownVessels[v.id];
				SCANdata data = getData(v.mainBody);
				vessel.vessel = v;
			
				if(!data.disabled) {
					if(v.mainBody == FlightGlobals.currentMainBody || scan_background) {
						if(isVesselKnown(v)) {
							doScanPass(knownVessels[v.id], scan_UT, scan_UT, vessel.lastUT, vessel.latitude, vessel.longitude);
							++currentActiveVessel;
							currentActiveSensor += knownVessels[v.id].sensors.Count;
						}
					}
				}

				vessel.body = v.mainBody;
				vessel.frame = Time.frameCount;
				vessel.lastUT = scan_UT;
				vessel.latitude = fixLatitude(v.latitude);
				vessel.longitude = fixLongitude(v.longitude);
			}
			activeVessels = currentActiveVessel;
			activeSensors = currentActiveSensor;
		}

		public int actualPasses, maxRes;
		protected static Queue<double> scanQueue;
		protected void doScanPass(SCANvessel vessel, double UT, double startUT, double lastUT, double llat, double llon) {
			Vessel v = vessel.vessel;
			SCANdata data = getData(v.mainBody);
			double soi_radius = v.mainBody.sphereOfInfluence - v.mainBody.Radius;
			double alt = v.altitude, lat = fixLatitude(v.latitude), lon = fixLongitude(v.longitude);
			double res = 0;
			Orbit o = v.orbit;
			bool uncovered;

			if(scanQueue == null) scanQueue = new Queue<double>();
			if(scanQueue.Count != 0) scanQueue.Clear();

		loop: // don't look at me like that, I just unrolled the recursion
			if(res > 0) {
				if(double.IsNaN(UT)) goto dequeue;
				if(o.ObT <= 0) goto dequeue;
				if(double.IsNaN(o.getObtAtUT(UT))) goto dequeue;
				Vector3d pos = o.getPositionAtUT(UT);
				double rotation = 0;
				if(v.mainBody.rotates) {
					rotation = (360 * ((UT - scan_UT) / v.mainBody.rotationPeriod)) % 360;
				}
				alt = v.mainBody.GetAltitude(pos);
				lat = fixLatitude(v.mainBody.GetLatitude(pos));
				lon = fixLongitude(v.mainBody.GetLongitude(pos) - rotation);
				if(alt < 0) alt = 0;
				if(res > maxRes) maxRes = (int)res;
			} else {
				alt = v.heightFromTerrain;
				if(alt < 0) alt = v.altitude;
			}

			if(Math.Abs(lat - llat) < 1 && Math.Abs(lon - llon) < 1 && res > 0) goto dequeue;
			actualPasses++;

			uncovered = res <= 0;
			foreach(SCANsensor sensor in knownVessels[v.id].sensors.Values) {
				if(res <= 0) {
					if(data.getCoverage(sensor.sensor) > 0) uncovered = false;
				}

				sensor.inRange = false;
				sensor.bestRange = false;
				if(alt < sensor.min_alt) continue;
				if(alt > Math.Min(sensor.max_alt, soi_radius)) continue;
				sensor.inRange = true;

				double fov = sensor.fov;
				double ba = Math.Min(sensor.best_alt, soi_radius);
				if(alt < ba) fov = (alt / ba) * fov;
				else sensor.bestRange = true;

				double surfscale = 600000d/v.mainBody.Radius;
				if(surfscale < 1) surfscale = 1;
				surfscale = Math.Sqrt(surfscale);
				fov *= surfscale;
				if(fov > 20) fov = 20;

				int f = (int)Math.Truncate(fov);
				int f1 = f + (int)Math.Round(fov - f);
				
				double clampLat;
				double clampLon;
				for(int x=-f; x<=f1; ++x) {
					clampLon = lon + x;	// longitude does not need clamping
					/*if (clampLon < 0  ) clampLon = 0; */
					/*if (clampLon > 360) clampLon = 360; */
					for(int y=-f; y<=f1; ++y) {
						clampLat = lat + y;
						if (clampLat > 90) clampLat = 90;
						if (clampLat < -90) clampLat = -90;
                        if (sensor.resourceSensor != 0) 
                            data.registerResourcePass(clampLon, clampLat, sensor.resourceSensor); //Additional scan pass for resource scanners, probably needs some tweaking
					    else
                            data.registerPass(clampLon, clampLat, sensor.sensor);
                    }
				}
			}
			if(uncovered) return;
			/* 
			if(v.mainBody == FlightGlobals.currentMainBody) {
				if(res > 0) data.map_small.SetPixel((int)Math.Round(lon) + 180, (int)Math.Round(lat) + 90, Color.magenta);
				else data.map_small.SetPixel((int)Math.Round(lon) + 180, (int)Math.Round(lat) + 90, Color.yellow);
			}
			*/

			if(vessel.lastUT <= 0) return;
			if(vessel.frame <= 0) return;
			if(v.LandedOrSplashed) return;
			if(res >= timeWarpResolution) goto dequeue;

			if(startUT > UT) {
				scanQueue.Enqueue((startUT + UT) / 2);
				scanQueue.Enqueue(startUT);
				scanQueue.Enqueue(UT);
				scanQueue.Enqueue(lat);
				scanQueue.Enqueue(lon);
				scanQueue.Enqueue(res + 1);
			}
			startUT = UT;
			UT = (lastUT + UT) / 2;
			llat = lat;
			llon = lon;
			res = res + 1;
			goto loop;
		
		dequeue:
			if(scanQueue.Count <= 0) return;
			UT = scanQueue.Dequeue();
			startUT = scanQueue.Dequeue();
			lastUT = scanQueue.Dequeue();
			llat = scanQueue.Dequeue();
			llon = scanQueue.Dequeue();
			res = scanQueue.Dequeue();
			goto loop;
		}
	}
}
