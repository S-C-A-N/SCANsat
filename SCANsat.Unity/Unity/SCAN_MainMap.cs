using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SCANsat.Unity.Interfaces;

namespace SCANsat.Unity.Unity
{
	public class SCAN_MainMap : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerDownHandler
	{
		[SerializeField]
		private TextHandler m_Version = null;
		[SerializeField]
		private Toggle m_ColorToggle = null;
		[SerializeField]
		private Toggle m_TypeToggle = null;
		[SerializeField]
		private Toggle m_MinimizeToggle = null;
		[SerializeField]
		private TextHandler m_MinimizeText = null;
		[SerializeField]
		private TextHandler m_TypeLabel = null;
		[SerializeField]
		private RawImage m_MainMap = null;
		[SerializeField]
		private TextHandler m_LoText = null;
		[SerializeField]
		private TextHandler m_HiText = null;
		[SerializeField]
		private TextHandler m_MultiText = null;
		[SerializeField]
		private TextHandler m_M700Text = null;
		[SerializeField]
		private TextHandler m_OreText = null;
		[SerializeField]
		private TextHandler m_PercentageText = null;
		[SerializeField]
		private GameObject m_VesselPrefab = null;
		[SerializeField]
		private Transform m_VesselTransform = null;
		[SerializeField]
		private GameObject m_MapPrefab = null;

		private ISCAN_MainMap mapInterface;
		private bool loaded;
		private RectTransform rect;
		private Vector2 mouseStart;
		private Vector3 windowStart;
		private List<SCAN_VesselInfo> vessels = new List<SCAN_VesselInfo>();
		private List<SCAN_MapLabel> mapLabels = new List<SCAN_MapLabel>();

		private void Awake()
		{
			rect = GetComponent<RectTransform>();
		}

		private void Update()
		{
			if (mapInterface == null || !mapInterface.IsVisible)
				return;

			mapInterface.Update();

			if (!mapInterface.Minimized)
			{
				for (int i = vessels.Count - 1; i >= 0; i--)
				{
					SCAN_VesselInfo vessel = vessels[i];

					vessel.UpdateText(mapInterface.VesselInfo(vessel.ID));
				}
			}

			for (int i = mapLabels.Count - 1; i >= 0; i--)
			{
				SCAN_MapLabel mapLabel = mapLabels[i];

				mapLabel.UpdatePosition(mapInterface.VesselPosition(mapLabel.ID));
			}
		}

		public void setMap(ISCAN_MainMap map)
		{
			if (map == null)
				return;

			mapInterface = map;

			if (m_Version != null)
				m_Version.OnTextUpdate.Invoke(map.Version);

			if (/*m_ColorToggle == null || */m_MinimizeToggle == null || m_TypeToggle == null)
				return;

			//m_ColorToggle.isOn = map.Color;
			m_TypeToggle.isOn = map.MapType;
			m_MinimizeToggle.isOn = map.Minimized;

			CreateVessels(map.VesselInfoList);

			SetScale(map.Scale);

			SetPosition(map.Position);

			if (m_VesselTransform != null)
				m_VesselTransform.gameObject.SetActive(!map.Minimized);

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

		public void RefreshVesselTypes()
		{
			if (mapInterface == null)
				return;

			for (int i = mapLabels.Count - 1; i >= 0; i--)
			{
				SCAN_MapLabel mapLabel = mapLabels[i];

				mapLabel.UpdateImage(mapInterface.VesselType(mapLabel.ID));
			}
		}

		public void RefreshVessels()
		{
			for (int i = vessels.Count - 1; i >= 0; i--)
			{
				SCAN_VesselInfo v = vessels[i];

				v.gameObject.SetActive(false);
				Destroy(v.gameObject);
			}

			for (int i = mapLabels.Count - 1; i >= 0; i--)
			{
				SCAN_MapLabel m = mapLabels[i];

				m.gameObject.SetActive(false);
				Destroy(m.gameObject);
			}

			vessels.Clear();
			mapLabels.Clear();

			if (mapInterface != null)
				CreateVessels(mapInterface.VesselInfoList);
		}

		private void CreateVessels(Dictionary<Guid, MapLabelInfo> vessels)
		{
			if (vessels == null)
				return;

			if (mapInterface == null || m_VesselPrefab == null || m_MapPrefab == null || m_VesselTransform == null || m_MainMap == null)
				return;

			for (int i = 0; i < vessels.Count; i++)
			{
				Guid id = vessels.ElementAt(i).Key;

				MapLabelInfo label;

				if (!vessels.TryGetValue(id, out label))
					continue;

				CreateVessel(id, label);

				CreateMapLabel(id, label);
			}
		}

		private void CreateVessel(Guid id, MapLabelInfo info)
		{
			SCAN_VesselInfo vInfo = Instantiate(m_VesselPrefab).GetComponent<SCAN_VesselInfo>();

			if (vInfo == null)
				return;

			vInfo.transform.SetParent(m_VesselTransform, false);

			vInfo.SetVessel(id, info, mapInterface);

			vessels.Add(vInfo);
		}

		private void CreateMapLabel(Guid id, MapLabelInfo info)
		{
			SCAN_MapLabel mapLabel = Instantiate(m_MapPrefab).GetComponent<SCAN_MapLabel>();

			if (mapLabel == null)
				return;

			mapLabel.transform.SetParent(m_MainMap.transform, false);

			mapLabel.Setup(id, info);

			mapLabels.Add(mapLabel);
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			transform.SetAsLastSibling();
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
			
			if (mapInterface == null)
				return;

			mapInterface.ClampToScreen(rect);
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			if (rect == null || mapInterface == null)
				return;

			mapInterface.Position = new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y);
		}

