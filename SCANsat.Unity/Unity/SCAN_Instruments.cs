using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SCANsat.Unity.Interfaces;

namespace SCANsat.Unity.Unity
{
	public class SCAN_Instruments : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerDownHandler
	{
		[SerializeField]
		private TextHandler m_Version = null;
		[SerializeField]
		private TextHandler m_ReadoutText = null;
		[SerializeField]
		private Transform m_ResourceButtons = null;
		[SerializeField]
		private RawImage m_AnomalyImage = null;
		[SerializeField]
		private TextHandler m_AnomalyPrintText = null;
		[SerializeField]
		private TextHandler m_AnomalyNameText = null;
		[SerializeField]
		private GameObject m_AnomalyObject = null;

		private ISCAN_Instruments insInterface;
		private RectTransform rect;
		private Vector2 mouseStart;
		private Vector3 windowStart;

		private void Awake()
		{
			rect = GetComponent<RectTransform>();
		}

		private void Update()
		{
			if (insInterface == null || !insInterface.IsVisible)
				return;

			insInterface.Update();
		}

		public void SetInstruments(ISCAN_Instruments ins)
		{
			if (ins == null)
				return;

			insInterface = ins;

			if (m_Version != null)
				m_Version.OnTextUpdate.Invoke(ins.Version);

			if (!ins.ResourceButtons && m_ResourceButtons != null)
				m_ResourceButtons.gameObject.SetActive(false);

			SetScale(ins.Scale);

			SetPosition(ins.Position);
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

			if (insInterface == null)
				return;

			insInterface.ClampToScreen(rect);
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			if (rect == null || insInterface == null)
				return;

			insInterface.Position = new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y);
		}

		public void UpdateText(string s)
		{
			if (m_ReadoutText == null)
				return;

			m_ReadoutText.OnTextUpdate.Invoke(s);
		}

		public void UpdateAnomaly(Texture2D tex)
		{
			if (m_AnomalyImage == null)
				return;

			m_AnomalyImage.texture = tex;
		}

		public void UpdateAnomalyText(string s)
		{
			if (m_AnomalyPrintText == null)
				return;

			m_AnomalyPrintText.OnTextUpdate.Invoke(s);
		}

		public void UpdateAnomalyName(string s)
		{
			if (m_AnomalyNameText == null)
				return;

			m_AnomalyNameText.OnTextUpdate.Invoke(s);
		}

		public void Close()
		{
			if (insInterface == null)
				return;

			insInterface.IsVisible = false;
		}

		public void PreviousResource()
		{
			if (insInterface == null)
				return;

			insInterface.PreviousResource();
		}

		public void NextResource()
		{
			if (insInterface == null)
				return;

			insInterface.NextResource();
		}

		public void SetDetailState(bool isOn)
		{
			if (m_AnomalyObject == null)
				return;

			if (isOn && !m_AnomalyObject.activeSelf)
				m_AnomalyObject.SetActive(true);
			else if (!isOn && m_AnomalyObject.activeSelf)
				m_AnomalyObject.SetActive(false);
		}

	}
}
