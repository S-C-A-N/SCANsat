using SCANsat;
using UnityEngine;
using System;
using System.Collections.Generic;
using JSI;
using System.Globalization;

namespace SCANsatRPM
{
	public class JSISCANsatRPM: InternalModule
	{
		[KSPField]
		public int buttonUp;
		[KSPField]
		public int buttonDown = 1;
		[KSPField]
		public int buttonEnter = 2;
		[KSPField]
		public int buttonEsc = 3;
		[KSPField]
		public int buttonHome = 4;
		[KSPField]
		public int maxZoom = 20;
		[KSPField]
		public float iconPixelSize = 8f;
		[KSPField]
		public Vector2 iconShadowShift = new Vector2(1, 1);
		[KSPField]
		public float redrawEdge = 0.8f;
		[KSPField]
		public string iconColorSelf = string.Empty;
		private Color iconColorSelfValue = new Color(1f, 1f, 1f, 0.6f);
		[KSPField]
		public string iconColorTarget = string.Empty;
		private Color iconColorTargetValue = new Color32(255, 235, 4, 153);
		[KSPField]
		public string iconColorUnvisitedAnomaly = string.Empty;
		private Color iconColorUnvisitedAnomalyValue = new Color(1f, 0f, 0f, 0.5f);
		[KSPField]
		public string iconColorVisitedAnomaly = string.Empty;
		private Color iconColorVisitedAnomalyValue = new Color(0f, 1f, 0f, 0.5f);
		[KSPField]
		public string iconColorShadow = string.Empty;
		private Color iconColorShadowValue = new Color(0f, 0f, 0f, 0.5f);
		[KSPField]
		public string iconColorAP = string.Empty;
		private Color iconColorAPValue = new Color(0f, 1f, 0f, 0.5f);
		[KSPField]
		public string iconColorPE = string.Empty;
		private Color iconColorPEValue = new Color(1f, 0f, 0f, 0.5f);
		[KSPField]
		public string iconColorNode = string.Empty;
		private Color iconColorNodeValue = XKCDColors.Aqua;
		[KSPField]
		public string iconColorANDN = string.Empty;
		private Color iconColorANDNValue = XKCDColors.Mauve;
		[KSPField]
		public string trailColor = string.Empty;
		private Color trailColorValue = new Color(0f, 0f, 1f, 0.6f);
		[KSPField]
		public float zoomModifier = 1.5f;
		[KSPField]
		public string scaleBar;
		[KSPField]
		public string scaleLabels;
		[KSPField]
		public string scaleLevels = "500000,200000,100000,50000,20000,10000,5000,1000";
		[KSPField]
		public Vector2 scaleBarPosition = new Vector2(16, 16);
		[KSPField]
		public float scaleBarSizeLimit = 512 / 2 - 16;
		[KSPField]
		public int trailLimit = 100;
		[KSPField]
		public float trailPointEvery = 30;
		[KSPField]
		public int orbitPoints = 120;
		// That ends our glut of configurable values.
		private bool showLines;
		private int mapMode;
		private int zoomLevel;
		private int screenWidth;
		private int screenHeight;
		private Vector2d mapSizeScale;
		private double mapCenterLong, mapCenterLat;
		private SCANmap map;
		private CelestialBody orbitingBody;
		private Vessel targetVessel;
		private double redrawDeviation;
		private SCANdata.SCANanomaly[] localAnomalies;
		private Material iconMaterial;
		private PersistenceAccessor persistence;
		private string persistentVarName;
		private double pixelsPerKm;
		private Texture2D scaleBarTexture, scaleLabelTexture;
		private float[] scaleLevelValues;
		private float scaleLabelSpan;
		private readonly List<Vector2d> trail = new List<Vector2d>();
		private static Material trailMaterial;
		private double trailCounter;
		private Rect screenSpace;
		private bool pageActiveState;
		private double start;
		private readonly List<MapMarkupLine> mapMarkup = new List<MapMarkupLine>();
		private readonly Color scaleTint = new Color(0.5f, 0.5f, 0.5f, 0.5f);
		// Neutral tint.
		private bool satFound;
		private bool startupComplete;

