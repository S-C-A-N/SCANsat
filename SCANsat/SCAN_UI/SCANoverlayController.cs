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
		internal readonly static Rect defaultRect = new Rect(Screen.width - 280, 200, 200, 260);
		private static Rect sessionRect = defaultRect;
		private CelestialBody body;
		private SCANdata data;
		private SCANresourceGlobal currentResource;
		private List<SCANresourceGlobal> resources;
		private List<PResource.Resource> resourceFractions;
		private bool biomeMode = false;
		private bool drawOverlay = false;
		private bool oldOverlay = false;
		private int selection;

		private Texture2D mapOverlay;
		private Texture2D biomeOverlay;
		private Color32[] resourcePixels;
		private Color32[] biomePixels;
		private float[,] abundanceValues;
		private int mapHeight = 256;
		private float transparency = 0f;
		private int interpolationScale = 8;

		protected override void Awake()
		{
			WindowCaption = "S.C.A.N. Overlay";
			WindowRect = sessionRect;
			WindowStyle = SCANskins.SCAN_window;
			WindowOptions = new GUILayoutOption[2] { GUILayout.Width(200), GUILayout.Height(260) };
			Visible = false;
			DragEnabled = true;
			ClampToScreenOffset = new RectOffset(-140, -140, -200, -200);

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
			get { return drawOverlay; }
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
			resourceSettings(id);
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
			for (int i = 0; i < resources.Count; i++)
			{
				SCANresourceGlobal r = resources[i];

				if (r == null)
					continue;

				if (GUILayout.Button(r.Name, selection == i ? SCANskins.SCAN_labelLeftActive : SCANskins.SCAN_labelLeft))
				{
					biomeMode = false;
					OverlayGenerator.Instance.ClearDisplay();

					if (selection != i)
					{
						selection = i;
						currentResource = r;
						currentResource.CurrentBodyConfig(body.name);
						oldOverlay = drawOverlay = true;
						refreshMap();
						return;
					}

					if (drawOverlay)
					{
						oldOverlay = drawOverlay = false;
					}
					else
					{
						oldOverlay = drawOverlay = true;
						refreshMap();
					}
				}
			}

			if (GUILayout.Button("Biome Map", selection == (resources.Count) ? SCANskins.SCAN_labelLeftActive : SCANskins.SCAN_labelLeft))
			{
				biomeMode = true;

				OverlayGenerator.Instance.ClearDisplay();

				if (selection != resources.Count)
				{
					selection = resources.Count;
					oldOverlay = drawOverlay = true;
					refreshMap();
					return;
				}

				if (drawOverlay)
				{
					oldOverlay = drawOverlay = false;
				}
				else
				{
					oldOverlay = drawOverlay = true;
					refreshMap();
				}
			}
		}

		private void overlayToggle(int id)
		{
			drawOverlay = GUILayout.Toggle(drawOverlay, "Draw Overlay", SCANskins.SCAN_settingsToggle);
		}

		private void overlayOptions(int id)
		{
			if (!drawOverlay)
				return;

			if (GUILayout.Button("Refresh"))
				refreshMap();

			if (!biomeMode)
			{
				growE();
				GUILayout.Label("Coverage Transparency:", SCANskins.SCAN_labelSmallLeft);

				if (GUILayout.Button("-", SCANskins.SCAN_buttonSmall, GUILayout.Width(18)))
				{
					transparency = Mathf.Max(0f, transparency - 0.1f);
					refreshMap();
				}
				GUILayout.Label(transparency.ToString("P0"), SCANskins.SCAN_labelSmall);
				if (GUILayout.Button("+", SCANskins.SCAN_buttonSmall, GUILayout.Width(18)))
				{
					transparency = Mathf.Min(1f, transparency + 0.1f);
					refreshMap();
				}
				stopE();
			}
		}

		private void resourceSettings(int id)
		{
			fillS();
			if (GUILayout.Button("Resource Settings"))
			{
				SCANcontroller.controller.resourceSettings.Visible = !SCANcontroller.controller.resourceSettings.Visible;
			}

			//if (GUILayout.Button("Biome Summary"))
			//{
			//	foreach (ResourceCache.AbundanceSummary a in ResourceCache.Instance.AbundanceCache)
			//	{
			//		if (a.ResourceName == "Ore" && a.HarvestType == HarvestTypes.Planetary)
			//			SCANUtil.SCANlog("{0}: For {1} on Body {2} of scanner type {3}: Abundance = {4:P3}", a.ResourceName, a.BiomeName, a.BodyId, a.HarvestType, a.Abundance);
			//	}

				
			//}
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
			if (biomeMode)
				body.SetResourceMap(SCANuiUtil.drawBiomeMap(ref biomeOverlay, ref biomePixels, data, transparency, mapHeight * 2));
			else
				body.SetResourceMap(SCANuiUtil.drawResourceTexture(ref mapOverlay, ref resourcePixels, ref abundanceValues, mapHeight, data, currentResource, interpolationScale, transparency));
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
