using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SCANsat.Unity.Interfaces;

namespace SCANsat.Unity.Unity
{
	public class SCAN_SettingsData : SettingsPage
	{
		[SerializeField]
		private Toggle m_GreyScaleToggle = null;
		[SerializeField]
		private Toggle m_CSVExportToggle = null;
		[SerializeField]
		private TextHandler m_MapWidth = null;
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

			loaded = true;
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

			int width = settings.MapWidth;

			if (int.TryParse(m_MapWidthInput.text, out width))
			{
				if (width % 2 != 0)
					width += 1;

				settings.MapWidth = width;

				if (m_MapWidth != null)
					m_MapWidth.OnTextUpdate.Invoke("Map Width: " + width.ToString());
			}
		}

		public void ResetCurrentMap()
		{
			if (settings == null)
				return;

			settings.ResetCurrent();
		}

		public void ResetAllMaps()
		{
			if (settings == null)
				return;

			settings.ResetAll();
		}

		public void ResetSCANResource()
		{
			if (settings == null)
				return;

			settings.ResetSCANResource();
		}

		public void ResetStockResource()
		{
			if (settings == null)
				return;

			settings.ResetStockResource();
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