		private bool TestForActiveSCANsat()
		{
			if (satFound)
				return true;
			foreach (ScenarioModule thatScenario in ScenarioRunner.GetLoadedModules()) {
				if (thatScenario.ClassName == "SCANcontroller") {
					satFound = true;
					return true;
				}
			}
			return false;
		}
		// Analysis disable once UnusedParameter
		public bool MapRenderer(RenderTexture screen, float cameraAspect)
		{
			// Just in case.
			if (!HighLogic.LoadedSceneIsFlight)
				return false;

			if (!TestForActiveSCANsat())
				return false;

			if (!startupComplete)
				JUtil.AnnoyUser(this);

			if (screenWidth == 0 || screenHeight == 0) {
				int? loadedMode = persistence.GetVar(persistentVarName + "mode");
				mapMode = loadedMode ?? 0;
				int? loadedZoom = persistence.GetVar(persistentVarName + "zoom");
				zoomLevel = loadedZoom ?? 0;
				int? loadedColors = persistence.GetVar(persistentVarName + "color");
				SCANcontroller.controller.colours = loadedColors ?? 0;
				screenWidth = screen.width;
				screenHeight = screen.height;
				iconMaterial = new Material(Shader.Find("KSP/Alpha/Unlit Transparent"));

				screenSpace = new Rect(0, 0, screenWidth, screenHeight);

				RedrawMap();

				return false;
			}

			// In case SCANsat is present but not working, we can't run any of our code. :(
			if (map == null) {
				return false;
			}

			start = Planetarium.GetUniversalTime();

			Graphics.Blit(map.map, screen);
			GL.PushMatrix();
			GL.LoadPixelMatrix(0, screenWidth, screenHeight, 0);

			// Markup lines are the lowest layer.
			if (showLines)
				foreach (MapMarkupLine vectorLine in mapMarkup) {
					if (vectorLine.body == orbitingBody && vectorLine.points.Count > 0) {
						DrawTrail(vectorLine.points, vectorLine.color, Vector2d.zero);
					}
				}

			// Trails go above markup lines
			if (showLines && trailLimit > 0 && trail.Count > 0)
				DrawTrail(trail, trailColorValue, new Vector2d(vessel.longitude, vessel.latitude), true);
		
			// Anomalies go above trails
			foreach (SCANdata.SCANanomaly anomaly in localAnomalies) {
				if (anomaly.known)
					DrawIcon(anomaly.longitude, anomaly.latitude,
						anomaly.detail ? (VesselType)int.MaxValue : VesselType.Unknown,
						anomaly.detail ? iconColorVisitedAnomalyValue : iconColorUnvisitedAnomalyValue);
			}
			// Target orbit and targets go above anomalies
			if (targetVessel != null && targetVessel.mainBody == orbitingBody) {
				if (showLines && JUtil.OrbitMakesSense(targetVessel)) {
					DrawOrbit(targetVessel, targetVessel.orbit, start, iconColorTargetValue);
					// Connect our orbit and the target orbit with a line at the point of closest approach...
					if (JUtil.OrbitMakesSense(vessel)) {
						double closestApproachMoment;
						JUtil.GetClosestApproach(vessel.orbit,targetVessel.orbit, out closestApproachMoment);
						Vector2d targetCoord, vesselCoord;
						bool targetCollision, vesselCollision;
						// Analysis disable once CompareOfFloatsByEqualityOperator
						if (closestApproachMoment != start &&
						    GetPositionAtT(targetVessel, targetVessel.orbit, start, closestApproachMoment, out targetCoord, out targetCollision) && !targetCollision &&
						    GetPositionAtT(vessel, targetVessel.orbit, start, closestApproachMoment, out vesselCoord, out vesselCollision) && !vesselCollision) {
							var endpoints = new List<Vector2d>();
							endpoints.Add(targetCoord);
							endpoints.Add(vesselCoord);
							DrawTrail(endpoints, iconColorTargetValue, Vector2d.zero);
						}
					}
				}
				DrawIcon(targetVessel.longitude, targetVessel.latitude, targetVessel.vesselType, iconColorTargetValue);
				if (showLines) {
					DrawOrbitIcon(targetVessel, MapIcons.OtherIcon.AP, iconColorTargetValue);
					DrawOrbitIcon(targetVessel, MapIcons.OtherIcon.PE, iconColorTargetValue);
				}


			}
			// Own orbit goes above that.
			if (showLines && JUtil.OrbitMakesSense(vessel)) {
				DrawOrbit(vessel, vessel.orbit, start, iconColorSelfValue);
				DrawOrbitIcon(vessel, MapIcons.OtherIcon.AP, iconColorAPValue);
				DrawOrbitIcon(vessel, MapIcons.OtherIcon.PE, iconColorPEValue);
				if (targetVessel != null && JUtil.OrbitMakesSense(targetVessel)) {
					if (vessel.orbit.AscendingNodeExists(targetVessel.orbit))
						DrawOrbitIcon(vessel, MapIcons.OtherIcon.AN, iconColorANDNValue, vessel.orbit.TimeOfAscendingNode(targetVessel.orbit, start));
					if (vessel.orbit.DescendingNodeExists(targetVessel.orbit))
						DrawOrbitIcon(vessel, MapIcons.OtherIcon.DN, iconColorANDNValue, vessel.orbit.TimeOfDescendingNode(targetVessel.orbit, start));
				}
				// And the maneuver node and post-maneuver orbit: 
				ManeuverNode node = vessel.patchedConicSolver.maneuverNodes.Count > 0 ? vessel.patchedConicSolver.maneuverNodes[0] : null;
				if (node != null) {
					DrawOrbit(vessel, node.nextPatch, node.UT, iconColorNodeValue);
					DrawOrbitIcon(vessel, MapIcons.OtherIcon.NODE, iconColorNodeValue, node.UT);
				}
			}
			// Own icon goes above that
			DrawIcon(vessel.longitude, vessel.latitude, vessel.vesselType, iconColorSelfValue);
			// And scale goes above everything.
			DrawScale();

			GL.PopMatrix();

			return true;
		}

