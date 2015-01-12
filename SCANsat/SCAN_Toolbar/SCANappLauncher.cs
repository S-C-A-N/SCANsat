﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SCANsat.Platform;
using SCANsat.SCAN_UI;
using SCANsat.SCAN_UI.UI_Framework;

namespace SCANsat.SCAN_Toolbar
{
	class SCANappLauncher : SCAN_MBE
	{
		private ApplicationLauncherButton SCANappLauncherButton = null;

		internal override void Start()
		{
			setupToolbar();
		}

		internal override void OnDestroy()
		{
			GameEvents.onGUIApplicationLauncherUnreadifying.Remove(removeButton);

			if (SCANappLauncherButton != null)
				removeButton(HighLogic.LoadedScene);
		}

		private void setupToolbar()
		{
			SCANUtil.SCANlog("Starting App Launcher Manager");
			StartCoroutine(addButton());
		}

		IEnumerator addButton()
		{
			SCANUtil.SCANlog("Waiting For Application Launcher...");

			while (!ApplicationLauncher.Ready)
				yield return null;

			if (HighLogic.LoadedScene == GameScenes.FLIGHT)
				SCANappLauncherButton = ApplicationLauncher.Instance.AddModApplication(toggleFlightOn, toggleFlightOff, null, null, null, null, ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW, SCANskins.SCAN_SmallMapIcon);
			else if (HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedScene == GameScenes.TRACKSTATION)
				SCANappLauncherButton = ApplicationLauncher.Instance.AddModApplication(toggleKSCOn, toggleKSCOff, null, null, null, null, ApplicationLauncher.AppScenes.SPACECENTER | ApplicationLauncher.AppScenes.TRACKSTATION, SCANskins.SCAN_BigMapIcon);

			GameEvents.onGUIApplicationLauncherUnreadifying.Add(removeButton);
		}

		private void removeButton(GameScenes scene)
		{
			ApplicationLauncher.Instance.RemoveModApplication(SCANappLauncherButton);
			SCANappLauncherButton = null;
			SCANUtil.SCANlog("App Launcher Button Removed");
		}

		private void toggleFlightOn()
		{
			if (SCANcontroller.controller != null)
			{
				SCANcontroller.controller.mainMap.Visible = true;
				SCANcontroller.controller.mainMapVisible = true;
			}
		}

		private void toggleFlightOff()
		{
			if (SCANcontroller.controller != null)
			{
				SCANcontroller.controller.mainMap.Visible = false;
				SCANcontroller.controller.mainMapVisible = false;
			}
		}

		private void toggleKSCOn()
		{
			if (SCANcontroller.controller != null)
			{
				SCANcontroller.controller.kscMap.Visible = true;
				SCANcontroller.controller.kscMapVisible = true;
			}
		}

		private void toggleKSCOff()
		{
			if (SCANcontroller.controller != null)
			{
				SCANcontroller.controller.kscMap.Visible = false;
				SCANcontroller.controller.kscMapVisible = false;
			}
		}
	}
}