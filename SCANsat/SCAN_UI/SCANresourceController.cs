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
	class SCANresourceController : SCAN_MBW
	{
		internal readonly static Rect defaultRect = new Rect(Screen.width - 280, 200, 200, 300);
		private static Rect sessionRect = defaultRect;
		private CelestialBody body;
		private SCANdata data;
		private SCANresourceGlobal currentResource;
		private List<SCANresourceGlobal> resources;
		private List<PResource.Resource> resourceFractions;
		private bool drawResourceOverlay = false;
		private bool oldOverlay = false;

		private int mapHeight = 256;
		private float transparency = 0f;
		private int interpolationScale = 8;

		protected override void Awake()
		{
			WindowCaption = "S.C.A.N. Resources";
			WindowRect = sessionRect;
			WindowStyle = SCANskins.SCAN_window;
			WindowOptions = new GUILayoutOption[2] { GUILayout.Width(200), GUILayout.Height(300) };
			Visible = false;
			DragEnabled = true;
			ClampToScreenOffset = new RectOffset(-200, -200, -40, -40);

			SCAN_SkinsLibrary.SetCurrent("SCAN_Unity");
		}

		protected override void Start()
		{
			resources = SCANcontroller.setLoadedResourceList();

			setBody(FlightGlobals.currentMainBody);
		}

		protected override void Update()
		{
			if (MapView.MapIsEnabled)
			{
				MapObject target = PlanetariumCamera.fetch.target;

				CelestialBody mapBody = getTargetBody(target);
				if (mapBody == null)
					return;

				if (mapBody != body)
					setBody(mapBody);
			}
			else
			{
				if (body != FlightGlobals.currentMainBody)
					setBody(FlightGlobals.currentMainBody);
			}
		}

		protected override void OnDestroy()
		{

		}

		private CelestialBody getTargetBody(MapObject target)
		{
			if (target.type == MapObject.MapObjectType.CELESTIALBODY)
			{
				return target.celestialBody;
			}
			else if (target.type == MapObject.MapObjectType.MANEUVERNODE)
			{
				return target.maneuverNode.patch.referenceBody;
			}
			else if (target.type == MapObject.MapObjectType.VESSEL)
			{
				return target.vessel.mainBody;
			}

			return null;
		}

		protected override void DrawWindowPre(int id)
		{

		}

		protected override void DrawWindow(int id)
		{
			versionLabel(id);				/* Standard version label and close button */
			closeBox(id);

			drawResourceList(id);
			overlayToggle(id);
			overlayOptions(id);
		}

		protected override void DrawWindowPost(int id)
		{
			if (oldOverlay != drawResourceOverlay)
			{
				oldOverlay = drawResourceOverlay;
				if (oldOverlay)
					refreshMap();
				else
					OverlayGenerator.Instance.ClearDisplay();
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

		private void drawResourceList(int id)
		{
			foreach (SCANresourceGlobal r in resources)
			{
				growE();
				if (GUILayout.Button(r.Name, SCANskins.SCAN_labelLeft))
				{
					if (currentResource == r)
					{
						OverlayGenerator.Instance.ClearDisplay();
						oldOverlay = drawResourceOverlay = false;
					}
					else
					{
						currentResource = r;
						currentResource.CurrentBodyConfig(body.name);

						OverlayGenerator.Instance.ClearDisplay();
						refreshMap();
						oldOverlay = drawResourceOverlay = true;
					}
				}

				//if (GUILayout.Button(r.CurrentBody.Fraction.ToString("P1"), SCANskins.SCAN_labelRight))
				//{
				//	currentResource = r;
				//	currentResource.CurrentBodyConfig(body.name);
				//}
				stopE();
			}
		}

		private void overlayToggle(int id)
		{
			drawResourceOverlay = GUILayout.Toggle(drawResourceOverlay, "Draw Overlay", SCANskins.SCAN_settingsToggle);
		}

		private void overlayOptions(int id)
		{
			if (drawResourceOverlay)
			{
				growE();
				GUILayout.Label("Coverage Transparency:", SCANskins.SCAN_labelSmallLeft);

				if (GUILayout.Button("-", SCANskins.SCAN_labelSmall))
				{
					transparency = Mathf.Max(0f, transparency - 0.1f);
					refreshMap();
				}
				GUILayout.Label(transparency.ToString("P0"), SCANskins.SCAN_labelSmall);
				if (GUILayout.Button("+", SCANskins.SCAN_labelSmall))
				{
					transparency = Mathf.Min(1f, transparency + 0.1f);
					refreshMap();
				}
				stopE();

				growE();
				GUILayout.Label("Interpolation:", SCANskins.SCAN_labelSmallLeft);

				if (GUILayout.Button("-", SCANskins.SCAN_labelSmall))
				{
					interpolationScale = Math.Max(2, interpolationScale / 2);
					refreshMap();
				}
				GUILayout.Label(interpolationScale.ToString(), SCANskins.SCAN_labelSmall);
				if (GUILayout.Button("+", SCANskins.SCAN_labelSmall))
				{
					interpolationScale = Math.Min(32, interpolationScale * 2);
					refreshMap();
				}
				stopE();

				growE();
				GUILayout.Label("Map Height:", SCANskins.SCAN_labelSmallLeft);

				if (GUILayout.Button("-", SCANskins.SCAN_labelSmall))
				{
					mapHeight = Math.Max(64, mapHeight / 2);
					refreshMap();
				}
				GUILayout.Label(mapHeight.ToString(), SCANskins.SCAN_labelSmall);
				if (GUILayout.Button("+", SCANskins.SCAN_labelSmall))
				{
					mapHeight = Math.Min(1024, mapHeight * 2);
					refreshMap();
				}
				stopE();

				if (GUILayout.Button("Refresh"))
					refreshMap();
			}
		}

		private void refreshMap()
		{
			body.SetResourceMap(SCANuiUtil.drawResourceTexture(mapHeight, data, currentResource, interpolationScale, transparency));
		}

		private void setBody(CelestialBody B)
		{
			body = B;
			data = SCANUtil.getData(body);
			if (data == null)
			{
				data = new SCANdata(body);
				SCANcontroller.controller.addToBodyData(body, data);
			}
			//resourceFractions = ResourceMap.Instance.GetResourceItemList(HarvestTypes.Planetary, body);
			if (resources.Count > 0)
			{
				currentResource = resources[0];
				currentResource.CurrentBodyConfig(body.name);

				//foreach (SCANresourceGlobal r in resources)
				//{
				//	SCANresourceBody b = r.getBodyConfig(body.name, false);
				//	if (b != null)
				//	{
				//		b.Fraction = resourceFractions.FirstOrDefault(a => a.resourceName == r.Name).fraction;
				//	}
				//}
			}

			oldOverlay = drawResourceOverlay = false;
		}
	}
}