		private void DrawOrbitIcon(Vessel thatVessel, MapIcons.OtherIcon iconType, Color iconColor, double givenPoint = 0)
		{
			double timePoint = start;
			switch (iconType) {
				case MapIcons.OtherIcon.AP:
					timePoint += thatVessel.orbit.timeToAp;
					break;
				case MapIcons.OtherIcon.PE:
					timePoint += thatVessel.orbit.timeToPe;
					break;
				case MapIcons.OtherIcon.AN:
				case MapIcons.OtherIcon.DN:
				case MapIcons.OtherIcon.NODE:
					timePoint = givenPoint;
					break;
			}

			if (JUtil.OrbitMakesSense(thatVessel)) {
				bool collision;
				Vector2d coord;
				if (GetPositionAtT(thatVessel, thatVessel.orbit, start, timePoint, out coord, out collision) && !collision) {
					DrawIcon(coord.x, coord.y, thatVessel.vesselType, iconColor, iconType);
				}
			}
		}

		private static bool GetPositionAtT(Vessel thatVessel, Orbit thatOrbit, double initial, double timePoint, out Vector2d coordinates, out bool collision)
		{
			coordinates = Vector2d.zero;
			collision = false;
			if (double.IsNaN(thatOrbit.getObtAtUT(initial + timePoint)))
				return false;
			double rotOffset = 0;
			if (thatVessel.mainBody.rotates) {
				rotOffset = (360 * ((timePoint - initial) / thatVessel.mainBody.rotationPeriod)) % 360;
			}
			Vector3d pos = thatOrbit.getPositionAtUT(timePoint);
			if (thatOrbit.Radius(timePoint) < thatVessel.mainBody.Radius + thatVessel.mainBody.TerrainAltitude(pos)) {
				collision = true;
				return false;
			}
			coordinates = new Vector2d(thatVessel.mainBody.GetLongitude(pos) - rotOffset, thatVessel.mainBody.GetLatitude(pos));
			return true;
		}

