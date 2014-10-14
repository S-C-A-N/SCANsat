#region license
/* 
 *  [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 *
 * SCANsat - Big map window object
 * 
 * This is all essentially place-holder code to be replaced by a version
 * of the SCANkscMap
 * 
 * Copyright (c)2013 damny;
 * Copyright (c)2014 David Grandy <david.grandy@gmail.com>;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
 *
 */
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using SCANsat.Platform;
using SCANsat;
using UnityEngine;

using palette = SCANsat.SCANpalette;

namespace SCANsat.SCAN_UI
{
	class SCANbigMap : MBW
	{
		private static SCANmap bigmap;
		private SCANmap spotmap;
		private Vessel v;
		private CelestialBody b;
		private SCANdata data;
		private double startUT;
		private bool bigmap_dragging, overlay_static_dirty, notMappingToday;
		private float bigmap_drag_w, bigmap_drag_x;
		private Texture2D overlay_static;
		private Rect rc = new Rect(0, 0, 20, 20);
		private Rect pos_spotmap = new Rect(10f, 10f, 10f, 10f);
		internal static Rect defaultRect = new Rect(350, 55, 380, 180);
		internal static int[] eq_an_map, eq_dn_map;
		internal static Texture2D eq_map;
		internal static int eq_frame;

		protected override void Awake()
		{
			WindowCaption = "Map of ";
			WindowRect = defaultRect;
			WindowOptions = new GUILayoutOption[2] { GUILayout.Width(360), GUILayout.Height(180) };
			WindowStyle = SCANskins.SCAN_window;
			Visible = false;
			DragEnabled = true;
			ClampEnabled = false;

			SCAN_SkinsLibrary.SetCurrent("SCAN_Unity");
		}

		internal override void Start()
		{
			GameEvents.onVesselSOIChanged.Add(soiChanged);
			if (bigmap == null)
			{
				bigmap = new SCANmap();
				bigmap.setProjection((SCANmap.MapProjection)SCANcontroller.controller.projection);
				bigmap.setWidth(SCANcontroller.controller.map_width);
				WindowRect.x = SCANcontroller.controller.map_x;
				WindowRect.y = SCANcontroller.controller.map_y;
			}
			else
			{
				WindowRect.x = SCANcontroller.controller.map_x;
				WindowRect.y = SCANcontroller.controller.map_y;
			}
			b = FlightGlobals.currentMainBody;
			WindowCaption = string.Format("Map of {0}", b.theName);
			bigmap.setBody(b);
		}

		internal override void OnDestroy()
		{
			GameEvents.onVesselSOIChanged.Remove(soiChanged);
		}

		protected override void DrawWindowPre(int id)
		{
			bool repainting = Event.current.type == EventType.Repaint;

			// Handle dragging independently of mouse events: MouseDrag doesn't work
			// so well if the resizer widget can't follow the mouse because the map
			// aspect ratio is constrained.
			if (bigmap_dragging && !repainting)
			{
				if (Input.GetMouseButtonUp(0))
				{
					bigmap_dragging = false;
					if (bigmap_drag_w < 450)
						bigmap_drag_w = 450;
					bigmap.setWidth((int)bigmap_drag_w);
					overlay_static = null;
					SCANcontroller.controller.map_width = bigmap.mapwidth;
				}
				else
				{
					float xx = Input.mousePosition.x;
					bigmap_drag_w += xx - bigmap_drag_x;
					bigmap_drag_x = xx;
				}
				if (Event.current.isMouse)
					Event.current.Use();
			}

			v = FlightGlobals.ActiveVessel;
			b = v.mainBody;
			data = SCANUtil.getData(b);
		}

