#region license
/* 
 * [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 *
 * SCAN_SettingsData - Script for controlling the data management settings page
 * 
 * Copyright (c)2014 David Grandy <david.grandy@gmail.com>;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
 */
#endregion

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using SCANsat.Unity.Interfaces;

namespace SCANsat.Unity.Unity
{
	public class SCAN_SettingsData : SettingsPage
	{
		[SerializeField]
		private SCAN_Toggle m_GreyScaleToggle = null;
		[SerializeField]
		private SCAN_Toggle m_CSVExportToggle = null;
		[SerializeField]
		private TextHandler m_MapWidth = null;
		[SerializeField]
		private TextHandler m_ResetCurrentText = null;
		[SerializeField]
		private TextHandler m_ResetCurrentSCANText = null;
		[SerializeField]
		private TextHandler m_ResetCurrentStockText = null;
		[SerializeField]
		private TextHandler m_FillCurrentText = null;
		[SerializeField]
		private InputField m_MapWidthInput = null;
		[SerializeField]
		private GameObject m_ResetSCANResource = null;
		[SerializeField]
		private GameObject m_ResetStockResource = null;
		[SerializeField]
		private GameObject m_MapFill = null;

		private bool loaded;
		private ISCAN_Settings settings;

		private void Update()
		{
			if (settings == null)
				return;

			if (settings.LockInput)
			{
				if (m_MapWidthInput != null && !m_MapWidthInput.isFocused)
					settings.LockInput = false;
			}
		}

		public void setup(ISCAN_Settings set)
		{
			if (set == null)
				return;

			settings = set;

			if (m_GreyScaleToggle != null)
				m_GreyScaleToggle.isOn = set.GreyScale;

			if (m_CSVExportToggle != null)
				m_CSVExportToggle.isOn = set.ExportCSV;

			if (m_MapWidth != null)
				m_MapWidth.OnTextUpdate.Invoke("Map Width: " + set.MapWidth.ToString());

			if (!set.DisableStock)
			{
				if (set.InstantScan)
				{
					if (m_ResetSCANResource != null)
						m_ResetSCANResource.SetActive(false);

					if (m_ResetStockResource != null)
						m_ResetStockResource.SetActive(false);
				}
			}

			if (!set.ShowMapFill && m_MapFill != null)
				m_MapFill.SetActive(false);

			SetButtonText();

			loaded = true;
		}

		public void SetButtonText()
		{
			if (settings == null)
				return;

			if (m_ResetCurrentText != null)
				m_ResetCurrentText.OnTextUpdate.Invoke("Reset Map of " + settings.CurrentBody);

			if (m_ResetCurrentSCANText != null)
				m_ResetCurrentSCANText.OnTextUpdate.Invoke("Reset SCANsat resource data of " + settings.CurrentBody);

			if (m_ResetCurrentStockText != null)
				m_ResetCurrentStockText.OnTextUpdate.Invoke("Reset stock resource data for " + settings.CurrentBody);

			if (m_FillCurrentText != null)
				m_FillCurrentText.OnTextUpdate.Invoke("Fill map of " + settings.CurrentBody);
		}

		public void GreyScale(bool isOn)
		{
			if (!loaded || settings == null)
				return;

			settings.GreyScale = isOn;
		}

		public void ExportCSV(bool isOn)
		{
			if (!loaded || settings == null)
				return;

			settings.ExportCSV = isOn;
		}

		public void SetMapWidth()
		{
			if (settings == null || m_MapWidthInput == null)
				return;

			settings.LockInput = false;

			int width = settings.MapWidth;

			if (int.TryParse(m_MapWidthInput.text, out width))
			{
				if (width % 2 != 0)
					width += 1;

				if (width > 8192)
					width = 8192;
				else if (width < 560)
					width = 560;

				m_MapWidthInput.text = width.ToString();

				settings.MapWidth = width;

				if (m_MapWidth != null)
					m_MapWidth.OnTextUpdate.Invoke("Map Width: " + width.ToString());
			}
		}

