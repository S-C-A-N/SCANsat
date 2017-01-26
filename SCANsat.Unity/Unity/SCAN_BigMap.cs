using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SCANsat.Unity.Interfaces;

namespace SCANsat.Unity.Unity
{
	public class SCAN_BigMap : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerDownHandler
	{
		[SerializeField]
		private TextHandler m_Version = null;
		[SerializeField]
		private TextHandler m_Title = null;
		[SerializeField]
		private Transform m_Projection = null;
		[SerializeField]
		private Transform m_MapType = null;
		[SerializeField]
		private Transform m_Resources = null;
		[SerializeField]
		private Transform m_CelestialBody = null;
		[SerializeField]
		private ToggleGroup m_DropDownToggles = null;
		[SerializeField]
		private GameObject m_DropDownPrefab = null;
		[SerializeField]
		private GameObject m_OrbitObject = null;
		[SerializeField]
		private GameObject m_WaypointObject = null;
		[SerializeField]
		private SCAN_Toggle m_ColorToggle = null;
		[SerializeField]
		private SCAN_Toggle m_GridToggle = null;
		[SerializeField]
		private SCAN_Toggle m_OrbitToggle = null;
		[SerializeField]
		private SCAN_Toggle m_WaypointToggle = null;
		[SerializeField]
		private SCAN_Toggle m_AnomalyToggle = null;
		[SerializeField]
		private SCAN_Toggle m_FlagToggle = null;
		[SerializeField]
		private SCAN_Toggle m_AsteroidToggle = null;
		[SerializeField]
		private SCAN_Toggle m_LegendToggle = null;
		[SerializeField]
		private SCAN_Toggle m_ResourcesToggle = null;
		[SerializeField]
		private TextHandler m_ReadoutText = null;
		[SerializeField]
		private RawImage m_MapImage = null;
		[SerializeField]
		private LayoutElement m_MapLayout = null;
		[SerializeField]
		private GameObject m_MapLabelPrefab = null;
		[SerializeField]
		private GameObject m_LegendObject = null;
		[SerializeField]
		private RawImage m_LegendImage = null;
		[SerializeField]
		private TextHandler m_LegendLabelOne = null;
		[SerializeField]
		private TextHandler m_LegendLabelTwo = null;
		[SerializeField]
		private TextHandler m_LegendLabelThree = null;

		private ISCAN_BigMap bigInterface;
		private bool loaded;
		private RectTransform rect;
		private Vector2 mouseStart;
		private Vector3 windowStart;
		private bool resizing;
		private bool inMap;
		private Vector2 rectPos = new Vector2();

		private List<SCAN_SimpleLabel> orbitLabels = new List<SCAN_SimpleLabel>();
		private List<SCAN_MapLabel> orbitIconLabels = new List<SCAN_MapLabel>();
		private List<SCAN_MapLabel> anomalyLabels = new List<SCAN_MapLabel>();
		private List<SCAN_MapLabel> waypointLabels = new List<SCAN_MapLabel>();
		private List<SCAN_MapLabel> flagLabels = new List<SCAN_MapLabel>();
		private SCAN_MapLabel vesselLabel;

		private SCAN_DropDown dropDown;

		private void Awake()
		{
			rect = GetComponent<RectTransform>();
		}

		private void Update()
		{
			if (bigInterface == null || !bigInterface.IsVisible)
				return;

			bigInterface.Update();

			if (vesselLabel != null)
				vesselLabel.UpdatePosition(bigInterface.VesselPosition());

			if (inMap && m_MapImage != null && m_ReadoutText != null)
			{
				RectTransformUtility.ScreenPointToLocalPointInRectangle(m_MapImage.rectTransform, Input.mousePosition, bigInterface.MainCanvas.worldCamera, out rectPos);

				m_ReadoutText.OnTextUpdate.Invoke(bigInterface.MapInfo(rectPos));
			}

			if (bigInterface.OrbitToggle && bigInterface.ShowOrbit)
			{
				for (int i = orbitLabels.Count - 1; i >= 0; i--)
				{
					SCAN_SimpleLabel label = orbitLabels[i];

					label.UpdateIcon(bigInterface.OrbitInfo(i));
				}
			}
		}

		private void OnGUI()
		{
			if (resizing)
				return;

			if (rect == null || bigInterface == null || !bigInterface.IsVisible || m_MapImage == null)
				return;

			bigInterface.MapScreenPosition = ScreenPosition(m_MapImage.rectTransform, bigInterface.MainCanvas);

			bigInterface.OnGUI();
		}

