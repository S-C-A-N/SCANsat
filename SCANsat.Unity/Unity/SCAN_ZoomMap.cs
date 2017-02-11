using System;
using System.Collections.Generic;
using System.Linq;
using SCANsat.Unity.Interfaces;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SCANsat.Unity.Unity
{
	public class SCAN_ZoomMap : CanvasFader, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerDownHandler
	{
		[SerializeField]
		private float m_MaxWidth = 520;
		[SerializeField]
		private float m_MaxHeight = 440;
		[SerializeField]
		private TextHandler m_Version = null;
		[SerializeField]
		private Transform m_MapTypeMenu = null;
		[SerializeField]
		private Transform m_ResourceMenu = null;
		[SerializeField]
		private ToggleGroup m_DropDownToggles = null;
		[SerializeField]
		private GameObject m_DropDownPrefab = null;
		[SerializeField]
		private GameObject m_MapMoveObject = null;
		[SerializeField]
		private GameObject m_TobBarObject = null;
		[SerializeField]
		private GameObject m_TobBarSecondObject = null;
		[SerializeField]
		private GameObject m_ToggleBarObject = null;
		[SerializeField]
		private GameObject m_ReadoutBar = null;
		[SerializeField]
		private GameObject m_LegendBar = null;
		[SerializeField]
		private GameObject m_WaypointBar = null;
		[SerializeField]
		private TextHandler m_Title = null;
		[SerializeField]
		private TextHandler m_ZoomLevel = null;
		[SerializeField]
		private GameObject m_OrbitObject = null;
		[SerializeField]
		private Toggle m_OrbitToggle = null;
		[SerializeField]
		private Toggle m_ColorToggle = null;
		[SerializeField]
		private Toggle m_LegendToggle = null;
		[SerializeField]
		private Toggle m_ResourceToggle = null;
		[SerializeField]
		private Toggle m_IconsToggle = null;
		[SerializeField]
		private TextHandler m_ReadoutText = null;
		[SerializeField]
		private RawImage m_ZoomImage = null;
		[SerializeField]
		private LayoutElement m_MapLayout = null;
		[SerializeField]
		private RawImage m_LegendImage = null;
		[SerializeField]
		private TextHandler m_LegendLabelOne = null;
		[SerializeField]
		private TextHandler m_LegendLabelTwo = null;
		[SerializeField]
		private TextHandler m_LegendLabelThree = null;
		[SerializeField]
		private GameObject m_MapLabelPrefab = null;
		[SerializeField]
		private InputField m_WaypointInput = null;
		[SerializeField]
		private Image m_VesselLockImage = null;
		[SerializeField]
		private Sprite m_VesselLock = null;
		[SerializeField]
		private Sprite m_VesselUnlock = null;
		[SerializeField]
		private Image m_WindowState = null;
		[SerializeField]
		private Sprite m_WindowMax = null;
		[SerializeField]
		private Sprite m_WindowMed = null;
		[SerializeField]
		private Sprite m_WindowMin = null;

		private VerticalLayoutGroup windowLayout;
		private RectTransform rect;
		private Vector2 mouseStart;
		private Vector3 windowStart;
		private Vector2 resizeStart;
		private bool loaded;
		private bool inMap;
		private bool waypointSelecting;
		private string waypoint;
		private Vector2 rectPos = new Vector2();

		private SCAN_DropDown dropDown;

		private List<SCAN_SimpleLabel> orbitLabels = new List<SCAN_SimpleLabel>();
		private List<SCAN_MapLabel> orbitIconLabels = new List<SCAN_MapLabel>();
		private List<SCAN_MapLabel> anomalyLabels = new List<SCAN_MapLabel>();
		private List<SCAN_MapLabel> waypointLabels = new List<SCAN_MapLabel>();
		private List<SCAN_MapLabel> flagLabels = new List<SCAN_MapLabel>();
		private SCAN_MapLabel vesselLabel;
		private SCAN_MapLabel tempWaypointLabel;

		private ISCAN_ZoomMap zoomInterface;

		protected override void Awake()
		{
			base.Awake();

			rect = GetComponent<RectTransform>();
			windowLayout = GetComponent<VerticalLayoutGroup>();

			Alpha(0);
		}

		private void Update()
		{
			if (zoomInterface == null || !zoomInterface.IsVisible)
				return;

			if (zoomInterface.LockInput)
			{
				if (m_WaypointInput != null && !m_WaypointInput.isFocused)
					zoomInterface.LockInput = false;
			}

			zoomInterface.Update();

			if (vesselLabel != null)
			{
				Vector2 pos = zoomInterface.VesselPosition();

				if (pos.x < 0 || pos.y < 0)
					vesselLabel.UpdateActive(false);
				else
				{
					vesselLabel.UpdateActive(true);
					vesselLabel.UpdatePosition(pos);
				}
			}

			if (inMap && m_ZoomImage != null && m_ReadoutText != null)
			{
				RectTransformUtility.ScreenPointToLocalPointInRectangle(m_ZoomImage.rectTransform, Input.mousePosition, zoomInterface.MainCanvas.worldCamera, out rectPos);

				m_ReadoutText.OnTextUpdate.Invoke(zoomInterface.MapInfo(rectPos));
			}

			if (zoomInterface.OrbitToggle && zoomInterface.ShowOrbit)
			{
				for (int i = orbitLabels.Count - 1; i >= 0; i--)
				{
					SCAN_SimpleLabel label = orbitLabels[i];

					label.UpdateIcon(zoomInterface.OrbitInfo(i));
				}

				for (int i = orbitIconLabels.Count - 1; i >= 0; i--)
				{
					SCAN_MapLabel label = orbitIconLabels[i];

					label.UpdatePositionActivation(zoomInterface.OrbitIconInfo(label.StringID));
				}
			}
		}

		public void setMap(ISCAN_ZoomMap map)
		{
			if (map == null)
				return;

			zoomInterface = map;

			if (m_Version != null)
				m_Version.OnTextUpdate.Invoke(map.Version);

			if (m_ColorToggle != null)
				m_ColorToggle.isOn = map.ColorToggle;

			if (m_OrbitToggle != null)
				m_OrbitToggle.isOn = map.OrbitToggle;

			if (m_IconsToggle != null)
				m_IconsToggle.isOn = map.IconsToggle;

			if (m_LegendToggle != null)
				m_LegendToggle.isOn = map.LegendToggle;

			if (m_ResourceToggle != null)
				m_ResourceToggle.isOn = map.ResourceToggle;

			if (!map.OrbitAvailable && m_OrbitObject != null)
				m_OrbitObject.SetActive(false);

			if (!map.ShowResource && m_ResourceMenu != null)
				m_ResourceMenu.gameObject.SetActive(false);

			if (map.VesselLock && m_MapMoveObject != null)
				m_MapMoveObject.SetActive(false);

			if (m_VesselLockImage != null && m_VesselLock != null && m_VesselUnlock != null)
				m_VesselLockImage.sprite = map.VesselLock ? m_VesselLock : m_VesselUnlock;

			SetLegend(map.LegendToggle, map.LegendImage, map.LegendLabels);

			SetWindowState(map.WindowState);

			SetScale(map.Scale);

			SetPosition(map.Position);

			SetSize(map.Size);

			SetIcons();

			ProcessTooltips();

			ResetText();

			FadeIn();

			loaded = true;
		}

		public void FadeIn()
		{
			Fade(1, true);
		}

		public void FadeOut()
		{
			Fade(0, false, Kill, false);
		}

		private void Kill()
		{
			gameObject.SetActive(false);
			Destroy(gameObject);
		}

		public void Close()
		{
			if (zoomInterface != null)
				zoomInterface.IsVisible = false;
		}

		public void ProcessTooltips()
		{
			if (zoomInterface == null)
				return;

			TooltipHandler[] handlers = gameObject.GetComponentsInChildren<TooltipHandler>(true);

			if (handlers == null)
				return;

			for (int j = 0; j < handlers.Length; j++)
				ProcessTooltip(handlers[j], zoomInterface.TooltipsOn, zoomInterface.TooltipCanvas, zoomInterface.Scale);
		}

		private void ProcessTooltip(TooltipHandler handler, bool isOn, Canvas c, float scale)
		{
			if (handler == null)
				return;

			handler.IsActive = isOn && !handler.HelpTip;
			handler._Canvas = c;
			handler.Scale = scale;
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
			else if (size.x > m_MaxWidth)
				size.x = m_MaxWidth;

			if (size.y < m_MapLayout.minHeight)
				size.y = m_MapLayout.minHeight;
			else if (size.y > m_MaxHeight)
				size.y = m_MaxHeight;

			if (size.x % 2 != 0)
				size.x += 1;
			if (size.y % 2 != 0)
				size.y += 1;

			if (size.x % 4 != 0)
				size.x += 2;
			if (size.y % 4 != 0)
				size.y += 2;

			m_MapLayout.preferredWidth = size.x;
			m_MapLayout.preferredHeight = size.y;
		}

		private void SetWindowState(int i)
		{
			switch (i)
			{
				case 0:
					if (m_WindowState != null && m_WindowMax != null)
						m_WindowState.sprite = m_WindowMax;

					if (m_TobBarObject != null)
						m_TobBarObject.SetActive(true);

					if (m_TobBarSecondObject != null)
						m_TobBarSecondObject.SetActive(true);

					if (m_ToggleBarObject != null)
						m_ToggleBarObject.SetActive(true);

					SetLegend(zoomInterface.LegendToggle, zoomInterface.LegendImage, zoomInterface.LegendLabels);

					if (windowLayout != null)
					{
						RectOffset padding = windowLayout.padding;

						windowLayout.padding = new RectOffset(padding.left, 14, padding.top, 6);
					}

					break;
				case 1:
					if (m_WindowState != null && m_WindowMed != null)
						m_WindowState.sprite = m_WindowMed;

					if (m_TobBarObject != null)
						m_TobBarObject.SetActive(true);

					if (m_TobBarSecondObject != null)
						m_TobBarSecondObject.SetActive(true);

					if (m_ToggleBarObject != null)
						m_ToggleBarObject.SetActive(false);

					if (m_LegendBar != null)
						m_LegendBar.SetActive(false);

					if (windowLayout != null)
					{
						RectOffset padding = windowLayout.padding;

						windowLayout.padding = new RectOffset(padding.left, 18, padding.top, 18);
					}

					break;
				case 2:
					if (m_WindowState != null && m_WindowMin != null)
						m_WindowState.sprite = m_WindowMin;

					if (m_TobBarObject != null)
						m_TobBarObject.SetActive(false);

					if (m_TobBarSecondObject != null)
						m_TobBarSecondObject.SetActive(false);

					if (m_ToggleBarObject != null)
						m_ToggleBarObject.SetActive(false);

					if (m_LegendBar != null)
						m_LegendBar.SetActive(false);

					if (windowLayout != null)
					{
						RectOffset padding = windowLayout.padding;

						windowLayout.padding = new RectOffset(padding.left, 18, padding.top, 18);
					}

					break;
			}
		}

		private void SetLegend(bool isOn, Texture2D tex, IList<string> labels)
		{
			if (m_LegendBar == null)
				return;

			if (zoomInterface.CurrentMapType == "Altimetry" && zoomInterface.WindowState == 0)
				m_LegendBar.SetActive(isOn);
			else
			{
				m_LegendBar.SetActive(false);
				return;
			}

			if (!isOn)
				return;

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

			if (m_MapLabelPrefab == null || m_ZoomImage == null)
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

			mapLabel.transform.SetParent(m_ZoomImage.transform, false);

			mapLabel.Setup(id, info);

			flagLabels.Add(mapLabel);
		}

		private void SetAnomalyIcons(Dictionary<string, MapLabelInfo> anomalies)
		{
			if (anomalies == null)
				return;

			if (m_MapLabelPrefab == null || m_ZoomImage == null)
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

			mapLabel.transform.SetParent(m_ZoomImage.transform, false);

			mapLabel.Setup(id, info);

			anomalyLabels.Add(mapLabel);
		}

		private void SetWaypointIcons(Dictionary<int, MapLabelInfo> waypoints)
		{
			if (waypoints == null)
				return;

			if (m_MapLabelPrefab == null || m_ZoomImage == null)
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

		private SCAN_MapLabel createWaypoint(int id, MapLabelInfo info, bool temp = false)
		{
			SCAN_MapLabel mapLabel = Instantiate(m_MapLabelPrefab).GetComponent<SCAN_MapLabel>();

			if (mapLabel == null)
				return null;

			mapLabel.transform.SetParent(m_ZoomImage.transform, false);

			mapLabel.Setup(id, info);

			if (!temp)
				waypointLabels.Add(mapLabel);

			return mapLabel;
		}

		private void SetVesselIcon(KeyValuePair<Guid, MapLabelInfo> vessel)
		{
			if (vessel.Value.label == "null")
				return;

			if (m_MapLabelPrefab == null || m_ZoomImage == null)
				return;

			SCAN_MapLabel mapLabel = Instantiate(m_MapLabelPrefab).GetComponent<SCAN_MapLabel>();

			if (mapLabel == null)
				return;

			mapLabel.transform.SetParent(m_ZoomImage.transform, false);

			mapLabel.Setup(vessel.Key, vessel.Value);

			vesselLabel = mapLabel;
		}

		private void SetOrbitIcons(int count)
		{
			if (zoomInterface == null || m_ZoomImage == null)
				return;

			for (int i = 0; i < count; i++)
			{
				SimpleLabelInfo info = zoomInterface.OrbitInfo(i);

				CreateOrbitIcon(info);
			}
		}

		private void CreateOrbitIcon(SimpleLabelInfo info)
		{
			GameObject labelObj = new GameObject("SCAN_SimpleLabel");

			SCAN_SimpleLabel label = labelObj.AddComponent<SCAN_SimpleLabel>();

			if (label == null)
				return;

			label.transform.SetParent(m_ZoomImage.transform, false);

			label.Setup(info);

			orbitLabels.Add(label);
		}

		private void SetOrbitMapIcons(Dictionary<string, MapLabelInfo> orbitLabels)
		{
			if (orbitLabels == null)
				return;

			if (m_MapLabelPrefab == null || m_ZoomImage == null)
				return;

			for (int i = 0; i < orbitLabels.Count; i++)
			{
				string id = orbitLabels.ElementAt(i).Key;

				MapLabelInfo info;

				if (!orbitLabels.TryGetValue(id, out info))
					continue;

				CreateOrbitMapIcon(id, info);
			}
		}

		private void CreateOrbitMapIcon(string id, MapLabelInfo info)
		{
			SCAN_MapLabel mapLabel = Instantiate(m_MapLabelPrefab).GetComponent<SCAN_MapLabel>();

			if (mapLabel == null)
				return;

			mapLabel.transform.SetParent(m_ZoomImage.transform, false);

			mapLabel.Setup(id, info);

			orbitIconLabels.Add(mapLabel);
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
			
			for (int i = orbitIconLabels.Count - 1; i >= 0; i--)
			{
				SCAN_MapLabel m = orbitIconLabels[i];

				m.gameObject.SetActive(false);
				Destroy(m.gameObject);
			}

			if (vesselLabel != null)
			{
				vesselLabel.gameObject.SetActive(false);
				Destroy(vesselLabel.gameObject);
			}
	
			if (tempWaypointLabel != null)
			{
				tempWaypointLabel.gameObject.SetActive(false);
				Destroy(tempWaypointLabel.gameObject);
			}
		
			flagLabels.Clear();
			anomalyLabels.Clear();
			waypointLabels.Clear();
			orbitLabels.Clear();
			orbitIconLabels.Clear();
			vesselLabel = null;
			tempWaypointLabel = null;
		}

		public void RefreshIcons()
		{
			ClearIcons();

			SetIcons();
		}

		private void SetIcons()
		{
			if (zoomInterface == null)
				return;

			if (zoomInterface.IconsToggle)
			{
				SetFlagIcons(zoomInterface.FlagInfoList);

				SetAnomalyIcons(zoomInterface.AnomalyInfoList);

				if (zoomInterface.ShowWaypoint)
					SetWaypointIcons(zoomInterface.WaypointInfoList);
			}

			if (zoomInterface.OrbitToggle && zoomInterface.ShowOrbit)
			{
				SetOrbitIcons(zoomInterface.OrbitSteps);
				SetOrbitMapIcons(zoomInterface.OrbitLabelList);
			}

			SetVesselIcon(zoomInterface.VesselInfo);
		}

		private void ResetText()
		{
			if (zoomInterface == null)
				return;

			if (m_ZoomLevel != null)
				m_ZoomLevel.OnTextUpdate.Invoke(zoomInterface.ZoomLevelText);

			if (m_Title != null)
				m_Title.OnTextUpdate.Invoke(zoomInterface.MapCenterText);
		}

		public void OnEnterMap(BaseEventData eventData)
		{
			inMap = true;

			if (m_ReadoutBar != null)
				m_ReadoutBar.SetActive(true);
		}

		public void OnExitMap(BaseEventData eventData)
		{
			inMap = false;

			if (m_ReadoutBar != null)
				m_ReadoutBar.SetActive(false);

			if (m_ReadoutText != null)
				m_ReadoutText.OnTextUpdate.Invoke("");
		}

		public void OnClickMap(BaseEventData eventData)
		{
			if (!inMap || zoomInterface == null || m_ZoomImage == null || !(eventData is PointerEventData))
				return;

			OnPointerDown((PointerEventData)eventData);

			Vector2 pos;

			RectTransformUtility.ScreenPointToLocalPointInRectangle(m_ZoomImage.rectTransform, Input.mousePosition, zoomInterface.MainCanvas.worldCamera, out pos);

			if (waypointSelecting)
			{
				if (tempWaypointLabel != null)
				{
					tempWaypointLabel.gameObject.SetActive(false);
					DestroyImmediate(tempWaypointLabel.gameObject);
					tempWaypointLabel = null;
				}

				SetWaypoint(pos);
			}
			else
			{
				zoomInterface.ClickMap((int)((PointerEventData)eventData).button, pos);

				ResetText();

				RefreshIcons();

				SetLegend(zoomInterface.LegendToggle, zoomInterface.LegendImage, zoomInterface.LegendLabels);
			}
		}

		private void SetWaypoint(Vector2 p)
		{
			Vector2 mapPos = new Vector2(p.x, p.y + zoomInterface.Size.y);

			MapLabelInfo info = new MapLabelInfo()
			{
				label = "",
				image = zoomInterface.WaypointSprite,
				pos = mapPos,
				baseColor = Color.white,
				flash = false,
				width = 20,
				alignBottom = true,
				show = true
			};

			tempWaypointLabel = createWaypoint(0, info, true);
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
			if (rect == null || zoomInterface == null)
				return;

			zoomInterface.Position = new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y);
		}

		public void OnStartResize(BaseEventData eventData)
		{
			if (m_MapLayout == null)
				return;

			if (!(eventData is PointerEventData))
				return;

			mouseStart = ((PointerEventData)eventData).position;
			resizeStart = new Vector2(m_MapLayout.preferredWidth, m_MapLayout.preferredHeight);

			ClearIcons();
		}

		public void OnResize(BaseEventData eventData)
		{
			if (m_MapLayout == null)
				return;

			if (!(eventData is PointerEventData))
				return;

			float width = resizeStart.x + (((PointerEventData)eventData).position.x - mouseStart.x);
			float height = resizeStart.y - (((PointerEventData)eventData).position.y - mouseStart.y);

			if (width < m_MapLayout.minWidth)
				width = m_MapLayout.minWidth;
			else if (width > m_MaxWidth)
				width = m_MaxWidth;

			if (height < m_MapLayout.minHeight)
				height = m_MapLayout.minHeight;
			else if (height > m_MaxHeight)
				height = m_MaxHeight;

			if (width % 2 != 0)
				width += 1;
			if (width % 4 != 0)
				width += 2;

			if (height % 2 != 0)
				height += 1;
			if (height % 4 != 0)
				height += 2;

			m_MapLayout.preferredWidth = width;
			m_MapLayout.preferredHeight = height;
		}

		public void OnEndResize(BaseEventData eventData)
		{
			if (m_MapLayout == null || zoomInterface == null)
				return;

			zoomInterface.Size = new Vector2(m_MapLayout.preferredWidth, m_MapLayout.preferredHeight);

			SetIcons();

			ResetText();
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

			dropDown.FadeOut();
			dropDown = null;

			if (m_DropDownToggles != null)
				m_DropDownToggles.SetAllTogglesOff();
		}

		public void UpdateTitle(string text)
		{
			if (m_Title == null)
				return;

			m_Title.OnTextUpdate.Invoke(text);
		}

		public void UpdateMapTexture(Texture2D map)
		{
			if (m_ZoomImage == null)
				return;

			m_ZoomImage.texture = map;
		}

		public void ToggleWindowState()
		{
			if (zoomInterface == null)
				return;

			int i = zoomInterface.WindowState + 1;

			if (i > 2)
				i = 0;

			zoomInterface.WindowState = i;

			SetWindowState(i);
		}
		
		public void ToggleTypeSelection(bool isOn)
		{
			if (dropDown != null)
			{
				dropDown.FadeOut(true);
				dropDown = null;
			}

			if (!isOn)
				return;

			if (zoomInterface == null || m_DropDownPrefab == null || m_MapTypeMenu == null)
				return;

			dropDown = Instantiate(m_DropDownPrefab).GetComponent<SCAN_DropDown>();

			if (dropDown == null)
				return;

			dropDown.transform.SetParent(m_MapTypeMenu, false);

			dropDown.Setup(zoomInterface.MapTypes, zoomInterface.CurrentMapType);

			dropDown.OnSelectUpdate.AddListener(new UnityEngine.Events.UnityAction<string>(SetType));
		}

		private void SetType(string selection)
		{
			if (zoomInterface == null)
				return;

			zoomInterface.CurrentMapType = selection;

			dropDown.FadeOut();
			dropDown = null;

			if (m_DropDownToggles != null)
				m_DropDownToggles.SetAllTogglesOff();

			RefreshIcons();

			SetLegend(zoomInterface.LegendToggle, zoomInterface.LegendImage, zoomInterface.LegendLabels);
		}

		public void ToggleResourceSelection(bool isOn)
		{
			if (dropDown != null)
			{
				dropDown.FadeOut(true);
				dropDown = null;
			}

			if (!isOn)
				return;

			if (zoomInterface == null || m_DropDownPrefab == null || m_ResourceMenu == null)
				return;

			dropDown = Instantiate(m_DropDownPrefab).GetComponent<SCAN_DropDown>();

			if (dropDown == null)
				return;

			dropDown.transform.SetParent(m_ResourceMenu, false);

			dropDown.Setup(zoomInterface.Resources, zoomInterface.CurrentResource);

			dropDown.OnSelectUpdate.AddListener(new UnityEngine.Events.UnityAction<string>(SetResource));
		}

		private void SetResource(string selection)
		{
			if (zoomInterface == null)
				return;

			zoomInterface.CurrentResource = selection;

			dropDown.FadeOut();
			dropDown = null;

			if (m_DropDownToggles != null)
				m_DropDownToggles.SetAllTogglesOff();

			RefreshIcons();
		}

		public void RefreshMap()
		{
			if (zoomInterface == null)
				return;

			zoomInterface.RefreshMap();

			RefreshIcons();
		}

		public void ToggleColor(bool isOn)
		{
			if (!loaded || zoomInterface == null)
				return;

			zoomInterface.ColorToggle = isOn;

			RefreshIcons();

			SetLegend(zoomInterface.LegendToggle, zoomInterface.LegendImage, zoomInterface.LegendLabels);
		}

		public void ToggleOrbit(bool isOn)
		{
			if (!loaded || zoomInterface == null)
				return;

			zoomInterface.OrbitToggle = isOn;

			RefreshIcons();
		}

		public void ToggleIcons(bool isOn)
		{
			if (!loaded || zoomInterface == null)
				return;

			zoomInterface.IconsToggle = isOn;

			RefreshIcons();
		}

		public void ToggleLegend(bool isOn)
		{
			if (!loaded || zoomInterface == null)
				return;

			zoomInterface.LegendToggle = isOn;

			SetLegend(isOn, zoomInterface.LegendImage, zoomInterface.LegendLabels);
		}

		public void ToggleResource(bool isOn)
		{
			if (!loaded || zoomInterface == null)
				return;

			zoomInterface.ResourceToggle = isOn;

			RefreshIcons();
		}

		public void SyncVessel()
		{
			if (zoomInterface == null)
				return;

			zoomInterface.VesselSync();
		}

		public void LockVessel()
		{
			if (zoomInterface == null)
				return;

			zoomInterface.VesselLock = !zoomInterface.VesselLock;

			if (m_VesselLockImage != null && m_VesselLock != null && m_VesselUnlock != null)
				m_VesselLockImage.sprite = zoomInterface.VesselLock ? m_VesselLock : m_VesselUnlock;

			if (m_MapMoveObject != null)
				m_MapMoveObject.SetActive(!zoomInterface.VesselLock);
		}

		public void ZoomOut()
		{
			if (zoomInterface == null)
				return;

			zoomInterface.ZoomMap(false);

			ResetText();

			RefreshIcons();

			SetLegend(zoomInterface.LegendToggle, zoomInterface.LegendImage, zoomInterface.LegendLabels);
		}

		public void ZoomIn()
		{
			if (zoomInterface == null)
				return;

			zoomInterface.ZoomMap(true);

			ResetText();

			RefreshIcons();

			SetLegend(zoomInterface.LegendToggle, zoomInterface.LegendImage, zoomInterface.LegendLabels);
		}

		public void MoveLeft()
		{
			if (zoomInterface == null)
				return;

			zoomInterface.MoveMap(0);

			ResetText();

			RefreshIcons();

			SetLegend(zoomInterface.LegendToggle, zoomInterface.LegendImage, zoomInterface.LegendLabels);
		}

		public void MoveRight()
		{
			if (zoomInterface == null)
				return;

			zoomInterface.MoveMap(1);

			ResetText();

			RefreshIcons();

			SetLegend(zoomInterface.LegendToggle, zoomInterface.LegendImage, zoomInterface.LegendLabels);
		}

		public void MoveUp()
		{
			if (zoomInterface == null)
				return;

			zoomInterface.MoveMap(2);

			ResetText();

			RefreshIcons();

			SetLegend(zoomInterface.LegendToggle, zoomInterface.LegendImage, zoomInterface.LegendLabels);
		}

		public void MoveDown()
		{
			if (zoomInterface == null)
				return;

			zoomInterface.MoveMap(3);

			ResetText();

			RefreshIcons();

			SetLegend(zoomInterface.LegendToggle, zoomInterface.LegendImage, zoomInterface.LegendLabels);
		}

		public void GenerateWaypoint()
		{
			waypointSelecting = !waypointSelecting;

			if (tempWaypointLabel != null)
			{
				tempWaypointLabel.gameObject.SetActive(false);
				DestroyImmediate(tempWaypointLabel.gameObject);
				tempWaypointLabel = null;
			}

			if (zoomInterface == null)
				return;

			zoomInterface.LockInput = false;

			if (m_WaypointBar != null)
				m_WaypointBar.SetActive(waypointSelecting);

			if (waypointSelecting && m_WaypointInput != null)
			{
				if (string.IsNullOrEmpty(waypoint))
					m_WaypointInput.text = zoomInterface.RandomWaypoint;
				else
					m_WaypointInput.text = waypoint;
			}
		}

		public void OnInputClick(BaseEventData eventData)
		{
			if (!(eventData is PointerEventData) || zoomInterface == null)
				return;

			if (((PointerEventData)eventData).button != PointerEventData.InputButton.Left)
				return;

			zoomInterface.LockInput = true;
		}

		public void SetWaypoint()
		{
			if (zoomInterface == null || m_WaypointInput == null)
				return;

			zoomInterface.LockInput = false;

			if (tempWaypointLabel != null)
				zoomInterface.SetWaypoint(m_WaypointInput.text, tempWaypointLabel.Info.pos);

			waypoint = "";

			waypointSelecting = false;

			if (tempWaypointLabel != null)
			{
				tempWaypointLabel.gameObject.SetActive(false);
				DestroyImmediate(tempWaypointLabel.gameObject);
				tempWaypointLabel = null;
			}

			if (m_WaypointBar != null)
				m_WaypointBar.SetActive(false);

			RefreshIcons();
		}

		public void CancelWaypoint()
		{
			if (zoomInterface != null)
				zoomInterface.LockInput = false;

			waypointSelecting = false;

			if (tempWaypointLabel != null)
			{
				tempWaypointLabel.gameObject.SetActive(false);
				DestroyImmediate(tempWaypointLabel.gameObject);
				tempWaypointLabel = null;
			}

			if (m_WaypointBar != null)
				m_WaypointBar.SetActive(false);

			RefreshIcons();
		}

		public void MechJebLanding()
		{
			if (zoomInterface != null)
				zoomInterface.LockInput = false;

			waypoint = "";

			waypointSelecting = false;

			if (tempWaypointLabel != null)
			{
				tempWaypointLabel.gameObject.SetActive(false);
				DestroyImmediate(tempWaypointLabel.gameObject);
				tempWaypointLabel = null;
			}

			if (m_WaypointBar != null)
				m_WaypointBar.SetActive(false);

			RefreshIcons();
		}

	}
}