		public void OnInputClick(BaseEventData eventData)
		{
			if (!(eventData is PointerEventData) || settings == null)
				return;

			if (((PointerEventData)eventData).button != PointerEventData.InputButton.Left)
				return;

			settings.LockInput = true;
		}

		private void PopupPopup(string message, UnityAction callback)
		{
			if (SCAN_Settings.Instance == null)
				return;

			if (SCAN_Settings.Instance.WarningPopup != null)
			{
				SCAN_Settings.Instance.WarningPopup.FadeOut(true);
				SCAN_Settings.Instance.WarningPopup = null;
			}

			if (SCAN_Settings.Instance.PopupPrefab == null)
				return;

			SCAN_Settings.Instance.WarningPopup = Instantiate(SCAN_Settings.Instance.PopupPrefab).GetComponent<SCAN_Popup>();

			if (SCAN_Settings.Instance.WarningPopup == null)
				return;

			SCAN_Settings.Instance.WarningPopup.transform.SetParent(transform, false);

			SCAN_Settings.Instance.WarningPopup.Setup(message);

			SCAN_Settings.Instance.WarningPopup.OnSelectUpdate.AddListener(callback);
		}

		public void ResetCurrentMap()
		{
			if (settings != null)
				PopupPopup(settings.DataResetCurrent, ConfirmResetCurrentMap);
		}

		private void ConfirmResetCurrentMap()
		{
			SCAN_Settings.Instance.WarningPopup.FadeOut(true);
			SCAN_Settings.Instance.WarningPopup = null;

			if (settings == null)
				return;

			settings.ResetCurrent();
		}

		public void ResetAllMaps()
		{
			if (settings != null)
				PopupPopup(settings.DataResetAll, ConfirmResetAllMaps);
		}

		private void ConfirmResetAllMaps()
		{
			SCAN_Settings.Instance.WarningPopup.FadeOut(true);
			SCAN_Settings.Instance.WarningPopup = null;

			if (settings == null)
				return;

			settings.ResetAll();
		}

		public void ResetSCANResourceCurrent()
		{
			if (settings != null)
				PopupPopup(settings.SCANResourceResetCurrent, ConfirmResetSCANResourceCurrent);
		}

		private void ConfirmResetSCANResourceCurrent()
		{
			SCAN_Settings.Instance.WarningPopup.FadeOut(true);
			SCAN_Settings.Instance.WarningPopup = null;

			if (settings == null)
				return;

			settings.ResetSCANResourceCurrent();
		}

		public void ResetSCANResourceAll()
		{
			if (settings != null)
				PopupPopup(settings.SCANResourceResetAll, ConfirmResetSCANResourceAll);
		}

		private void ConfirmResetSCANResourceAll()
		{
			SCAN_Settings.Instance.WarningPopup.FadeOut(true);
			SCAN_Settings.Instance.WarningPopup = null;

			if (settings == null)
				return;

			settings.ResetSCANResourceCurrent();
		}

		public void ResetStockResourceCurrent()
		{
			if (settings != null)
				PopupPopup(settings.StockResourceResetCurrent, ConfirmResetStockResourceCurrent);
		}

		private void ConfirmResetStockResourceCurrent()
		{
			SCAN_Settings.Instance.WarningPopup.FadeOut(true);
			SCAN_Settings.Instance.WarningPopup = null;

			if (settings == null)
				return;

			settings.ResetStockResourceCurrent();
		}

		public void ResetStockResourceAll()
		{
			if (settings != null)
				PopupPopup(settings.StockResourceResetAll, ConfirmResetStockResourceAll);
		}

		private void ConfirmResetStockResourceAll()
		{
			SCAN_Settings.Instance.WarningPopup.FadeOut(true);
			SCAN_Settings.Instance.WarningPopup = null;

			if (settings == null)
				return;

			settings.ResetStockResourceAll();
		}

		public void FillCurrentMap()
		{
			if (settings == null)
				return;

			settings.FillCurrent();
		}

		public void FillAllMaps()
		{
			if (settings == null)
				return;

			settings.FillAll();
		}
	}
}