		private Vector2 ScreenPosition(RectTransform r, Canvas canvas)
		{
			Vector3[] corners = new Vector3[4];
			Vector2 pos = new Vector2();

			r.GetWorldCorners(corners);

			//print(string.Format("[SCAN_UI] Map Corners: 0: {0:N2} - 1: {1:N2} - 2: {2:N2} - 3: {3:N2}",
				//corners[0], corners[1], corners[2], corners[3]));

			if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
			{
				pos = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
			}
			else
			{
				pos = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[0]);
			}

			pos.y = Screen.height - pos.y - r.sizeDelta.y;

			//print(string.Format("[SCAN_UI] Screen Position: {0:N2}", pos));

			return pos;
		}

		public void setMap(ISCAN_BigMap map)
		{
			if (map == null)
				return;

			bigInterface = map;

			if (m_Version != null)
				m_Version.OnTextUpdate.Invoke(map.Version);

			if (m_ColorToggle != null)
				m_ColorToggle.isOn = map.ColorToggle;

			if (m_GridToggle != null)
				m_GridToggle.isOn = map.GridToggle;

			if (m_OrbitToggle != null)
				m_OrbitToggle.isOn = map.OrbitToggle;

			if (m_WaypointToggle != null)
				m_WaypointToggle.isOn = map.WaypointToggle;

			if (m_AnomalyToggle != null)
				m_AnomalyToggle.isOn = map.AnomalyToggle;

			if (m_FlagToggle != null)
				m_FlagToggle.isOn = map.FlagToggle;

			if (m_AsteroidToggle != null)
				m_AsteroidToggle.isOn = map.AsteroidToggle;

			if (m_LegendToggle != null)
				m_LegendToggle.isOn = map.LegendToggle;

			if (m_ResourcesToggle != null)
				m_ResourcesToggle.isOn = map.ResourceToggle;

			if (!map.ShowOrbit && m_OrbitObject != null)
				m_OrbitObject.SetActive(false);

			if (!map.ShowWaypoint && m_WaypointObject != null)
				m_WaypointObject.SetActive(false);

			if (!map.ShowResource && m_Resources != null)
				m_Resources.gameObject.SetActive(false);

			if (m_LegendObject != null && !map.LegendToggle)
				m_LegendObject.SetActive(false);

			if (map.LegendToggle)
				SetLegend(map.LegendImage, map.LegendLabels);

			SetScale(map.Scale);

			SetPosition(map.Position);

			SetSize(map.Size);

			SetIcons();

			loaded = true;
		}

		public void SetScale(float scale)
		{
			rect.localScale = Vector3.one * scale;
		}

		public void SetPosition(Vector2 pos)
		{
			if (rect == null)
				return;

			rect.anchoredPosition = new Vector3(pos.x, pos.y, 0);
		}

		private void SetSize(Vector2 size)
		{
			if (m_MapLayout == null)
				return;

			if (size.x < m_MapLayout.minWidth)
				size.x = m_MapLayout.minWidth;
			else if (size.x > 8192)
				size.x = 8192;

			if (size.x % 2 != 0)
				size.x += 1;

			m_MapLayout.preferredWidth = size.x;
			m_MapLayout.preferredHeight = m_MapLayout.preferredWidth / 2;
		}

		private void SetLegend(Texture2D tex, IList<string> labels)
		{
			if (m_LegendImage != null)
				m_LegendImage.texture = tex;

			if (labels == null || labels.Count != 3)
				return;

			if (m_LegendLabelOne != null)
				m_LegendLabelOne.OnTextUpdate.Invoke(labels[0]);

			if (m_LegendLabelTwo != null)
				m_LegendLabelTwo.OnTextUpdate.Invoke(labels[1]);

			if (m_LegendLabelThree != null)
				m_LegendLabelThree.OnTextUpdate.Invoke(labels[2]);
		}

		private void SetFlagIcons(Dictionary<Guid, MapLabelInfo> flags)
		{
			if (flags == null)
				return;

			if (bigInterface == null || m_MapLabelPrefab == null || m_MapImage == null)
				return;

			for (int i = 0; i < flags.Count; i++)
			{
				Guid id = flags.ElementAt(i).Key;

				MapLabelInfo info;

				if (!flags.TryGetValue(id, out info))
					continue;

				createFlag(id, info);
			}
		}

