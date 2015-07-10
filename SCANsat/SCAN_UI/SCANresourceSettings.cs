#region license
/*  [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 *
 * SCANresourceSettings - Window for resource scanning options
 * 
 * Copyright (c)2013 damny;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 DMagic
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
 */
#endregion

using System;
using SCANsat.SCAN_Data;
using SCANsat.SCAN_UI.UI_Framework;
using SCANsat.SCAN_Platform;
using UnityEngine;
using palette = SCANsat.SCAN_UI.UI_Framework.SCANpalette;

namespace SCANsat.SCAN_UI
{
	class SCANresourceSettings : SCAN_MBW
	{
		internal readonly static Rect defaultRect = new Rect(300, 200, 300, 270);
		private static Rect sessionRect = defaultRect;
		private int mapHeight;
		private float transparency;
		private int interpolationScale;
		private bool popup, warningResource, warningStockResource, controlLock, oldNarrowBand;
		private const string lockID = "resourceSettingLockID";
		private Rect warningRect;

		protected override void Awake()
		{
			WindowCaption = "S.C.A.N. Resources Settings";
			WindowRect = sessionRect;
			WindowStyle = SCANskins.SCAN_window;
			WindowOptions = new GUILayoutOption[2] { GUILayout.Width(300), GUILayout.Height(270) };
			Visible = false;
			DragEnabled = true;
			ClampToScreenOffset = new RectOffset(-200, -200, -200, -200);

			SCAN_SkinsLibrary.SetCurrent("SCAN_Unity");
		}

		protected override void Start()
		{
			oldNarrowBand = SCANcontroller.controller.needsNarrowBand;

			mapHeight = SCANcontroller.controller.overlayMapHeight;
			transparency = SCANcontroller.controller.overlayTransparency;
			interpolationScale = SCANcontroller.controller.overlayInterpolation;
		}

		internal void removeControlLocks()
		{
			InputLockManager.RemoveControlLock(lockID);
			controlLock = false;
		}