		private void DrawOrbit(Vessel thatVessel, Orbit thatOrbit, double startMoment, Color32 thatColor)
		{
			if (orbitPoints == 0)
				return;
			double dTstep = Math.Floor(thatOrbit.period / orbitPoints);
			var points = new List<Vector2d>();
			for (double timePoint = startMoment; timePoint < (startMoment + thatOrbit.period); timePoint += dTstep) {
				bool collision;
				Vector2d coord;
				if (GetPositionAtT(thatVessel, thatOrbit, start, timePoint, out coord, out collision))
					points.Add(coord);
				if (collision)
					break;
			}
			DrawTrail(points, thatColor, Vector2d.zero);
		}

		private void DrawTrail(IList<Vector2d> points, Color32 lineColor, Vector2d endPoint, bool hasEndpoint = false)
		{
			if (points.Count < 2)
				return;
			GL.Begin(GL.LINES);
			trailMaterial.SetPass(0);
			GL.Color(lineColor);
			float xStart, yStart;
			// actualMapWidth is the width of the virtual map (accounting for
			// zoom level).  We use this value to determine if a particular
			// line segment wraps around from the right edge to the left edge.
			// We compute the value once here, instead of doing it every single
			// segment.
			float actualMapWidth = (float)mapSizeScale.x * screenWidth;

			xStart = (float)longitudeToPixels(points[0].x, points[0].y);
			yStart = (float)latitudeToPixels(points[0].x, points[0].y);
			for (int i = 1; i < points.Count; i++) {
				float xEnd = (float)longitudeToPixels(points[i].x, points[i].y);
				float yEnd = (float)latitudeToPixels(points[i].x, points[i].y);
				DrawLine(xStart, yStart, xEnd, yEnd, screenSpace, actualMapWidth);

				xStart = xEnd;
				yStart = yEnd;
			}
			if (hasEndpoint)
				DrawLine(xStart, yStart, (float)longitudeToPixels(endPoint.x, endPoint.y), (float)latitudeToPixels(endPoint.x, endPoint.y), screenSpace, actualMapWidth);
			GL.End();
		}

		private static void DrawLine(float xStart, float yStart, float xEnd, float yEnd, Rect screenSpace, float actualMapWidth)
		{
			var start = new Vector2(xStart, yStart);
			var end = new Vector2(xEnd, yEnd);

			if (!screenSpace.Contains(start) && !screenSpace.Contains(end))
				return;

			// We order these so we don't have to mess with absolute values.
			float leftX = Math.Min(start.x, end.x);
			float rightX = Math.Max(start.x, end.x);

			// MOARdV:
			// We treat the map as a cylinder here, since it is one as far as
			// the rectangular projection of it is concerned.
			// Compute the x component of the Manhattan distance between the
			// two points, and compare that to the distance if we move the
			// left end point one scaled map width to the right (moving it to
			// the right of the right end point).  If the repositioned point
			// is closer than the original point, we infer that the line
			// segment wraps around the edge of the map.  This will always be
			// true as long as we don't generate a line segment longer than
			// 1/2 of the map's width.  If the two points are diametrically
			// opposed to each other on the map, we don't have enough
			// information to guess which way is the right way, so do nothing.
			// The Manhattan distance of the original arrangement of points
			// is rightX - leftX.  The MD of the repositioned point is
			// (leftX + actualMapWidth) - rightX.  Move like terms and
			// divide by two, and here's what you get:
			if (leftX + actualMapWidth * 0.5f < rightX) {
				if (start.x < end.x) {
					end.x -= actualMapWidth;
				} else {
					start.x -= actualMapWidth;
				}
			}

			GL.Vertex(start);
			GL.Vertex(end);

		}

		private void DrawScale()
		{
			if (scaleBarTexture == null || scaleLabelTexture == null)
				return;

			var scaleBarRect = new Rect();
			scaleBarRect.x = scaleBarPosition.x;
			scaleBarRect.height = scaleLabelTexture.height / scaleLevelValues.Length;
			scaleBarRect.y = screenHeight - scaleBarPosition.y - scaleBarRect.height;

			int scaleID = 0;
			for (int i = scaleLevelValues.Length; i-- > 0;) {
				if (scaleLevelValues[i] * pixelsPerKm < scaleBarSizeLimit) {
					scaleBarRect.width = (float)(scaleLevelValues[i] * pixelsPerKm);
					scaleID = i;
					break;
				}
			}
			Graphics.DrawTexture(scaleBarRect, scaleBarTexture, new Rect(0, 0, 1f, 1f), 4, 4, 4, 4, scaleTint);

			scaleBarRect.x += scaleBarRect.width;
			scaleBarRect.width = scaleLabelTexture.width;
			Graphics.DrawTexture(scaleBarRect, scaleLabelTexture, new Rect(0f, scaleID * scaleLabelSpan, 1f, scaleLabelSpan), 0, 0, 0, 0, scaleTint);
		}