		private void createFlag(Guid id, MapLabelInfo info)
		{
			SCAN_MapLabel mapLabel = Instantiate(m_MapLabelPrefab).GetComponent<SCAN_MapLabel>();

			if (mapLabel == null)
				return;

			mapLabel.transform.SetParent(m_MapImage.transform, false);

			mapLabel.Setup(id, info);

			flagLabels.Add(mapLabel);
		}

		private void SetAnomalyIcons(Dictionary<string, MapLabelInfo> anomalies)
		{
			if (anomalies == null)
				return;

			if (bigInterface == null || m_MapLabelPrefab == null || m_MapImage == null)
				return;

			for (int i = 0; i < anomalies.Count; i++)
			{
				string id = anomalies.ElementAt(i).Key;

				MapLabelInfo info;

				if (!anomalies.TryGetValue(id, out info))
					continue;

				createAnomaly(id, info);
			}
		}

		private void createAnomaly(string id, MapLabelInfo info)
		{
			SCAN_MapLabel mapLabel = Instantiate(m_MapLabelPrefab).GetComponent<SCAN_MapLabel>();

			if (mapLabel == null)
				return;

			mapLabel.transform.SetParent(m_MapImage.transform, false);

			mapLabel.Setup(id, info);

			anomalyLabels.Add(mapLabel);
		}

		private void SetWaypointIcons(Dictionary<int, MapLabelInfo> waypoints)
		{
			if (waypoints == null)
				return;

			if (bigInterface == null || m_MapLabelPrefab == null || m_MapImage == null)
				return;

			for (int i = 0; i < waypoints.Count; i++)
			{
				int id = waypoints.ElementAt(i).Key;

				MapLabelInfo info;

				if (!waypoints.TryGetValue(id, out info))
					continue;

				createWaypoint(id, info);
			}
		}

		private void createWaypoint(int id, MapLabelInfo info)
		{
			SCAN_MapLabel mapLabel = Instantiate(m_MapLabelPrefab).GetComponent<SCAN_MapLabel>();

			if (mapLabel == null)
				return;

			mapLabel.transform.SetParent(m_MapImage.transform, false);

			mapLabel.Setup(id, info);

			waypointLabels.Add(mapLabel);
		}

		private void SetVesselIcon(KeyValuePair<Guid, MapLabelInfo> vessel)
		{
			if (vessel.Value.label == "null")
				return;

			if (bigInterface == null || m_MapLabelPrefab == null || m_MapImage == null)
				return;

			SCAN_MapLabel mapLabel = Instantiate(m_MapLabelPrefab).GetComponent<SCAN_MapLabel>();

			if (mapLabel == null)
				return;

			mapLabel.transform.SetParent(m_MapImage.transform, false);

			mapLabel.Setup(vessel.Key, vessel.Value);

			vesselLabel = mapLabel;
		}

		private void SetOrbitIcons(int count)
		{
			if (bigInterface == null || m_MapImage == null)
				return;

			for (int i = 0; i < count; i++)
			{
				SimpleLabelInfo info = bigInterface.OrbitInfo(i);

				CreateOrbitIcon(info);
			}
		}

		private void CreateOrbitIcon(SimpleLabelInfo info)
		{
			GameObject labelObj = new GameObject("SCAN_SimpleLabel");

			SCAN_SimpleLabel label = labelObj.AddComponent<SCAN_SimpleLabel>();

			if (label == null)
				return;

			label.transform.SetParent(m_MapImage.transform, false);

			label.Setup(info);

			orbitLabels.Add(label);
		}

		private void ClearIcons()
		{
			for (int i = waypointLabels.Count - 1; i >= 0; i--)
			{
				SCAN_MapLabel m = waypointLabels[i];

				m.gameObject.SetActive(false);
				Destroy(m.gameObject);
			}

			for (int i = anomalyLabels.Count - 1; i >= 0; i--)
			{
				SCAN_MapLabel m = anomalyLabels[i];

				m.gameObject.SetActive(false);
				Destroy(m.gameObject);
			}

			for (int i = flagLabels.Count - 1; i >= 0; i--)
			{
				SCAN_MapLabel m = flagLabels[i];

				m.gameObject.SetActive(false);
				Destroy(m.gameObject);
			}

			for (int i = orbitLabels.Count - 1; i >= 0; i--)
			{
				SCAN_SimpleLabel s = orbitLabels[i];

				s.gameObject.SetActive(false);
				Destroy(s.gameObject);
			}

			if (vesselLabel != null)
			{
				vesselLabel.gameObject.SetActive(false);
				Destroy(vesselLabel.gameObject);
			}

			flagLabels.Clear();
			anomalyLabels.Clear();
			waypointLabels.Clear();
			orbitLabels.Clear();
			vesselLabel = null;
		}

