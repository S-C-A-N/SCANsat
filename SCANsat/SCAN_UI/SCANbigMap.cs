

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
		private SCANmap bigmap, spotmap;
		private Vessel v;
		private CelestialBody b;
		private SCANdata data;
		private double startUT;
		private bool bigmap_dragging, overlay_static_dirty, notMappingToday;
		private float bigmap_drag_w, bigmap_drag_x;
		private Texture2D overlay_static;
		private Rect rc = new Rect(0, 0, 0, 0);
		private Rect pos_spotmap = new Rect(10f, 10f, 10f, 10f);
		private Rect pos_spotmap_x = new Rect(10f, 10f, 25f, 25f);

		private float fps_time_passed, fps, fps_sum, fps_frames;

		protected override void Awake()
		{
			WindowCaption = string.Format("Map of {0}", b.theName);
			WindowRect = new Rect(250, 55, 380, 180);
			WindowOptions = new GUILayoutOption[2] { GUILayout.Width(360), GUILayout.Height(180) };
			WindowStyle = SCANskins.SCAN_window;
			Visible = true;
			DragEnabled = true;

			SCAN_SkinsLibrary.SetCurrent("SCAN_Unity");
		}

		internal override void Start()
		{
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
				SCANcontroller.controller.map_x = (int)WindowRect.x;
				SCANcontroller.controller.map_y = (int)WindowRect.y;
			}
			bigmap.setBody(FlightGlobals.currentMainBody);
		}

		internal override void OnDestroy()
		{

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
			growS();
			growE(); // added to put colors on the right of map

			Texture2D map = bigmap.getPartialMap(); // this is the expensive call

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

			if (overlay_static == null)
			{
				overlay_static = new Texture2D((int)bigmap.mapwidth, (int)bigmap.mapheight, TextureFormat.ARGB32, false);
				overlay_static_dirty = true;
			}

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

			if (bigmap.projection == SCANmap.MapProjection.Polar)
			{
				rc.x = maprect.x + maprect.width / 2 - maprect.width / 8;
				rc.y = maprect.y + maprect.height / 8;
				SCANuiUtil.drawLabel(rc, "S", true, true, false);
				rc.x = maprect.x + maprect.width / 2 + maprect.width / 8;
				SCANuiUtil.drawLabel(rc, "N", true, true, false);
			}

			if (SCANcontroller.controller.map_orbit && !notMappingToday)
			{
				SCANuiUtil.drawOrbit(maprect, bigmap, v, startUT, overlay_static);
			}

			stopE(); // draw colors before here
			growE(GUILayout.ExpandWidth(true));
			growE(GUILayout.Width(300));

			#region buttons: close / export png
			growS();
			if (GUILayout.Button("Close", SCANskins.SCAN_buttonFixed))
			{
				Visible = false;
			}

			GUILayout.FlexibleSpace();
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
			if (GUILayout.Button("Grey"))
			{
				SCANcontroller.controller.colours = 1;
				data.resetImages();
				bigmap.resetMap();
			}
			if (GUILayout.Button("Colour"))
			{
				SCANcontroller.controller.colours = 0;
				data.resetImages();
				bigmap.resetMap();
			}
			if (GUILayout.Button("Legend"))
			{
				SCANcontroller.controller.legend = !SCANcontroller.controller.legend;
			}
			#endregion
			stopS();
			growS();
			#region buttons: altimetry / slope / biome
			if (GUILayout.Button("Altimetry"))
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
			#endregion
			stopS();
			growS();
			growE();
			#region buttons: markers / flags / orbit / asteroids
			if (GUILayout.Button("Markers"))
			{
				SCANcontroller.controller.map_markers = !SCANcontroller.controller.map_markers;
			}
			if (GUILayout.Button("Flags"))
			{
				SCANcontroller.controller.map_flags = !SCANcontroller.controller.map_flags;
			}
			stopE();
			growE();

			if (GUILayout.Button("Orbit"))
			{
				SCANcontroller.controller.map_orbit = !SCANcontroller.controller.map_orbit;
			}

			if (GUILayout.Button("Asteroids"))
			{
				SCANcontroller.controller.map_asteroids = !SCANcontroller.controller.map_asteroids;
			}
			#endregion
			stopE();
			growE();
			#region buttons: grid / [resources]
			if (GUILayout.Button("Grid"))
			{
				SCANcontroller.controller.map_grid = !SCANcontroller.controller.map_grid;
				overlay_static_dirty = true;
			}

			if (SCANcontroller.controller.globalOverlay) //Button to turn on/off resource overlay
			{
				if (!SCANcontroller.controller.kethaneBusy || SCANcontroller.controller.resourceOverlayType == 0)
				{
					if (GUILayout.Button(SCANcontroller.ResourcesList[SCANcontroller.controller.gridSelection].name))
					{
						SCANcontroller.controller.map_ResourceOverlay = !SCANcontroller.controller.map_ResourceOverlay;
						bigmap.resetMap();
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
				if (GUILayout.Button(SCANmap.projectionNames[i]))
				{
					bigmap.setProjection((SCANmap.MapProjection)i);
					SCANcontroller.controller.projection = i;
					overlay_static_dirty = true;
				}
			}
			#endregion

			stopS();
			stopE();

			string info = "";
			float mx = Event.current.mousePosition.x - maprect.x;
			float my = Event.current.mousePosition.y - maprect.y;
			bool in_map = false, in_spotmap = false;
			double mlon = 0, mlat = 0;

			#region if (mouse inside bigmap)
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
					if (SCANUtil.isCovered(mlon, mlat, data, SCANdata.SCANtype.AltimetryLoRes))
					{
						if (v.mainBody.pqsController == null)
							info += palette.colored(palette.c_ugly, "LO ");
						else
							info += palette.colored(palette.c_good, "LO ");
					}
					else
						info += "<color=\"grey\">LO</color> ";
					if (SCANUtil.isCovered(mlon, mlat, data, SCANdata.SCANtype.AltimetryHiRes))
					{
						if (v.mainBody.pqsController == null)
							info += palette.colored(palette.c_ugly, "HI ");
						else
							info += palette.colored(palette.c_good, "HI ");
					}
					else
						info += "<color=\"grey\">HI</color> ";
					if (SCANUtil.isCovered(mlon, mlat, data, SCANdata.SCANtype.Biome))
					{
						if (v.mainBody.BiomeMap == null || v.mainBody.BiomeMap.Map == null)
							info += palette.colored(palette.c_ugly, "BIO ");
						else
							info += palette.colored(palette.c_good, "BIO ");
					}
					else
						info += "<color=\"grey\">BIO</color> ";
					if (SCANUtil.isCovered(mlon, mlat, data, SCANdata.SCANtype.Anomaly))
						info += palette.colored(palette.c_good, "ANOM ");
					else
						info += "<color=\"grey\">ANOM</color> ";
					if (SCANUtil.isCovered(mlon, mlat, data, SCANdata.SCANtype.AnomalyDetail))
						info += palette.colored(palette.c_good, "BTDT ");
					else
						info += "<color=\"grey\">BTDT</color> ";
					if (SCANUtil.isCovered(mlon, mlat, data, SCANdata.SCANtype.AltimetryHiRes))
					{
						info += "<b>" + SCANUtil.getElevation(v.mainBody, mlon, mlat).ToString("N2") + "m</b> ";
					}
					else if (SCANUtil.isCovered(mlon, mlat, data, SCANdata.SCANtype.AltimetryLoRes))
					{
						info += "<b>~" + (((int)SCANUtil.getElevation(v.mainBody, mlon, mlat) / 500) * 500).ToString() + "m</b> ";
					}
					if (SCANUtil.isCovered(mlon, mlat, data, SCANdata.SCANtype.Biome))
					{
						info += SCANUtil.getBiomeName(v.mainBody, mlon, mlat) + " ";
					}
					info += "\n" + SCANuiUtil.toDMS(mlat, mlon) + " (lat: " + mlat.ToString("F2") + " lon: " + mlon.ToString("F2") + ") ";
					if (in_spotmap)
						info += " " + spotmap.mapscale.ToString("F1") + "x";
					if (SCANcontroller.controller.map_ResourceOverlay && SCANcontroller.controller.globalOverlay) //Adds selected resource amount to big map legend
					{
						if (SCANcontroller.controller.resourceOverlayType == 0 && SCANreflection.ORSXFound)
						{
							if (SCANUtil.isCovered(mlon, mlat, data, bigmap.resource.type))
							{
								double amount = SCANUtil.ORSOverlay(mlon, mlat, bigmap.body.flightGlobalsIndex, bigmap.resource.name);
								string label;
								if (bigmap.resource.linear) //Make sure that ORS values are handled correctly based on which scale type they use
									label = (amount * 100).ToString("N1") + " %";
								else
									label = (amount * 1000000).ToString("N1") + " ppm";
								info += palette.colored(bigmap.resource.fullColor, "\n<b>" + bigmap.resource.name + ": " + label + "</b>");
							}
						}
						else if (SCANcontroller.controller.resourceOverlayType == 1)
						{
							if (SCANUtil.isCovered(mlon, mlat, data, bigmap.resource.type))
							{
								double amount = data.kethaneValueMap[SCANUtil.icLON(mlon), SCANUtil.icLAT(mlat)];
								if (amount < 0) amount = 0d;
								info += palette.colored(bigmap.resource.fullColor, "\n<b>" + bigmap.resource.name + ": " + amount.ToString("N1") + "</b>");
							}
						}
					}
				}
				else
				{
					info += " " + mlat.ToString("F") + " " + mlon.ToString("F"); // uncomment for debugging projections
				}
			}
			#endregion

			#region bigmap mouseover info and legend
			if (maprect.width < 720)
			{
				stopE();
				SCANuiUtil.readableLabel(info, true);
				SCANuiUtil.drawLegend(bigmap);
			}
			else
			{
				growS();
				SCANuiUtil.readableLabel(info, true);
				SCANuiUtil.drawLegend(bigmap);
				stopS();
				stopE();
			}
			#endregion

			if (!notMappingToday)
				SCANuiUtil.drawMapLabels(maprect, v, bigmap, data);

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
					SCANuiUtil.drawMapLabels(pos_spotmap, v, spotmap, data);
				}
				pos_spotmap_x.x = pos_spotmap.x + pos_spotmap.width + 4;
				pos_spotmap_x.y = pos_spotmap.y;
				if (GUI.Button(pos_spotmap_x, SCANcontroller.controller.closeBox, SCANskins.SCAN_closeButton))
				{
					spotmap = null;
				}
			}
			#endregion
			#region big map fps counter, version label
			Rect fpswidget = new Rect(maprect.x + maprect.width - 32, maprect.y + maprect.height + 32, 32, 24);
			GUI.Label(fpswidget, fps.ToString("N1"));
			#endregion
			#region version label
			Rect versionLabel = new Rect(maprect.x + maprect.width - 44, maprect.y + maprect.height + 50, 54, 24);
			GUI.Label(versionLabel, SCANversions.SCANsatVersion);
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

		}

	}
}