		private void DrawIcon(double longitude, double latitude, VesselType vt, Color iconColor, MapIcons.OtherIcon icon = MapIcons.OtherIcon.None)
		{
			var position = new Rect((float)(longitudeToPixels(longitude, latitude) - iconPixelSize / 2),
				               (float)(latitudeToPixels(longitude, latitude) - iconPixelSize / 2),
				               iconPixelSize, iconPixelSize);

			Rect shadow = position;
			shadow.x += iconShadowShift.x;
			shadow.y += iconShadowShift.y;

			iconMaterial.color = iconColorShadowValue;
			Graphics.DrawTexture(shadow, MapView.OrbitIconsMap, MapIcons.VesselTypeIcon(vt, icon), 0, 0, 0, 0, iconMaterial);

			iconMaterial.color = iconColor;
			Graphics.DrawTexture(position, MapView.OrbitIconsMap, MapIcons.VesselTypeIcon(vt, icon), 0, 0, 0, 0, iconMaterial);
		}

		private double longitudeToPixels(double longitude, double latitude)
		{
			return rescaleLongitude((map.projectLongitude(longitude, latitude) + 180d) % 360d) * (screenWidth / 360d);
		}

		private double latitudeToPixels(double longitude, double latitude)
		{
			// MOARdV:
			// I haven't thoroughly tested this, nor have I looked at the
			// source, but I believe map.project* are mapping the lat/lon
			// to a 360 x 180 2D surface that repeats in the X direction
			// like a cylinder.  On that assumption, we don't want to apply
			// the remainder operator to latitude - that would cause a value
			// of 181 to become 1, for instance, moving the point from one
			// pole to the other.  We have to translate the latitude by 90 to
			// put the baseline latitude in the range [0,180] instead of
			// [-90, +90].
			double projLat = map.projectLatitude(longitude, latitude);
			double translatedLat = 90.0 + projLat - map.lat_offset;
			double scaledLat = translatedLat * mapSizeScale.y;
			double pix = scaledLat * screenHeight / 180.0;

			// Invert the y value
			return screenHeight - pix;
		}

		private double rescaleLongitude(double lon)
		{
			return Clamp(lon - map.lon_offset, 360d) * mapSizeScale.x;
		}

		private static double Clamp(double value, double clamp)
		{
			value = value % clamp;
			if (value < 0)
				return value + clamp;
			return value;
		}

		public void ButtonProcessor(int buttonID)
		{
			if (screenWidth == 0 || screenHeight == 0)
				return;
			if (buttonID == buttonUp) {
				ChangeZoom(false);
			}
			if (buttonID == buttonDown) {
				ChangeZoom(true);
			}
			if (buttonID == buttonEnter) {
				ChangeMapMode(true);
			}
			if (buttonID == buttonEsc) {
				// Whatever possessed him to do THAT?
				SCANcontroller.controller.colours = SCANcontroller.controller.colours == 0 ? 1 : 0;
				persistence.SetVar(persistentVarName + "color", SCANcontroller.controller.colours);
				RedrawMap();
			}
			if (buttonID == buttonHome) {
				showLines = !showLines;
				persistence.SetVar(persistentVarName + "lines", showLines);
			}
		}

		private void ChangeMapMode(bool up)
		{
			mapMode += up ? 1 : -1;

			if (mapMode > 2)
				mapMode = 0;
			if (mapMode < 0)
				mapMode = 2;
			persistence.SetVar(persistentVarName + "mode", mapMode);
			RedrawMap();
		}

		private void ChangeZoom(bool up)
		{
			int oldZoom = zoomLevel;
			zoomLevel += up ? 1 : -1;
			if (zoomLevel < 0)
				zoomLevel = 0;
			if (zoomLevel > maxZoom)
				zoomLevel = maxZoom;
			if (zoomLevel != oldZoom) {
				persistence.SetVar(persistentVarName + "zoom", zoomLevel);
				RedrawMap();
			}
		}