		protected override void DrawWindowPre(int id)
		{
			//Lock space center click through
			if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
			{
				Vector2 mousePos = Input.mousePosition;
				mousePos.y = Screen.height - mousePos.y;
				if (WindowRect.Contains(mousePos) && !controlLock)
				{
					InputLockManager.SetControlLock(ControlTypes.CAMERACONTROLS | ControlTypes.KSC_ALL, lockID);
					controlLock = true;
				}
				else if (!WindowRect.Contains(mousePos) && controlLock)
				{
					removeControlLocks();
				}
			}

			//Lock tracking scene click through
			if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
			{
				Vector2 mousePos = Input.mousePosition;
				mousePos.y = Screen.height - mousePos.y;
				if (WindowRect.Contains(mousePos) && !controlLock)
				{
					InputLockManager.SetControlLock(ControlTypes.TRACKINGSTATION_UI, lockID);
					controlLock = true;
				}
				else if (!WindowRect.Contains(mousePos) && controlLock)
				{
					InputLockManager.RemoveControlLock(lockID);
					controlLock = false;
				}
			}

			if (!popup)
			{
				warningResource = false;
				warningStockResource = false;
			}
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

			if (oldNarrowBand != SCANcontroller.controller.needsNarrowBand)
			{
				oldNarrowBand = SCANcontroller.controller.needsNarrowBand;
				if (SCANcontroller.controller.instrumentsWindow != null && oldNarrowBand)
					SCANcontroller.controller.instrumentsWindow.resetResourceList();
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
				removeControlLocks();
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
			if (SCANcontroller.controller.disableStockResource)
				GUILayout.Toggle(false, "Instant Resource Scan", SCANskins.SCAN_settingsToggle);
			else
				SCANcontroller.controller.easyModeScanning = GUILayout.Toggle(SCANcontroller.controller.easyModeScanning, "Instant Resource Scan", SCANskins.SCAN_settingsToggle);
			stopE();
			growE();
			fillS();
			SCANcontroller.controller.needsNarrowBand = GUILayout.Toggle(SCANcontroller.controller.needsNarrowBand, "Requires Narrow Band Scanner", SCANskins.SCAN_settingsToggle);
			fillS();
			stopE();
			growE();
			fillS();
			SCANcontroller.controller.disableStockResource = GUILayout.Toggle(SCANcontroller.controller.disableStockResource, "Disable Stock Scanning", SCANskins.SCAN_settingsToggle);
			fillS();
			stopE();
			GUILayout.Label("Resource Scan Data", SCANskins.SCAN_headline);
			if (popup)
			{
				GUILayout.Label("Reset SCANsat Resource Coverage", SCANskins.SCAN_button);
				if (SCANcontroller.controller.disableStockResource)
				{
					fillS(8);
					GUILayout.Label("Reset Stock Resource Scanning", SCANskins.SCAN_button);
				}
			}
			else
			{
				if (GUILayout.Button("Reset SCANsat Resource Coverage"))
				{
					popup = !popup;
					warningResource = !warningResource;
				}
				if (SCANcontroller.controller.disableStockResource)
				{
					fillS(8);
					if (GUILayout.Button("Reset Stock Resource Scanning"))
					{
						popup = !popup;
						warningStockResource = !warningStockResource;
					}
				}
			}
		}

		private void overlayOptions(int id)
		{
			GUILayout.Label("Overlay Map Quality", SCANskins.SCAN_headline);
			growE();
				GUILayout.Label("Interpolation:", SCANskins.SCAN_labelSmallLeft);

				fillS();

				if (GUILayout.Button("-", SCANskins.SCAN_buttonSmall, GUILayout.Width(18)))
				{
					interpolationScale = Math.Max(2, interpolationScale / 2);
					refreshMap();
				}
				GUILayout.Label(interpolationScale.ToString(), SCANskins.SCAN_labelSmall, GUILayout.Width(36));
				if (GUILayout.Button("+", SCANskins.SCAN_buttonSmall, GUILayout.Width(18)))
				{
					interpolationScale = Math.Min(32, interpolationScale * 2);
					refreshMap();
				}
			stopE();

			growE();
				GUILayout.Label("Map Height:", SCANskins.SCAN_labelSmallLeft);

				fillS();

				if (GUILayout.Button("-", SCANskins.SCAN_buttonSmall, GUILayout.Width(18)))
				{
					mapHeight = Math.Max(64, mapHeight / 2);
					refreshMap();
				}
				GUILayout.Label(mapHeight.ToString(), SCANskins.SCAN_labelSmall, GUILayout.Width(36));
				if (GUILayout.Button("+", SCANskins.SCAN_buttonSmall, GUILayout.Width(18)))
				{
					mapHeight = Math.Min(1024, mapHeight * 2);
					refreshMap();
				}
			stopE();

			growE();
				GUILayout.Label("Coverage Transparency:", SCANskins.SCAN_labelSmallLeft);

				fillS();

				if (GUILayout.Button("-", SCANskins.SCAN_buttonSmall, GUILayout.Width(18)))
				{
					transparency = Mathf.Max(0f, transparency - 0.1f);
					refreshMap();
				}
				GUILayout.Label(transparency.ToString("P0"), SCANskins.SCAN_labelSmall, GUILayout.Width(36));
				if (GUILayout.Button("+", SCANskins.SCAN_buttonSmall, GUILayout.Width(18)))
				{
					transparency = Mathf.Min(1f, transparency + 0.1f);
					refreshMap();
				}
			stopE();
		}

		//Confirmation boxes for map resets
		private void warningBox(int id)
		{
			if (popup)
			{
				if (warningResource)
				{
					CelestialBody thisBody = getTargetBody();

					if (thisBody == null)
					{
						popup = false;
						return;
					}
					warningRect = new Rect(WindowRect.width - (WindowRect.width / 2) - 150, WindowRect.height - 125, 300, 90);
					GUI.Box(warningRect, "");
					Rect r = new Rect(warningRect.x + 10, warningRect.y + 5, 280, 40);
					GUI.Label(r, "Erase SCANsat resource data for " + thisBody.theName + "?", SCANskins.SCAN_headlineSmall);
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
				else if (warningStockResource)
				{
					CelestialBody thisBody = getTargetBody();

					if (thisBody == null)
					{
						popup = false;
						return;
					}

					warningRect = new Rect(WindowRect.width - (WindowRect.width / 2) - 150, WindowRect.height - 125, 300, 110);
					GUI.Box(warningRect, "");
					Rect r = new Rect(warningRect.x + 10, warningRect.y + 5, 280, 60);
					GUI.Label(r, "Erase stock resource data for " + thisBody.theName + "?", SCANskins.SCAN_headlineSmall);
					r.x += 90;
					r.y += 65;
					r.width = 80;
					r.height = 30;
					if (GUI.Button(r, "Confirm", SCANskins.SCAN_buttonWarning))
					{
						popup = false;
						warningStockResource = false;
						var resources = ResourceScenario.Instance.gameSettings.GetPlanetScanInfo();
						resources.RemoveAll(a => a.PlanetId == thisBody.flightGlobalsIndex);
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

		private CelestialBody getTargetBody()
		{
			switch (HighLogic.LoadedScene)
			{
				case GameScenes.FLIGHT:
					return FlightGlobals.currentMainBody;
				case GameScenes.SPACECENTER:
					return Planetarium.fetch.Home;
				case GameScenes.TRACKSTATION:
					return SCANUtil.getTargetBody(MapView.MapCamera.target);
				default:
					return null;
			}
		}


	}
}
