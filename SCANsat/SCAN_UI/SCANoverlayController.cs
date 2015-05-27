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
	class SCANoverlayController : SCAN_MBW
	{
		internal readonly static Rect defaultRect = new Rect(Screen.width - 280, 200, 200, 300);
		private static Rect sessionRect = defaultRect;
		private CelestialBody body;
		private SCANdata data;
		private SCANresourceGlobal currentResource;
		private List<SCANresourceGlobal> resources;
		private List<PResource.Resource> resourceFractions;
		private bool resourceMode = false;
		private bool drawOverlay = false;
		private bool oldOverlay = false;

		private Texture2D mapOverlay;
		private Texture2D biomeOverlay;
		private int mapHeight = 256;
		private float transparency = 0f;
		private int interpolationScale = 8;

		protected override void Awake()
		{
			WindowCaption = "S.C.A.N. Planet Overlay";
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

			setBody(HighLogic.LoadedSceneIsFlight ? FlightGlobals.currentMainBody : Planetarium.fetch.Home);
		}

		protected override void Update()
		{
			if ((MapView.MapIsEnabled && HighLogic.LoadedSceneIsFlight && FlightGlobals.ready) || HighLogic.LoadedScene == GameScenes.TRACKSTATION)
			{
				CelestialBody mapBody = SCANUtil.getTargetBody(MapView.MapCamera.target);

				if (mapBody == null)
					return;

				if (mapBody != body)
					setBody(mapBody);
			}
			else if (HighLogic.LoadedSceneIsFlight && FlightGlobals.ready)
			{
				if (body != FlightGlobals.currentMainBody)
					setBody(FlightGlobals.currentMainBody);
			}
		}

		protected override void OnDestroy()
		{

		}

		public bool DrawOverlay
		{
			get { return resourceMode; }
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
			if (oldOverlay != drawOverlay)
			{
				oldOverlay = drawOverlay;
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
				if (GUILayout.Button(r.Name, (r == currentResource && resourceMode) ? SCANskins.SCAN_labelLeftActive : SCANskins.SCAN_labelLeft))
				{
					resourceMode = true;
					if (currentResource == r)
					{
						OverlayGenerator.Instance.ClearDisplay();
						oldOverlay = drawOverlay = false;
					}
					else
					{
						currentResource = r;
						currentResource.CurrentBodyConfig(body.name);

						OverlayGenerator.Instance.ClearDisplay();
						oldOverlay = drawOverlay = true;
						refreshMap();
					}
				}

				//if (GUILayout.Button(r.CurrentBody.Fraction.ToString("P1"), SCANskins.SCAN_labelRight))
				//{
				//	currentResource = r;
				//	currentResource.CurrentBodyConfig(body.name);
				//}
				stopE();
			}

			if (GUILayout.Button("Biome Map", (!resourceMode && drawOverlay) ? SCANskins.SCAN_labelLeftActive : SCANskins.SCAN_labelLeft))
			{
				resourceMode = false;
				oldOverlay = drawOverlay = true;
				refreshMap();
			}
		}

		private void overlayToggle(int id)
		{
			drawOverlay = GUILayout.Toggle(drawOverlay, "Draw Overlay", SCANskins.SCAN_settingsToggle);
		}

		private void overlayOptions(int id)
		{
			if (resourceMode)
			{
				growE();
				GUILayout.Label("Coverage Transparency:", SCANskins.SCAN_labelSmallLeft);

				if (GUILayout.Button("-", SCANskins.SCAN_buttonSmall, GUILayout.Width(15)))
				{
					transparency = Mathf.Max(0f, transparency - 0.1f);
					refreshMap();
				}
				GUILayout.Label(transparency.ToString("P0"), SCANskins.SCAN_labelSmall);
				if (GUILayout.Button("+", SCANskins.SCAN_buttonSmall, GUILayout.Width(15)))
				{
					transparency = Mathf.Min(1f, transparency + 0.1f);
					refreshMap();
				}
				stopE();

				if (GUILayout.Button("Refresh"))
					refreshMap();
			}
		}

		public void refreshMap(float t, int height, int interp)
		{
			transparency = t;
			mapHeight = height;
			interpolationScale = interp;
			if (drawOverlay)
				refreshMap();
		}

		private void refreshMap()
		{
			if (!resourceMode)
				body.SetResourceMap(SCANuiUtil.drawBiomeMap(biomeOverlay, data, transparency));
			else
				body.SetResourceMap(SCANuiUtil.drawResourceTexture(mapOverlay, mapHeight, data, currentResource, interpolationScale, transparency));
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

			if (currentResource == null)
			{
				if (resources.Count > 0)
				{
					currentResource = resources[0];
					currentResource.CurrentBodyConfig(body.name);
				}
			}
			else
			{
				currentResource.CurrentBodyConfig(body.name);
			}

			if (drawOverlay)
				refreshMap();


			//resourceFractions = ResourceMap.Instance.GetResourceItemList(HarvestTypes.Planetary, body);
			//if (resources.Count > 0)
			//{
			//	currentResource = resources[0];
			//	currentResource.CurrentBodyConfig(body.name);

			//	//foreach (SCANresourceGlobal r in resources)
			//	//{
			//	//	SCANresourceBody b = r.getBodyConfig(body.name, false);
			//	//	if (b != null)
			//	//	{
			//	//		b.Fraction = resourceFractions.FirstOrDefault(a => a.resourceName == r.Name).fraction;
			//	//	}
			//	//}
			//}
		}
	}
}
