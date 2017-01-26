using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SCANsat.Unity.Interfaces;

namespace SCANsat.Unity.Unity
{
	public class SCAN_Settings : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerDownHandler
	{
		[SerializeField]
		private TextHandler m_Version = null;
		[SerializeField]
		private Transform m_ContentTransform = null;
		[SerializeField]
		private Toggle m_GeneralToggle = null;
		[SerializeField]
		private Toggle m_BackgroundToggle = null;
		[SerializeField]
		private Toggle m_ResourceToggle = null;
		[SerializeField]
		private Toggle m_DataToggle = null;
		[SerializeField]
		private GameObject m_GeneralPrefab = null;
		[SerializeField]
		private GameObject m_BackgroundPrefab = null;
		[SerializeField]
		private GameObject m_ResourcePrefab = null;
		[SerializeField]
		private GameObject m_DataPrefab = null;

		private ISCAN_Settings settingsInterface;
		private RectTransform rect;
		private Vector2 mouseStart;
		private Vector3 windowStart;
		private int _page;

		private SettingsPage CurrentPage;

		public int Page
		{
			get { return _page; }
		}

		private void Awake()
		{
			rect = GetComponent<RectTransform>();
		}

		private void Update()
		{
			if (settingsInterface == null || !settingsInterface.IsVisible)
				return;

			settingsInterface.Update();
		}

		public void setSettings(ISCAN_Settings settings, int page)
		{
			if (settings == null)
				return;

			settingsInterface = settings;

			if (m_Version != null)
				m_Version.OnTextUpdate.Invoke(settings.Version);

			_page = page;

			switch (page)
			{
				case 0:
					if (m_GeneralToggle != null)
						m_GeneralToggle.isOn = true;
					break;
				case 1:
					if (m_BackgroundToggle != null)
						m_BackgroundToggle.isOn = true;
					break;
				case 2:
					if (m_ResourceToggle != null)
						m_ResourceToggle.isOn = true;
					break;
				case 3:
					if (m_DataToggle != null)
						m_DataToggle.isOn = true;
					break;
				default:
					if (m_GeneralToggle != null)
						m_GeneralToggle.isOn = true;
					break;
			}

			SetScale(settings.UIScale);
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

			if (settingsInterface == null)
				return;

			settingsInterface.ClampToScreen(rect);
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			if (rect == null || settingsInterface == null)
				return;

			settingsInterface.Position = new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y);
		}

		public void Close()
		{
			if (settingsInterface == null)
				return;

			settingsInterface.IsVisible = false;
		}

		public void GeneralSettings(bool isOn)
		{
			if (CurrentPage != null)
			{
				CurrentPage.gameObject.SetActive(false);
				DestroyImmediate(CurrentPage.gameObject);
			}

			if (m_GeneralPrefab == null || m_ContentTransform == null || settingsInterface == null)
				return;

			CurrentPage = Instantiate(m_GeneralPrefab).GetComponent<SettingsPage>();

			if (CurrentPage == null)
				return;

			_page = 0;

			CurrentPage.transform.SetParent(m_ContentTransform, false);

			((SCAN_SettingsGeneral)CurrentPage).setup(settingsInterface);
		}

		public void BackgroundSettings(bool isOn)
		{
			if (CurrentPage != null)
			{
				CurrentPage.gameObject.SetActive(false);
				DestroyImmediate(CurrentPage.gameObject);
			}

			if (m_BackgroundPrefab == null || m_ContentTransform == null || settingsInterface == null)
				return;

			CurrentPage = Instantiate(m_BackgroundPrefab).GetComponent<SettingsPage>();

			if (CurrentPage == null)
				return;

			_page = 1;

			CurrentPage.transform.SetParent(m_ContentTransform, false);

			((SCAN_SettingsBackground)CurrentPage).setup(settingsInterface);
		}

		public void ResourceSettings(bool isOn)
		{
			if (CurrentPage != null)
			{
				CurrentPage.gameObject.SetActive(false);
				DestroyImmediate(CurrentPage.gameObject);
			}

			if (m_ResourcePrefab == null || m_ContentTransform == null || settingsInterface == null)
				return;

			CurrentPage = Instantiate(m_ResourcePrefab).GetComponent<SettingsPage>();

			if (CurrentPage == null)
				return;

			_page = 2;

			CurrentPage.transform.SetParent(m_ContentTransform, false);

			((SCAN_SettingsResource)CurrentPage).setup(settingsInterface);
		}

		public void DataSettings(bool isOn)
		{
			if (CurrentPage != null)
			{
				CurrentPage.gameObject.SetActive(false);
				DestroyImmediate(CurrentPage.gameObject);
			}

			if (m_DataPrefab == null || m_ContentTransform == null || settingsInterface == null)
				return;

			CurrentPage = Instantiate(m_DataPrefab).GetComponent<SettingsPage>();

			if (CurrentPage == null)
				return;

			_page = 3;

			CurrentPage.transform.SetParent(m_ContentTransform, false);

			((SCAN_SettingsData)CurrentPage).setup(settingsInterface);
		}

		public void ColorSettings(bool isOn)
		{
			if (settingsInterface == null)
				return;

			settingsInterface.OpenColor();
		}

	}
}
