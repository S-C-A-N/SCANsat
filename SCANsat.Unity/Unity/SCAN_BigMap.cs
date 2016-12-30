using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SCANsat.Unity.Interfaces;

namespace SCANsat.Unity.Unity
{
	public class SCAN_BigMap : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
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
		private GameObject m_DropDownPrefab = null;
		[SerializeField]
		private GameObject m_OrbitObject = null;
		[SerializeField]
		private GameObject m_WaypointObject = null;
		[SerializeField]
		private Toggle m_ColorToggle = null;
		[SerializeField]
		private Toggle m_GridToggle = null;
		[SerializeField]
		private Toggle m_OrbitToggle = null;
		[SerializeField]
		private Toggle m_WaypointToggle = null;
		[SerializeField]
		private Toggle m_AnomalyToggle = null;
		[SerializeField]
		private Toggle m_FlagToggle = null;
		[SerializeField]
		private Toggle m_AsteroidToggle = null;
		[SerializeField]
		private Toggle m_LegendToggle = null;
		[SerializeField]
		private Toggle m_ResourcesToggle = null;
		[SerializeField]
		private TextHandler m_ReadoutText = null;
		[SerializeField]
		private RawImage m_MapImage = null;
		[SerializeField]
		private LayoutElement m_MapLayout = null;

		private ISCAN_BigMap bigInterface;
		private bool loaded;
		private bool resizing;
		private RectTransform rect;
		private Vector2 mouseStart;
		private Vector3 windowStart;

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
		}

		private void OnGUI()
		{
			if (bigInterface == null || !bigInterface.IsVisible)
				return;

			bigInterface.OnGUI();
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

			SetScale(map.Scale);

			SetPosition(map.Position);

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
		}

		public void OnResize(BaseEventData eventData)
		{
			if (m_MapLayout == null)
				return;

			if (!(eventData is PointerEventData))
				return;

			m_MapLayout.preferredWidth += ((PointerEventData)eventData).delta.x;

			if (m_MapLayout.preferredWidth % 2 != 0)
				m_MapLayout.preferredWidth += 1;

			m_MapLayout.preferredHeight = m_MapLayout.preferredWidth / 2;
		}

		public void OnEndResize(BaseEventData eventData)
		{
			resizing = false;

			if (m_MapLayout == null || bigInterface == null)
				return;

			bigInterface.Size = new Vector2(m_MapLayout.preferredWidth, m_MapLayout.preferredHeight);
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
		}

		public void RefreshMap()
		{
			if (bigInterface == null)
				return;

			bigInterface.RefreshMap();
		}

		public void ToggleColor(bool isOn)
		{
			if (!loaded || bigInterface == null)
				return;

			bigInterface.ColorToggle = isOn;
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
		}

		public void ToggleWaypoint(bool isOn)
		{
			if (!loaded || bigInterface == null)
				return;

			bigInterface.WaypointToggle = isOn;
		}

		public void ToggleAnomaly(bool isOn)
		{
			if (!loaded || bigInterface == null)
				return;

			bigInterface.AnomalyToggle = isOn;
		}

		public void ToggleFlag(bool isOn)
		{
			if (!loaded || bigInterface == null)
				return;

			bigInterface.FlagToggle = isOn;
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
		}

		public void ToggleResource(bool isOn)
		{
			if (!loaded || bigInterface == null)
				return;

			bigInterface.ResourceToggle = isOn;
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

			bigInterface.OpenSettings();
		}

		public void ExportMap()
		{
			if (bigInterface == null)
				return;

			bigInterface.ExportMap();
		}

	}
}
