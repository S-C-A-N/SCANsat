using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SCANsat.Platform;
using SCANsat.SCAN_UI;

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

			SCANappLauncherButton = ApplicationLauncher.Instance.AddModApplication(toggleOn, toggleOff, null, null, null, null, (ApplicationLauncher.AppScenes)63, SCANskins.SCAN_SmallMapIcon);

			GameEvents.onGUIApplicationLauncherUnreadifying.Add(removeButton);
		}

		private void removeButton(GameScenes scene)
		{
			ApplicationLauncher.Instance.RemoveModApplication(SCANappLauncherButton);
			SCANappLauncherButton = null;
			SCANUtil.SCANlog("App Launcher Button Removed");
		}

		private void toggleOn()
		{
			if (SCANcontroller.controller != null)
			{
				SCANcontroller.controller.mainMap.Visible = true;
				SCANcontroller.controller.mainMapVisible = true;
			}
		}

		private void toggleOff()
		{
			if (SCANcontroller.controller != null)
			{
				SCANcontroller.controller.mainMap.Visible = false;
				SCANcontroller.controller.mainMapVisible = false;
			}
		}
	}
}