		public void PageActive(bool status, int pageNumber)
		{
			pageActiveState = status;
			if (status)
				Debug.Log("JSISCANsatRPM: Active on page " + pageNumber);
			else
				Debug.Log("JSISCANsatRPM: Inactive.");
		}

		public override void OnUpdate()
		{
			if (!JUtil.IsActiveVessel(vessel) || !satFound)
				return;

			if ((Planetarium.GetUniversalTime() - trailPointEvery) > trailCounter) {
				trailCounter = Planetarium.GetUniversalTime();
				LeaveTrail();
			}

			if (!JUtil.IsInIVA())
				return;

			if (pageActiveState && map != null && !map.isMapComplete()) {
				map.getPartialMap();
			}

			targetVessel = FlightGlobals.fetch.VesselTarget as Vessel;

			if (UpdateCheck() || orbitingBody != vessel.mainBody) {
				if (orbitingBody != vessel.mainBody)
					trail.Clear();
				RedrawMap();
			}
		}

		private void RedrawMap()
		{
			map = new SCANmap();
			map.setProjection(SCANmap.MapProjection.Rectangular);
			orbitingBody = vessel.mainBody;
			map.setBody(vessel.mainBody);
			map.setSize(screenWidth, screenHeight);
			map.mapscale *= (zoomLevel * zoomLevel + zoomModifier);
			mapCenterLong = vessel.longitude;
			mapCenterLat = vessel.latitude;
			// That's really just sweeping the problem under the carpet instead of fixing it, but meh.
			if (zoomLevel == 0)
				mapCenterLat = 0;
			map.centerAround(mapCenterLong, mapCenterLat);
			map.resetMap(mapMode,1);

			// Compute and store the map scale factors in mapSizeScale.  We
			// use these values for every segment when drawing trails, so it
			// makes sense to compute it only when it changes.
			mapSizeScale = new Vector2d(360.0 * map.mapscale / map.mapwidth, 180.0 * map.mapscale / map.mapheight);
			redrawDeviation = redrawEdge * 180 / (zoomLevel * zoomLevel + zoomModifier);
			try {
				localAnomalies = SCANcontroller.controller.getData(vessel.mainBody).getAnomalies();
			} catch {
				Debug.Log("JSISCANsatRPM: Could not get a list of anomalies, what happened?");
			}
			// MATH!
			double kmPerDegreeLon = (2 * Math.PI * (orbitingBody.Radius / 1000d)) / 360d;
			double pixelsPerDegree = Math.Abs(longitudeToPixels(mapCenterLong + (((mapCenterLong + 1) > 360) ? -1 : 1), mapCenterLat) - longitudeToPixels(mapCenterLong, mapCenterLat));
			pixelsPerKm = pixelsPerDegree / kmPerDegreeLon;
		}

		private bool UpdateCheck()
		{
			if (map == null)
				return false;
			// Do not flush map when timewarping.
			if (TimeWarp.WarpMode == TimeWarp.Modes.HIGH && TimeWarp.CurrentRateIndex != 0)
				return false;
			if (Math.Abs(vessel.longitude - mapCenterLong) > redrawDeviation)
				return true;
			// Same sweeping.
			if (Math.Abs(vessel.latitude - mapCenterLat) > redrawDeviation && zoomLevel > 0)
				return true;

			return false;
		}

		private void LeaveTrail()
		{
			if (trailLimit > 0) {
				trail.Add(new Vector2d(vessel.longitude, vessel.latitude));
				if (trail.Count > trailLimit)
					trail.RemoveRange(0, trail.Count - trailLimit);
			}
		}