		public void UpdateLoColor(Color c)
		{
			if (m_LoText == null)
				return;

			m_LoText.OnColorUpdate.Invoke(c);
		}

		public void UpdateHiColor(Color c)
		{
			if (m_HiText == null)
				return;

			m_HiText.OnColorUpdate.Invoke(c);
		}

		public void UpdateMultiColor(Color c)
		{
			if (m_MultiText == null)
				return;

			m_MultiText.OnColorUpdate.Invoke(c);
		}

		public void UpdateM700Color(Color c)
		{
			if (m_M700Text == null)
				return;

			m_M700Text.OnColorUpdate.Invoke(c);
		}

		public void UpdateOreColor(Color c)
		{
			if (m_OreText == null)
				return;

			m_OreText.OnColorUpdate.Invoke(c);
		}

		public void UpdatePercentage(string text)
		{
			if (m_PercentageText == null)
				return;

			m_PercentageText.OnTextUpdate.Invoke(text);
		}

		public void UpdateMapTexture(Texture2D map)
		{
			if (m_MainMap == null)
				return;

			m_MainMap.texture = map;
		}

		public void ToggleColor(bool isOn)
		{
			if (!loaded || mapInterface == null)
				return;

			mapInterface.Color = isOn;
		}

		public void ToggleType(bool isOn)
		{
			if (m_TypeLabel != null)
				m_TypeLabel.OnTextUpdate.Invoke(isOn ? "Biome" : "Terrain");

			if (!loaded || mapInterface == null)
				return;

			mapInterface.MapType = isOn;
		}

		public void ToggleSize(bool isOn)
		{
			if (m_MinimizeText != null)
				m_MinimizeText.OnTextUpdate.Invoke(isOn ? "+" : "-");

			if (!loaded || mapInterface == null)
				return;

			mapInterface.Minimized = isOn;

			if (m_VesselTransform != null)
				m_VesselTransform.gameObject.SetActive(!isOn);
		}

		public void Close()
		{
			if (mapInterface == null)
				return;

			mapInterface.IsVisible = false;
		}

		public void OpenBigMap()
		{
			if (mapInterface == null)
				return;

			mapInterface.OpenBigMap();
		}

		public void OpenInstruments()
		{
			if (mapInterface == null)
				return;

			mapInterface.OpenInstruments();
		}

		public void OpenSettings()
		{
			if (mapInterface == null)
				return;

			mapInterface.OpenSettings();
		}

		public void OpenZoomMap()
		{
			if (mapInterface == null)
				return;

			mapInterface.OpenZoomMap();
		}

		public void OpenOverlay()
		{
			if (mapInterface == null)
				return;

			mapInterface.OpenOverlay();
		}
	}
}
