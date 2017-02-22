#region license
/* 
 * [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 *
 * SCAN_SettingsGeneral - Script for controlling the general settings page
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
using UnityEngine.EventSystems;
using SCANsat.Unity.Interfaces;

namespace SCANsat.Unity.Unity
{
	public class SCAN_SettingsGeneral : SettingsPage
	{
		[SerializeField]
		private SCAN_Toggle m_GroundTrackToggle = null;
		[SerializeField]
		private SCAN_Toggle m_GroundTrackActiveToggle = null;
		[SerializeField]
		private SCAN_Toggle m_WindowTooltipToggle = null;
		[SerializeField]
		private SCAN_Toggle m_StockToolbarToggle = null;
		[SerializeField]
		private SCAN_Toggle m_ToolbarMenuToggle = null;
		[SerializeField]
		private SCAN_Toggle m_StockUIToggle = null;
		[SerializeField]
		private SCAN_Toggle m_MapSpeedToggle = null;
		[SerializeField]
		private TextHandler m_UIScale = null;
		[SerializeField]
		private Slider m_UIScaleSlider = null;

		private bool loaded;
		private ISCAN_Settings settings;

		public void setup(ISCAN_Settings set)
		{
			if (set == null)
				return;

			settings = set;

			if (m_GroundTrackToggle != null)
				m_GroundTrackToggle.isOn = set.GroundTracks;

			if (m_GroundTrackActiveToggle != null)
			{
				m_GroundTrackActiveToggle.isOn = set.ActiveGround;
				m_GroundTrackActiveToggle.gameObject.SetActive(set.GroundTracks);
			}

			if (m_WindowTooltipToggle != null)
				m_WindowTooltipToggle.isOn = set.WindowTooltips;

			if (m_MapSpeedToggle != null)
				m_MapSpeedToggle.isOn = set.MapGenSpeed;

			if (m_StockToolbarToggle != null)
				m_StockToolbarToggle.isOn = set.StockToolbar;

			if (m_ToolbarMenuToggle != null)
			{
				m_ToolbarMenuToggle.isOn = set.ToolbarMenu;
				m_ToolbarMenuToggle.gameObject.SetActive(set.StockToolbar);
			}

			if (m_StockUIToggle != null)
				m_StockUIToggle.isOn = set.StockUIStyle;

			if (m_UIScale != null)
				m_UIScale.OnTextUpdate.Invoke("UI Scale: " + set.UIScale.ToString("P0"));

			if (m_UIScaleSlider != null)
				m_UIScaleSlider.value = set.UIScale * 100;

			loaded = true;
		}

		public void GroundTrack(bool isOn)
		{
			if (!loaded || settings == null)
				return;

			settings.GroundTracks = isOn;

			if (m_GroundTrackActiveToggle != null)
				m_GroundTrackActiveToggle.gameObject.SetActive(isOn);
		}

		public void ActiveTrackOnly(bool isOn)
		{
			if (!loaded || settings == null)
				return;

			settings.ActiveGround = isOn;
		}

		public void WindowTooltip(bool isOn)
		{
			if (!loaded || settings == null)
				return;

			settings.WindowTooltips = isOn;
		}

		public void SlowMapSpeed(bool isOn)
		{
			if (!loaded || settings == null)
				return;

			settings.MapGenSpeed = isOn;
		}

		public void StockToolbar(bool isOn)
		{
			if (!loaded || settings == null)
				return;

			settings.StockToolbar = isOn;

			if (m_ToolbarMenuToggle != null)
				m_ToolbarMenuToggle.gameObject.SetActive(isOn);
		}

		public void ToolbarMenu(bool isOn)
		{
			if (!loaded || settings == null)
				return;

			settings.ToolbarMenu = isOn;
		}

		public void StockUIStlye(bool isOn)
		{
			if (!loaded || settings == null)
				return;

			settings.StockUIStyle = isOn;
		}

		public void UISlider(float scale)
		{
			if (!loaded || m_UIScale == null)
				return;

			m_UIScale.OnTextUpdate.Invoke("UI Scale: " + (scale / 100).ToString("P0"));
		}

		public void SetUIScale()
		{
			if (settings == null || m_UIScaleSlider == null)
				return;

			settings.UIScale = m_UIScaleSlider.value / 100;
		}

		public void ResetWindows()
		{
			if (settings == null)
				return;

			settings.ResetWindows();
		}
	}
}