		private void Start()
		{

			InstallationPathWarning.Warn("SCANsatRPM");

			// Arrrgh.
			if (!string.IsNullOrEmpty(iconColorSelf))
				iconColorSelfValue = ConfigNode.ParseColor32(iconColorSelf);
			if (!string.IsNullOrEmpty(iconColorTarget))
				iconColorTargetValue = ConfigNode.ParseColor32(iconColorTarget);
			if (!string.IsNullOrEmpty(iconColorUnvisitedAnomaly))
				iconColorUnvisitedAnomalyValue = ConfigNode.ParseColor32(iconColorUnvisitedAnomaly);
			if (!string.IsNullOrEmpty(iconColorVisitedAnomaly))
				iconColorVisitedAnomalyValue = ConfigNode.ParseColor32(iconColorVisitedAnomaly);
			if (!string.IsNullOrEmpty(iconColorShadow))
				iconColorShadowValue = ConfigNode.ParseColor32(iconColorShadow);
			if (!string.IsNullOrEmpty(iconColorAP))
				iconColorAPValue = ConfigNode.ParseColor32(iconColorAP);
			if (!string.IsNullOrEmpty(iconColorPE))
				iconColorPEValue = ConfigNode.ParseColor32(iconColorPE);
			if (!string.IsNullOrEmpty(iconColorANDN))
				iconColorANDNValue = ConfigNode.ParseColor32(iconColorANDN);
			if (!string.IsNullOrEmpty(iconColorNode))
				iconColorNodeValue = ConfigNode.ParseColor32(iconColorNode);
			if (!string.IsNullOrEmpty(trailColor))
				trailColorValue = ConfigNode.ParseColor32(trailColor);

			// Referencing the parent project should work, shouldn't it.
			persistentVarName = "scansat" + internalProp.propID;
			persistence = new PersistenceAccessor(part);

			showLines = persistence.GetBool(persistentVarName + "lines") ?? true;

			trailMaterial = JUtil.DrawLineMaterial();

			LeaveTrail();

			if (!string.IsNullOrEmpty(scaleBar) && !string.IsNullOrEmpty(scaleLabels) && !string.IsNullOrEmpty(scaleLevels)) {
				scaleBarTexture = GameDatabase.Instance.GetTexture(scaleBar, false);
				scaleLabelTexture = GameDatabase.Instance.GetTexture(scaleLabels, false);
				var scales = new List<float>();
				foreach (string scl in scaleLevels.Split(',')) {
					float scale;
					if (float.TryParse(scl.Trim(), out scale))
						scales.Add(scale / 1000);

				}
				scaleLevelValues = scales.ToArray();
				Array.Sort(scaleLevelValues);
				scaleLabelSpan = 1f / scaleLevelValues.Length;
			}

			// Now the fun bit: Locate all cfg files depicting map features anywhere.

			foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes ("JSISCANSATVECTORMARK")) {
				mapMarkup.Add(new MapMarkupLine(node));
			}
			startupComplete = true;
		}

		private class MapMarkupLine
		{
			public CelestialBody body;
			public Color32 color;
			public List<Vector2d> points = new List<Vector2d>();

			public MapMarkupLine(ConfigNode node)
			{
				if (!node.HasData)
					throw new ArgumentException("Map markup section with no data?!");

				if (node.HasValue("body")) {
					string bodyName = node.GetValue("body").ToLower();
					foreach (CelestialBody thatBody in FlightGlobals.fetch.bodies) {
						if (thatBody.GetName().ToLower() == bodyName) {
							body = thatBody;
							break;
						}
					}
					if (body == null)
						throw new ArgumentException("No celestial body matching '" + bodyName + "'.");
				} else {
					throw new ArgumentException("Found a map markup section that does not say which celestial body it refers to.");
				}

				color = Color.white;
				if (node.HasValue("color"))
					color = ConfigNode.ParseColor32(node.GetValue("color"));
				// Now to actually load in the points...

				foreach (string pointData in node.GetValues("vertex")) {
					string[] tokens = pointData.Split(',');
					if (tokens.Length != 2)
						throw new ArgumentException("Incorrect vertex format.");
					double x, y;
					if (!(double.TryParse(tokens[0].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out x) &&
					    double.TryParse(tokens[1].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out y)))
						throw new ArgumentException("Could not parse a vertex position.");
					if (x > 180d || x < -180d || y > 90d || y < -90d)
						throw new ArgumentException("Vertex positions must be in degrees appropriate to longitude and latitude.");
					points.Add(new Vector2d(x, y));
				}
			}
		}
	}
}