		protected override void DrawWindow(int id)
		{
			versionLabel(id);

			growS();

			Texture2D map = bigmap.getPartialMap(); // this is the expensive call

			//Makes sure the window re-sizes correctly
			float dw = bigmap_drag_w;
			if (dw < 450) dw = 450;	// changed to prevent resizer from overlapping with buttons
			float dh = dw / 2f;
			if (bigmap_dragging)
			{
				GUILayout.Label("", GUILayout.Width(dw), GUILayout.Height(dh));
			}
			else
			{
				GUILayout.Label("", GUILayout.Width(map.width), GUILayout.Height(map.height));
			}

			Rect maprect = GUILayoutUtility.GetLastRect();
			maprect.width = bigmap.mapwidth;
			maprect.height = bigmap.mapheight;

			//The background texture for the map
			if (overlay_static == null)
			{
				overlay_static = new Texture2D((int)bigmap.mapwidth, (int)bigmap.mapheight, TextureFormat.ARGB32, false);
				overlay_static_dirty = true;
			}

			//Generate the grid texture
			if (overlay_static_dirty)
			{
				SCANuiUtil.clearTexture(overlay_static);
				if (SCANcontroller.controller.map_grid)
				{
					SCANuiUtil.drawGrid(maprect, bigmap, overlay_static);
				}
				overlay_static.Apply();
				overlay_static_dirty = false;
			}

			//Stretches the existing map while re-sizing
			if (bigmap_dragging)
			{
				maprect.width = dw;
				maprect.height = dh;
				GUI.DrawTexture(maprect, map, ScaleMode.StretchToFill);
			}
			else
			{
				GUI.DrawTexture(maprect, map); // this is the drawing of the map
			}

			if (overlay_static != null)
			{
				GUI.DrawTexture(maprect, overlay_static, ScaleMode.StretchToFill);
			}

			//Add the North/South labels to the polar projection
			if (bigmap.projection == SCANmap.MapProjection.Polar)
			{
				rc.x = maprect.x + maprect.width / 2 - maprect.width / 8;
				rc.y = maprect.y + maprect.height / 8;
				SCANuiUtil.drawLabel(rc, "S", false, true, true);
				rc.x = maprect.x + maprect.width / 2 + maprect.width / 8;
				SCANuiUtil.drawLabel(rc, "N", false, true, true);
			}

			//Draw the orbit overlays
			if (SCANcontroller.controller.map_orbit && !notMappingToday)
			{
				SCANuiUtil.drawOrbit(maprect, bigmap, v, startUT, overlay_static);
			}

			growE(GUILayout.ExpandWidth(true));
			growE(GUILayout.Width(300));

			#region buttons: close / export png
			growS();
			if (GUILayout.Button("Close", SCANskins.SCAN_buttonFixed))
			{
				Visible = false;
			}

			fillS();

			if (GUILayout.Button("Export PNG", SCANskins.SCAN_buttonFixed))
			{
				if (bigmap.isMapComplete())
				{
					bigmap.exportPNG();
				}
			}
			#endregion

			stopS();
			growS();

			#region buttons: grey / color / legend
			if (SCANcontroller.controller.colours == 1)
			{
				if (GUILayout.Button("Grey", SCANskins.SCAN_buttonActive))
				{
					SCANcontroller.controller.colours = 1;
				}

				if (GUILayout.Button("Colour"))
				{
					SCANcontroller.controller.colours = 0;
					data.resetImages();
					bigmap.resetMap();
				}
			}
			else
			{
				if (GUILayout.Button("Grey"))
				{
					SCANcontroller.controller.colours = 1;
					data.resetImages();
					bigmap.resetMap();
				}

				if (GUILayout.Button("Colour", SCANskins.SCAN_buttonActive))
				{
					SCANcontroller.controller.colours = 0;
				}
			}

			if (SCANcontroller.controller.legend)
			{
				if (GUILayout.Button("Legend", SCANskins.SCAN_buttonActive))
					SCANcontroller.controller.legend = !SCANcontroller.controller.legend;
			}
			else
			{
				if (GUILayout.Button("Legend"))
					SCANcontroller.controller.legend = !SCANcontroller.controller.legend;
			}

			#endregion
			stopS();
			growS();
			#region buttons: altimetry / slope / biome
			if (bigmap.mapmode == 0)
			{
				if (GUILayout.Button("Altimetry", SCANskins.SCAN_buttonActive))
				{
					bigmap.resetMap(0, 0);
				}
				if (GUILayout.Button("Slope"))
				{
					bigmap.resetMap(1, 0);
				}
				if (GUILayout.Button("Biome"))
				{
					bigmap.resetMap(2, 0);
				}
			}
			else if (bigmap.mapmode == 1)
			{
				if (GUILayout.Button("Altimetry"))
				{
					bigmap.resetMap(0, 0);
				}
				if (GUILayout.Button("Slope", SCANskins.SCAN_buttonActive))
				{
					bigmap.resetMap(1, 0);
				}
				if (GUILayout.Button("Biome"))
				{
					bigmap.resetMap(2, 0);
				}
			}
			else if (bigmap.mapmode == 2)
			{
				if (GUILayout.Button("Altimetry"))
				{
					bigmap.resetMap(0, 0);
				}
				if (GUILayout.Button("Slope"))
				{
					bigmap.resetMap(1, 0);
				}
				if (GUILayout.Button("Biome", SCANskins.SCAN_buttonActive))
				{
					bigmap.resetMap(2, 0);
				}
			}
			#endregion
			stopS();
			growS();
			growE();
			#region buttons: anomaly markers / flags / orbit / asteroids
			if (SCANcontroller.controller.map_markers)
			{
				if (GUILayout.Button("Markers", SCANskins.SCAN_buttonActive))
					SCANcontroller.controller.map_markers = !SCANcontroller.controller.map_markers;
			}
			else
			{
				if (GUILayout.Button("Markers"))
					SCANcontroller.controller.map_markers = !SCANcontroller.controller.map_markers;
			}

			if (SCANcontroller.controller.map_flags)
			{
				if (GUILayout.Button("Flags", SCANskins.SCAN_buttonActive))
					SCANcontroller.controller.map_flags = !SCANcontroller.controller.map_flags;
			}
			else
			{
				if (GUILayout.Button("Flags"))
					SCANcontroller.controller.map_flags = !SCANcontroller.controller.map_flags;
			}

			stopE();
			growE();

			if (SCANcontroller.controller.map_orbit)
			{
				if (GUILayout.Button("Orbit", SCANskins.SCAN_buttonActive))
					SCANcontroller.controller.map_orbit = !SCANcontroller.controller.map_orbit;
			}
			else
			{
				if (GUILayout.Button("Orbit"))
					SCANcontroller.controller.map_orbit = !SCANcontroller.controller.map_orbit;
			}

			if (SCANcontroller.controller.map_asteroids)
			{
				if (GUILayout.Button("Asteroids", SCANskins.SCAN_buttonActive))
					SCANcontroller.controller.map_asteroids = !SCANcontroller.controller.map_asteroids;
			}
			else
			{
				if (GUILayout.Button("Asteroids"))
					SCANcontroller.controller.map_asteroids = !SCANcontroller.controller.map_asteroids;
			}

			#endregion
			stopE();
			growE();
			#region buttons: grid / [resources]

			if (SCANcontroller.controller.map_grid)
			{
				if (GUILayout.Button("Grid", SCANskins.SCAN_buttonActive))
				{
					SCANcontroller.controller.map_grid = !SCANcontroller.controller.map_grid;
					overlay_static_dirty = true;
				}
			}
			else
			{
				if (GUILayout.Button("Grid"))
				{
					SCANcontroller.controller.map_grid = !SCANcontroller.controller.map_grid;
					overlay_static_dirty = true;
				}
			}

			if (SCANcontroller.controller.globalOverlay) //Button to turn on/off resource overlay
			{
				if (!SCANcontroller.controller.kethaneBusy || SCANcontroller.controller.resourceOverlayType == 0)
				{
					if (SCANcontroller.controller.map_ResourceOverlay)
					{
						if (GUILayout.Button(SCANcontroller.ResourcesList[SCANcontroller.controller.gridSelection].name, SCANskins.SCAN_buttonActive))
						{
							SCANcontroller.controller.map_ResourceOverlay = !SCANcontroller.controller.map_ResourceOverlay;
							bigmap.resetMap();
						}
					}
					else
					{
						if (GUILayout.Button(SCANcontroller.ResourcesList[SCANcontroller.controller.gridSelection].name))
						{
							SCANcontroller.controller.map_ResourceOverlay = !SCANcontroller.controller.map_ResourceOverlay;
							bigmap.resetMap();
						}
					}
				}
				else
				{ //Disable overlay while kethane database is rebuilding
					GUILayout.Button("Rebuilding...");
				}
			}

			#endregion

			stopE();
			stopS();
			growS();

			#region buttons: projections
			for (int i = 0; i < SCANmap.projectionNames.Length; ++i)
			{
				if (bigmap.projection.ToString() == SCANmap.projectionNames[i])
				{
					if (GUILayout.Button(SCANmap.projectionNames[i], SCANskins.SCAN_buttonActive))
					{
						bigmap.setProjection((SCANmap.MapProjection)i);
						SCANcontroller.controller.projection = i;
						overlay_static_dirty = true;
					}
				}
				else
				{
					if (GUILayout.Button(SCANmap.projectionNames[i]))
					{
						bigmap.setProjection((SCANmap.MapProjection)i);
						SCANcontroller.controller.projection = i;
						overlay_static_dirty = true;
					}
				}
			}
			#endregion

			stopS();
			stopE();

			//string scanInfo = "";
			//string posInfo = "";
			//string resourceInfo = "";
			float mx = Event.current.mousePosition.x - maprect.x;
			float my = Event.current.mousePosition.y - maprect.y;
			bool in_map = false, in_spotmap = false;
			double mlon = 0, mlat = 0;

			if (mx >= 0 && my >= 0 && mx < map.width && my < map.height && !bigmap_dragging)
			{
				double mlo = (mx * 360f / map.width) - 180;
				double mla = 90 - (my * 180f / map.height);
				mlon = bigmap.unprojectLongitude(mlo, mla);
				mlat = bigmap.unprojectLatitude(mlo, mla);

				if (spotmap != null)
				{
					if (mx >= pos_spotmap.x - maprect.x && my >= pos_spotmap.y - maprect.y && mx <= pos_spotmap.x + pos_spotmap.width - maprect.x && my <= pos_spotmap.y + pos_spotmap.height - maprect.y)
					{
						in_spotmap = true;
						mlon = spotmap.lon_offset + ((mx - pos_spotmap.x + maprect.x) / spotmap.mapscale) - 180;
						mlat = spotmap.lat_offset + ((pos_spotmap.height - (my - pos_spotmap.y + maprect.y)) / spotmap.mapscale) - 90;
						if (mlat > 90)
						{
							mlon = (mlon + 360) % 360 - 180;
							mlat = 180 - mlat;
						}
						else if (mlat < -90)
						{
							mlon = (mlon + 360) % 360 - 180;
							mlat = -180 - mlat;
						}
					}
				}

				if (mlon >= -180 && mlon <= 180 && mlat >= -90 && mlat <= 90)
				{
					in_map = true;
				}
			}

			#region if (mouse inside bigmap)
			//	if (mlon >= -180 && mlon <= 180 && mlat >= -90 && mlat <= 90)
			//	{
			//		in_map = true;
			//		if (SCANUtil.isCovered(mlon, mlat, data, SCANdata.SCANtype.AltimetryLoRes))
			//		{
			//			if (v.mainBody.pqsController == null)
			//				scanInfo += palette.colored(palette.c_ugly, "LO ");
			//			else
			//				scanInfo += palette.colored(palette.c_good, "LO ");
			//		}
			//		else
			//			scanInfo += "<color=\"grey\">LO</color> ";
			//		if (SCANUtil.isCovered(mlon, mlat, data, SCANdata.SCANtype.AltimetryHiRes))
			//		{
			//			if (v.mainBody.pqsController == null)
			//				scanInfo += palette.colored(palette.c_ugly, "HI ");
			//			else
			//				scanInfo += palette.colored(palette.c_good, "HI ");
			//		}
			//		else
			//			scanInfo += "<color=\"grey\">HI</color> ";
			//		if (SCANUtil.isCovered(mlon, mlat, data, SCANdata.SCANtype.Biome))
			//		{
			//			if (v.mainBody.BiomeMap == null || v.mainBody.BiomeMap.Map == null)
			//				scanInfo += palette.colored(palette.c_ugly, "MULTI ");
			//			else
			//				scanInfo += palette.colored(palette.c_good, "MULTI ");
			//		}
			//		else
			//			scanInfo += "<color=\"grey\">MULTI</color> ";
			//		if (SCANUtil.isCovered(mlon, mlat, data, SCANdata.SCANtype.AltimetryHiRes))
			//		{
			//			scanInfo += SCANUtil.getElevation(v.mainBody, mlon, mlat).ToString("N2") + "m ";
			//		}
			//		else if (SCANUtil.isCovered(mlon, mlat, data, SCANdata.SCANtype.AltimetryLoRes))
			//		{
			//			scanInfo += (((int)SCANUtil.getElevation(v.mainBody, mlon, mlat) / 500) * 500).ToString() + "m ";
			//		}
			//		if (SCANUtil.isCovered(mlon, mlat, data, SCANdata.SCANtype.Biome))
			//		{
			//			scanInfo += SCANUtil.getBiomeName(v.mainBody, mlon, mlat) + " ";
			//		}
			//		posInfo += SCANuiUtil.toDMS(mlat, mlon) + " (lat: " + mlat.ToString("F2") + " lon: " + mlon.ToString("F2") + ") ";
			//		if (in_spotmap)
			//			posInfo += " " + spotmap.mapscale.ToString("F1") + "x";
			//		if (SCANcontroller.controller.map_ResourceOverlay && SCANcontroller.controller.globalOverlay) //Adds selected resource amount to big map legend
			//		{
			//			if (SCANcontroller.controller.resourceOverlayType == 0 && SCANreflection.ORSXFound)
			//			{
			//				if (SCANUtil.isCovered(mlon, mlat, data, bigmap.resource.type))
			//				{
			//					double amount = SCANUtil.ORSOverlay(mlon, mlat, bigmap.body.flightGlobalsIndex, bigmap.resource.name);
			//					string label;
			//					if (bigmap.resource.linear) //Make sure that ORS values are handled correctly based on which scale type they use
			//						label = (amount * 100).ToString("N1") + " %";
			//					else
			//						label = (amount * 1000000).ToString("N1") + " ppm";
			//					resourceInfo += palette.colored(bigmap.resource.fullColor, bigmap.resource.name + ": " + label);
			//				}
			//			}
			//			else if (SCANcontroller.controller.resourceOverlayType == 1)
			//			{
			//				if (SCANUtil.isCovered(mlon, mlat, data, bigmap.resource.type))
			//				{
			//					double amount = data.kethaneValueMap[SCANUtil.icLON(mlon), SCANUtil.icLAT(mlat)];
			//					if (amount < 0) amount = 0d;
			//					resourceInfo += palette.colored(bigmap.resource.fullColor, bigmap.resource.name + ": " + amount.ToString("N1"));
			//				}
			//			}
			//		}
			//	}
			//	//else
			//	//{
			//	//	info += " " + mlat.ToString("F") + " " + mlon.ToString("F"); // uncomment for debugging projections
			//	//}
			//}
			#endregion

			bool repainting = Event.current.type == EventType.Repaint;

			#region bigmap mouseover info and legend

			if (maprect.width < 720)
			{
				stopE();
				SCANuiUtil.mouseOverInfo(mlon, mlat, bigmap, data, v.mainBody, in_map);
				if (bigmap.mapmode == 0 && SCANcontroller.controller.legend)
					SCANuiUtil.drawLegend();
			}
			else
			{
				growS();
				SCANuiUtil.mouseOverInfo(mlon, mlat, bigmap, data, v.mainBody, in_map);
				if (bigmap.mapmode == 0 && SCANcontroller.controller.legend)
					SCANuiUtil.drawLegend();
				stopS();
				stopE();
			}
			#endregion

			if (!notMappingToday)
				SCANuiUtil.drawMapLabels(maprect, v, bigmap, data, v.mainBody);

			#region zoom map
			if (spotmap != null)
			{
				spotmap.setBody(v.mainBody);

				if (SCANcontroller.controller.globalOverlay) //make sure resources show up in zoom map
					spotmap.setResource(SCANcontroller.ResourcesList[SCANcontroller.controller.gridSelection].name);

				GUI.Box(pos_spotmap, spotmap.getPartialMap());
				if (!notMappingToday)
				{
					SCANuiUtil.drawOrbit(pos_spotmap, spotmap, v, startUT, overlay_static);
					SCANuiUtil.drawMapLabels(pos_spotmap, v, spotmap, data, v.mainBody);
				}
				Rect sC = new Rect(pos_spotmap.x + 160, pos_spotmap.y, 18, 18);
				if (GUI.Button(sC, palette.colored(palette.cb_vermillion, SCANcontroller.controller.closeBox)))
				{
					spotmap = null;
				}
			}
			#endregion
			#region big map resizing
			Rect resizer = new Rect(maprect.x + maprect.width - 24, maprect.y + maprect.height + 8, 24, 24);
			GUI.Box(resizer, "//");
			if (Event.current.isMouse)
			{
				if (Event.current.type == EventType.MouseUp)
				{
					if (bigmap_dragging)
					{
					}
					else if (Event.current.button == 1)
					{
						if (in_map || in_spotmap)
						{
							if (bigmap.isMapComplete())
							{
								if (spotmap == null)
								{
									spotmap = new SCANmap();
									spotmap.setSize(180, 180);
								}
								if (in_spotmap)
								{
									spotmap.mapscale = spotmap.mapscale * 1.25f;
								}
								else
								{
									spotmap.mapscale = 10;
								}
								spotmap.centerAround(mlon, mlat);
								spotmap.resetMap(bigmap.mapmode, 1);
								pos_spotmap.width = 180;
								pos_spotmap.height = 180;
								if (!in_spotmap)
								{
									pos_spotmap.x = Event.current.mousePosition.x - pos_spotmap.width / 2;
									pos_spotmap.y = Event.current.mousePosition.y - pos_spotmap.height / 2;
									if (mx > maprect.width / 2)
										pos_spotmap.x -= pos_spotmap.width;
									else
										pos_spotmap.x += pos_spotmap.height;
									pos_spotmap.x = Math.Max(maprect.x, Math.Min(maprect.x + maprect.width - pos_spotmap.width, pos_spotmap.x));
									pos_spotmap.y = Math.Max(maprect.y, Math.Min(maprect.y + maprect.height - pos_spotmap.height, pos_spotmap.y));
								}
							}
						}
					}
					else if (Event.current.button == 0)
					{
						if (spotmap != null && in_spotmap)
						{
							if (bigmap.isMapComplete())
							{
								spotmap.mapscale = spotmap.mapscale / 1.25f;
								if (spotmap.mapscale < 10)
									spotmap.mapscale = 10;
								spotmap.resetMap(spotmap.mapmode, 1);
								Event.current.Use();
							}
						}
					}
					Event.current.Use();
				}
				else if (Event.current.type == EventType.MouseDown)
				{
					if (Event.current.button == 0)
					{
						if (resizer.Contains(Event.current.mousePosition))
						{
							bigmap_dragging = true;
							bigmap_drag_x = Input.mousePosition.x;
							bigmap_drag_w = bigmap.mapwidth;
							Event.current.Use();
						}
					}
				}
			}
			#endregion

			stopS();

		}

		protected override void DrawWindowPost(int id)
		{
			string titleText = "";
			if (bigmap_dragging)
				titleText = string.Format(" [{0}x{1}]", bigmap_drag_w, (bigmap_drag_w / 2));
			if (!bigmap.isMapComplete())
				titleText += " [rendering]";

			WindowCaption = string.Format("Map of {0}{1}", b.theName, titleText);

			if (SCANcontroller.controller.globalOverlay) //Update selected resource
				bigmap.setResource(SCANcontroller.ResourcesList[SCANcontroller.controller.gridSelection].name);

			SCANcontroller.controller.map_x = (int)WindowRect.x;
			SCANcontroller.controller.map_y = (int)WindowRect.y;
		}

		//Print the version number
		private void versionLabel(int id)
		{
			Rect vR = new Rect(6, 0, 40, 18);
			GUI.Label(vR, SCANversions.SCANsatVersion, SCANskins.SCAN_whiteReadoutLabel);
		}

		private void soiChanged(GameEvents.HostedFromToAction<Vessel, CelestialBody> VC)
		{
			b = VC.to;
			bigmap.setBody(b);
			WindowCaption = string.Format("Map of {0}", b.theName);
		}

	}
}
