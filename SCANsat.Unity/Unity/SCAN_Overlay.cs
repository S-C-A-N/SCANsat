using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SCANsat.Unity.Interfaces;

namespace SCANsat.Unity.Unity
{
	public class SCAN_Overlay : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
	{
		[SerializeField]
		private TextHandler m_Version = null;
		[SerializeField]
		private Toggle m_BiomeToggle = null;
		[SerializeField]
		private Toggle m_TerrainToggle = null;
		[SerializeField]
		private Toggle m_OverlayToggle = null;
		[SerializeField]
		private Toggle m_TooltipToggle = null;
		[SerializeField]
		private GameObject m_ResourcePrefab = null;
		[SerializeField]
		private Transform m_ResourceTransform = null;

		private ISCAN_Overlay overInterface;
		private bool loaded;
		private RectTransform rect;
		private Vector2 mouseStart;
		private Vector3 windowStart;

		private void Awake()
		{
			rect = GetComponent<RectTransform>();
		}

		private void Update()
		{
			if (overInterface == null || !overInterface.IsVisible)
				return;

			overInterface.Update();
		}

		public void SetOverlay(ISCAN_Overlay over)
		{
			if (over == null)
				return;

			overInterface = over;

			if (m_Version != null)
				m_Version.OnTextUpdate.Invoke(over.Version);

			if (m_BiomeToggle == null || m_TerrainToggle == null || m_OverlayToggle == null)
				return;

			m_BiomeToggle.isOn = over.DrawBiome;
			m_TerrainToggle.isOn = over.DrawTerrain;
			m_OverlayToggle.isOn = over.DrawOverlay;

			CreateResources(over.Resources);

			SetScale(over.Scale);

			SetPosition(over.Position);

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

			if (overInterface == null)
				return;

			overInterface.ClampToScreen(rect);
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			if (rect == null || overInterface == null)
				return;

			overInterface.Position = new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y);
		}

		private void CreateResources(IList<string> resources)
		{
			if (resources == null)
				return;

			if (m_ResourcePrefab == null || m_ResourceTransform == null)
				return;

			for (int i = resources.Count - 1; i >= 0; i--)
			{
				string s = resources[i];

				CreateResource(s);
			}
		}

		private void CreateResource(string resource)
		{
			SCAN_ResourceOverlay res = Instantiate(m_ResourcePrefab).GetComponent<SCAN_ResourceOverlay>();

			if (res == null)
				return;

			res.transform.SetParent(m_ResourceTransform, false);

			res.SetResource(resource, this, overInterface.CurrentResource == resource);
		}

		public void Close()
		{
			if (overInterface == null)
				return;

			overInterface.IsVisible = false;
		}

		public void SetResource(string resource, bool isOn)
		{
			if (overInterface == null)
				return;

			overInterface.DrawResource(resource, isOn);
		}

		public void ToggleBiome(bool isOn)
		{
			if (!loaded || overInterface == null)
				return;

			overInterface.DrawBiome = isOn;
		}

		public void ToggleTerrain(bool isOn)
		{
			if (!loaded || overInterface == null)
				return;

			overInterface.DrawTerrain = isOn;
		}

		public void DrawOverlay(bool isOn)
		{
			if (!loaded || overInterface == null)
				return;

			overInterface.DrawOverlay = isOn;
		}

		public void Refresh()
		{
			if (overInterface == null)
				return;

			overInterface.Refresh();
		}

		public void Settings()
		{
			if (overInterface == null)
				return;

			overInterface.OpenSettings();
		}
	}
}