		private void RefreshIcons()
		{
			ClearIcons();

			SetIcons();
		}

		private void SetIcons()
		{
			if (bigInterface == null)
				return;

			if (bigInterface.FlagToggle)
				SetFlagIcons(bigInterface.FlagInfoList);

			if (bigInterface.AnomalyToggle)
				SetAnomalyIcons(bigInterface.AnomalyInfoList);

			if (bigInterface.WaypointToggle && bigInterface.ShowWaypoint)
				SetWaypointIcons(bigInterface.WaypointInfoList);

			if (bigInterface.OrbitToggle && bigInterface.ShowOrbit)
				SetOrbitIcons(bigInterface.OrbitSteps);

			SetVesselIcon(bigInterface.VesselInfo);
		}

		public void OnEnterMap(BaseEventData eventData)
		{
			inMap = true;
		}

		public void OnExitMap(BaseEventData eventData)
		{
			inMap = false;

			if (m_ReadoutText != null)
				m_ReadoutText.OnTextUpdate.Invoke("");
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			if (rect == null)
				return;

			mouseStart = eventData.position;
			windowStart = rect.position;
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (rect == null)
				return;

			rect.position = windowStart + (Vector3)(eventData.position - mouseStart);
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			if (rect == null || bigInterface == null)
				return;

			bigInterface.Position = new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y);
		}

		public void OnStartResize(BaseEventData eventData)
		{
			if (!(eventData is PointerEventData))
				return;

			resizing = true;

			ClearIcons();
		}

		public void OnResize(BaseEventData eventData)
		{
			if (m_MapLayout == null)
				return;

			if (!(eventData is PointerEventData))
				return;

			//print(string.Format("[SCAN_UI] Resize - Width: {0:N2} - Delta: {1:N2} - Height: {2:N2}"
			//	, m_MapLayout.preferredWidth
			//	, ((PointerEventData)eventData).delta.x
			//	, m_MapLayout.preferredHeight));

			m_MapLayout.preferredWidth += ((PointerEventData)eventData).delta.x;

			if (m_MapLayout.preferredWidth < m_MapLayout.minWidth)
				m_MapLayout.preferredWidth = m_MapLayout.minWidth;
			else if (m_MapLayout.preferredWidth > 8192)
				m_MapLayout.preferredWidth = 8192;

			if (m_MapLayout.preferredWidth % 2 != 0)
				m_MapLayout.preferredWidth += 1;

			m_MapLayout.preferredHeight = m_MapLayout.preferredWidth / 2;

			//print(string.Format("[SCAN_UI] New size - Width: {0:N2} - Delta: {1:N2} - Height: {2:N2}"
			//	, m_MapLayout.preferredWidth
			//	, ((PointerEventData)eventData).delta.x
			//	, m_MapLayout.preferredHeight));
		}

		public void OnEndResize(BaseEventData eventData)
		{
			resizing = false;

			if (m_MapLayout == null || bigInterface == null)
				return;

			bigInterface.Size = new Vector2(m_MapLayout.preferredWidth, m_MapLayout.preferredHeight);

			SetIcons();
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			transform.SetAsLastSibling();

			if (dropDown == null)
				return;

			RectTransform r = dropDown.GetComponent<RectTransform>();

			if (r == null)
				return;

			if (RectTransformUtility.RectangleContainsScreenPoint(r, eventData.position, eventData.pressEventCamera))
				return;

			dropDown.gameObject.SetActive(false);
			DestroyImmediate(dropDown.gameObject);
			dropDown = null;

			if (m_DropDownToggles != null)
				m_DropDownToggles.SetAllTogglesOff();
		}

		public void Close()
		{
			if (bigInterface == null)
				return;

			bigInterface.IsVisible = false;
		}

		public void UpdateTitle(string text)
		{
			if (m_Title == null)
				return;

			m_Title.OnTextUpdate.Invoke(text);
		}

		public void UpdateMapTexture(Texture2D map)
		{
			if (m_MapImage == null)
				return;

			m_MapImage.texture = map;
		}

		public void ToggleProjectionSelection(bool isOn)
		{
			if (dropDown != null)
			{
				dropDown.gameObject.SetActive(false);
				DestroyImmediate(dropDown.gameObject);
				dropDown = null;
			}

			if (!isOn)
				return;

			if (bigInterface == null || m_DropDownPrefab == null || m_Projection == null)
				return;

			dropDown = Instantiate(m_DropDownPrefab).GetComponent<SCAN_DropDown>();

			if (dropDown == null)
				return;

			dropDown.transform.SetParent(m_Projection, false);

			dropDown.Setup(bigInterface.Projections, bigInterface.CurrentProjection);

			dropDown.OnSelectUpdate.AddListener(new UnityEngine.Events.UnityAction<string>(SetProjection));
		}

		private void SetProjection(string selection)
		{
			if (bigInterface == null)
				return;

			bigInterface.CurrentProjection = selection;

			dropDown.gameObject.SetActive(false);
			DestroyImmediate(dropDown.gameObject);
			dropDown = null;

			if (m_DropDownToggles != null)
				m_DropDownToggles.SetAllTogglesOff();

			RefreshIcons();
		}

		public void ToggleTypeSelection(bool isOn)
		{
			if (dropDown != null)
			{
				dropDown.gameObject.SetActive(false);
				DestroyImmediate(dropDown.gameObject);
				dropDown = null;
			}

			if (!isOn)
				return;

			if (bigInterface == null || m_DropDownPrefab == null || m_MapType == null)
				return;

			dropDown = Instantiate(m_DropDownPrefab).GetComponent<SCAN_DropDown>();

			if (dropDown == null)
				return;

			dropDown.transform.SetParent(m_MapType, false);

			dropDown.Setup(bigInterface.MapTypes, bigInterface.CurrentMapType);

			dropDown.OnSelectUpdate.AddListener(new UnityEngine.Events.UnityAction<string>(SetType));
		}

		private void SetType(string selection)
		{
			if (bigInterface == null)
				return;

			bigInterface.CurrentMapType = selection;

			dropDown.gameObject.SetActive(false);
			DestroyImmediate(dropDown.gameObject);
			dropDown = null;

			if (m_DropDownToggles != null)
				m_DropDownToggles.SetAllTogglesOff();

			RefreshIcons();
		}

		public void ToggleResourceSelection(bool isOn)
		{
			if (dropDown != null)
			{
				dropDown.gameObject.SetActive(false);
				DestroyImmediate(dropDown.gameObject);
				dropDown = null;
			}

			if (!isOn)
				return;

			if (bigInterface == null || m_DropDownPrefab == null || m_Resources == null)
				return;

			dropDown = Instantiate(m_DropDownPrefab).GetComponent<SCAN_DropDown>();

			if (dropDown == null)
				return;

			dropDown.transform.SetParent(m_Resources, false);

			dropDown.Setup(bigInterface.Resources, bigInterface.CurrentResource);

			dropDown.OnSelectUpdate.AddListener(new UnityEngine.Events.UnityAction<string>(SetResource));
		}

		private void SetResource(string selection)
		{
			if (bigInterface == null)
				return;

			bigInterface.CurrentResource = selection;

			dropDown.gameObject.SetActive(false);
			DestroyImmediate(dropDown.gameObject);
			dropDown = null;

			if (m_DropDownToggles != null)
				m_DropDownToggles.SetAllTogglesOff();

			RefreshIcons();
		}

		public void ToggleCelestialBodySelection(bool isOn)
		{
			if (dropDown != null)
			{
				dropDown.gameObject.SetActive(false);
				DestroyImmediate(dropDown.gameObject);
				dropDown = null;
			}

			if (!isOn)
				return;

			if (bigInterface == null || m_DropDownPrefab == null || m_CelestialBody == null)
				return;

			dropDown = Instantiate(m_DropDownPrefab).GetComponent<SCAN_DropDown>();

			if (dropDown == null)
				return;

			dropDown.transform.SetParent(m_CelestialBody, false);

			dropDown.Setup(bigInterface.CelestialBodies, bigInterface.CurrentCelestialBody);

			dropDown.OnSelectUpdate.AddListener(new UnityEngine.Events.UnityAction<string>(SetCelestialBody));
		}

		private void SetCelestialBody(string selection)
		{
			if (bigInterface == null)
				return;

			bigInterface.CurrentCelestialBody = selection;

			if (bigInterface.LegendToggle)
				SetLegend(bigInterface.LegendImage, bigInterface.LegendLabels);

			dropDown.gameObject.SetActive(false);
			DestroyImmediate(dropDown.gameObject);
			dropDown = null;

			if (m_DropDownToggles != null)
				m_DropDownToggles.SetAllTogglesOff();

			RefreshIcons();
		}

		public void RefreshMap()
		{
			if (bigInterface == null)
				return;

			bigInterface.RefreshMap();

			RefreshIcons();
		}

		public void ToggleColor(bool isOn)
		{
			if (!loaded || bigInterface == null)
				return;

			bigInterface.ColorToggle = isOn;

			RefreshIcons();

			if (bigInterface.LegendToggle)
				SetLegend(bigInterface.LegendImage, bigInterface.LegendLabels);
		}

		public void ToggleGrid(bool isOn)
		{
			if (!loaded || bigInterface == null)
				return;

			bigInterface.GridToggle = isOn;
		}

		public void ToggleOrbit(bool isOn)
		{
			if (!loaded || bigInterface == null)
				return;

			bigInterface.OrbitToggle = isOn;

			RefreshIcons();
		}

		public void ToggleWaypoint(bool isOn)
		{
			if (!loaded || bigInterface == null)
				return;

			bigInterface.WaypointToggle = isOn;

			if (isOn && bigInterface.ShowWaypoint)
				SetWaypointIcons(bigInterface.WaypointInfoList);
			else
			{
				for (int i = waypointLabels.Count - 1; i >= 0; i--)
				{
					SCAN_MapLabel m = waypointLabels[i];

					m.gameObject.SetActive(false);
					Destroy(m.gameObject);
				}

				waypointLabels.Clear();
			}
		}

		public void ToggleAnomaly(bool isOn)
		{
			if (!loaded || bigInterface == null)
				return;

			bigInterface.AnomalyToggle = isOn;

			if (isOn)
				SetAnomalyIcons(bigInterface.AnomalyInfoList);
			else
			{
				for (int i = anomalyLabels.Count - 1; i >= 0; i--)
				{
					SCAN_MapLabel m = anomalyLabels[i];

					m.gameObject.SetActive(false);
					Destroy(m.gameObject);
				}

				anomalyLabels.Clear();
			}
		}

		public void ToggleFlag(bool isOn)
		{
			if (!loaded || bigInterface == null)
				return;

			bigInterface.FlagToggle = isOn;

			if (isOn)
				SetFlagIcons(bigInterface.FlagInfoList);
			else
			{
				for (int i = flagLabels.Count - 1; i >= 0; i--)
				{
					SCAN_MapLabel m = flagLabels[i];

					m.gameObject.SetActive(false);
					Destroy(m.gameObject);
				}

				flagLabels.Clear();
			}
		}

		public void ToggleAsteroid(bool isOn)
		{
			if (!loaded || bigInterface == null)
				return;

			bigInterface.AsteroidToggle = isOn;
		}

		public void ToggleLegend(bool isOn)
		{
			if (!loaded || bigInterface == null)
				return;

			bigInterface.LegendToggle = isOn;

			if (m_LegendObject != null)
				m_LegendObject.SetActive(isOn);

			if (isOn)
				SetLegend(bigInterface.LegendImage, bigInterface.LegendLabels);
		}

		public void ToggleResource(bool isOn)
		{
			if (!loaded || bigInterface == null)
				return;

			bigInterface.ResourceToggle = isOn;

			RefreshIcons();
		}

		public void OpenSmallMap()
		{
			if (bigInterface == null)
				return;

			bigInterface.OpenMainMap();
		}

		public void OpenInstruments()
		{
			if (bigInterface == null)
				return;

			bigInterface.OpenInstruments();
		}

		public void OpenSettings()
		{
			if (bigInterface == null)
				return;

			bigInterface.OpenSettings();
		}

		public void OpenZoomMap()
		{
			if (bigInterface == null)
				return;

			bigInterface.OpenZoomMap();
		}

		public void OpenOverlay()
		{
			if (bigInterface == null)
				return;

			bigInterface.OpenOverlay();
		}

		public void ExportMap()
		{
			if (bigInterface == null)
				return;

			bigInterface.ExportMap();
		}

	}
}
