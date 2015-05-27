using System;
using System.Collections.Generic;
using System.Linq;
using SCANsat.SCAN_Data;
using SCANsat.SCAN_UI.UI_Framework;
using SCANsat.SCAN_Platform;
using UnityEngine;
using palette = SCANsat.SCAN_UI.UI_Framework.SCANpalette;

namespace SCANsat.SCAN_UI
{
	class SCANresourceSettings : SCAN_MBW
	{
		internal readonly static Rect defaultRect = new Rect(300, 200, 400, 300);
		private static Rect sessionRect = defaultRect;
		private int mapHeight = 256;
		private float transparency = 0f;
		private int interpolationScale = 8;
		private bool popup, warningResource;
		private Rect warningRect;

		protected override void Awake()
		{
			WindowCaption = "S.C.A.N. Resources Settings";
			WindowRect = sessionRect;
			WindowStyle = SCANskins.SCAN_window;
			WindowOptions = new GUILayoutOption[2] { GUILayout.Width(400), GUILayout.Height(300) };
			Visible = false;
			DragEnabled = true;
			ClampToScreenOffset = new RectOffset(-200, -200, -200, -200);

			SCAN_SkinsLibrary.SetCurrent("SCAN_Unity");
		}

		protected override void DrawWindow(int id)
		{
			versionLabel(id);				/* Standard version label and close button */
			closeBox(id);

			resourceController(id);
			resourceSettings(id);
			overlayOptions(id);

			warningBox(id);
		}

		protected override void DrawWindowPost(int id)
		{
			if (popup && Event.current.type == EventType.mouseDown && !warningRect.Contains(Event.current.mousePosition))
			{
				popup = false;
			}

			sessionRect = WindowRect;
		}

		//Draw the version label in the upper left corner
		private void versionLabel(int id)
		{
			Rect r = new Rect(4, 0, 50, 18);
			GUI.Label(r, SCANmainMenuLoader.SCANsatVersion, SCANskins.SCAN_whiteReadoutLabel);
		}

		//Draw the close button in the upper right corner
		private void closeBox(int id)
		{
			Rect r = new Rect(WindowRect.width - 20, 1, 18, 18);
			if (GUI.Button(r, SCANcontroller.controller.closeBox, SCANskins.SCAN_closeButton))
			{
				Visible = false;
			}
		}

		private void resourceController(int id)
		{
			if (SCANcontroller.controller.resourceOverlay == null)
				return;

			if (GUILayout.Button("Planetary Overlay Window"))
			{
				SCANcontroller.controller.resourceOverlay.Visible = !SCANcontroller.controller.resourceOverlay.Visible;
			}
		}

		private void resourceSettings(int id)
		{
			GUILayout.Label("Resource Settings", SCANskins.SCAN_headline);
			growE();
			SCANcontroller.controller.resourceBiomeLock = GUILayout.Toggle(SCANcontroller.controller.resourceBiomeLock, "Resource Biome Lock", SCANskins.SCAN_settingsToggle);
			SCANcontroller.controller.easyModeScanning = GUILayout.Toggle(SCANcontroller.controller.easyModeScanning, "Instant Resource Scan", SCANskins.SCAN_settingsToggle);
			stopE();
			growE();
			fillS();
			SCANcontroller.controller.needsNarrowBand = GUILayout.Toggle(SCANcontroller.controller.needsNarrowBand, "Zoom Requires Narrow Band Scanner", SCANskins.SCAN_settingsToggle);
			fillS();
			stopE();
			growE();
			fillS();
			SCANcontroller.controller.disableStockResource = GUILayout.Toggle(SCANcontroller.controller.disableStockResource, "Disable Stock Scanning", SCANskins.SCAN_settingsToggle);
			fillS();
			stopE();
			if (popup)
			{
				GUILayout.Label("Reset Resource Coverage", SCANskins.SCAN_button);
			}
			else
			{
				if (GUILayout.Button("Reset Resource Coverage"))
				{
					popup = !popup;
					warningResource = !warningResource;
				}
			}
		}

		private void overlayOptions(int id)
		{
			growE();
			GUILayout.Label("Interpolation:", SCANskins.SCAN_labelSmallLeft);

			if (GUILayout.Button("-", SCANskins.SCAN_buttonSmall, GUILayout.Width(15)))
			{
				interpolationScale = Math.Max(2, interpolationScale / 2);
				refreshMap();
			}
			GUILayout.Label(interpolationScale.ToString(), SCANskins.SCAN_labelSmall);
			if (GUILayout.Button("+", SCANskins.SCAN_buttonSmall, GUILayout.Width(15)))
			{
				interpolationScale = Math.Min(32, interpolationScale * 2);
				refreshMap();
			}
			stopE();

			growE();
			GUILayout.Label("Map Height:", SCANskins.SCAN_labelSmallLeft);

			if (GUILayout.Button("-", SCANskins.SCAN_buttonSmall, GUILayout.Width(15)))
			{
				mapHeight = Math.Max(64, mapHeight / 2);
				refreshMap();
			}
			GUILayout.Label(mapHeight.ToString(), SCANskins.SCAN_labelSmall);
			if (GUILayout.Button("+", SCANskins.SCAN_buttonSmall, GUILayout.Width(15)))
			{
				mapHeight = Math.Min(1024, mapHeight * 2);
				refreshMap();
			}
			stopE();

			if (GUILayout.Button("Refresh"))
				refreshMap();
		}

		//Confirmation boxes for map resets
		private void warningBox(int id)
		{
			if (popup)
			{
				if (warningResource)
				{
					CelestialBody thisBody = null;
					switch (HighLogic.LoadedScene)
					{
						case GameScenes.FLIGHT:
							thisBody = FlightGlobals.currentMainBody;
							break;
						case GameScenes.SPACECENTER:
							thisBody = Planetarium.fetch.Home;
							break;
						case GameScenes.TRACKSTATION:
							thisBody = SCANUtil.getTargetBody(MapView.MapCamera.target);
							break;
						default:
							thisBody = null;
							break;
					}

					if (thisBody == null)
					{
						popup = false;
						return;
					}
					warningRect = new Rect(WindowRect.width - (WindowRect.width / 2) - 150, WindowRect.height - 125, 300, 90);
					GUI.Box(warningRect, "");
					Rect r = new Rect(warningRect.x + 10, warningRect.y + 5, 280, 40);
					GUI.Label(r, "Erase resource data for " + thisBody.theName + "?", SCANskins.SCAN_headlineSmall);
					r.x += 90;
					r.y += 45;
					r.width = 80;
					r.height = 30;
					if (GUI.Button(r, "Confirm", SCANskins.SCAN_buttonWarning))
					{
						popup = false;
						warningResource = false;
						SCANdata data = SCANUtil.getData(thisBody);
						if (data != null)
							data.resetResources();
					}
				}
				else
					popup = false;
			}
		}

		private void refreshMap()
		{
			if (SCANcontroller.controller.resourceOverlay == null)
				return;

			SCANcontroller.controller.resourceOverlay.refreshMap(transparency, mapHeight, interpolationScale);
		}


	}
}
