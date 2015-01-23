#region license
/* 
 *  [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 *
 * SCANsat - Skins Library; For adding, copying and modifying GUI Skins and Styles
 *  * 
 * A modified form of TriggerAu's Skins Library class:
 * http://forum.kerbalspaceprogram.com/threads/66503-KSP-Plugin-Framework
 * 
 * Copyright (c)2013 damny;
 * Copyright (c)2014 David Grandy <david.grandy@gmail.com>;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
 *
 */
#endregion
using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Log = SCANsat.SCAN_Platform.Logging.ConsoleLogger;

namespace SCANsat.SCAN_Platform
{

	public static class SCAN_SkinsLibrary
	{
		#region Constructor
		static SCAN_SkinsLibrary()
		{
			_Initialized = false;
			knownSkins = new Dictionary<string, GUISkin>();
			GUIStyle_.knownStyles = new List<GUIStyle>();
			Color_.initColorTable();	// initalize the table of known colors
		}
		#endregion
		private static GUISkin _CurrentSkin;
		private static GUIStyle _CurrentTooltip;
		internal static bool _Initialized { get; private set; }
		public static GUISkin DefUnitySkin { get; private set; }
		public static GUISkin DefKSPSkin { get; private set; }

		internal static GUISkin CurrentSkin { get { return _CurrentSkin; } }
		internal static GUIStyle CurrentTooltip { get { return _CurrentTooltip; } }

		internal static Dictionary<string, GUISkin> knownSkins { get; set; }

		public delegate void SkinChangedEvent();
		public static event SkinChangedEvent OnSkinChanged;

		public delegate void TooltipChangedEvent();
		public static event TooltipChangedEvent OnTooltipChanged;	//FIXME: unused

		internal static void InitSkinList()
		{
			Log.Debug("in InitSkinList()");
			if (!_Initialized)
			{
				DefUnitySkin = GUI.skin;
				DefKSPSkin = HighLogic.Skin;

				knownSkins.Add("Unity", DefUnitySkin);
				knownSkins.Add("KSP", DefKSPSkin);
				DefUnitySkin.dumpSkins();
				SetCurrent("KSP");
				_Initialized = true;
			}
		}

		public static void SetCurrent(string SkinID)
		{
			GUISkin OldSkin = _CurrentSkin;

			Log.Debug("Setting GUISkin(string SkinID) to {0}", SkinID);

			if (knownSkins.ContainsKey(SkinID)) _CurrentSkin = knownSkins[SkinID];
			else Log.Now("SetCurrent: GUISkin {0} not found", SkinID);

			//SetCurrentTooltip(); // Now set the tooltip style as well
			if (OldSkin != CurrentSkin && OnSkinChanged != null) OnSkinChanged();
		}

		public static void SetCurrentTooltip()
		{
			//Use the custom skin if it exists
			Log.Debug("in SetCurrentTooltip()");
			if (StyleExists(_CurrentSkin, "SCAN_Tooltip")) _CurrentTooltip = GetStyle(_CurrentSkin, "SCAN_Tooltip");
			else
			{  //otherwise lets build a style for the defaults or take the label style otherwise
				if (_CurrentSkin == DefUnitySkin) _CurrentTooltip = new GUIStyle(DefUnitySkin.box);
				else if (_CurrentSkin == DefKSPSkin) _CurrentTooltip = GenDefKSPTooltip();
				else _CurrentTooltip = _CurrentSkin.label;
			}
		}
		public static GUIStyle GenDefKSPTooltip()
		{
			Log.Debug("in GenDefKSPTooltip()");
			GUIStyle retStyle = new GUIStyle(DefKSPSkin.label);						// build a new style to return
			Texture2D texBack = new Texture2D(1, 1, TextureFormat.ARGB32, false);	// background texture
			texBack.SetPixel(0, 0, new Color(0.5f, 0.5f, 0.5f, 0.95f));				// background color
			texBack.Apply();
			retStyle.normal.background = texBack;									// put bkg into style
			retStyle.normal.textColor = new Color32(224, 224, 224, 255);			// set some text defaults
			retStyle.padding = new RectOffset(3, 3, 3, 3);							// set padding defaults
			retStyle.alignment = TextAnchor.MiddleCenter;							// set default center alignment
			return retStyle;
		}

		public static GUISkin CopySkin(string SkinID)
		{
			Log.Debug("in CopySkin(string SkinID)");
			if (knownSkins.ContainsKey(SkinID)) return (GUISkin)SCAN_MBE.Instantiate(knownSkins[SkinID]);
			else
			{
				Log.Now("CopySkin(): GUISkin {0} not found", SkinID);
				throw new SystemException(string.Format("CopySkin(): GUISkin {0} not found", SkinID));
			}
		}

		public static void AddSkin(string SkinID, GUISkin NewSkin, bool SetAsCurrent = false)
		{
			Log.Debug("in AddSkin");
			NewSkin.name = SkinID;
			if (knownSkins.ContainsKey(SkinID)) knownSkins[SkinID] = NewSkin;
			else knownSkins.Add(SkinID, NewSkin);

			if (SetAsCurrent) SetCurrent(SkinID);
		}
		public static bool RemoveSkin(string SkinID)
		{
			switch (SkinID)
			{
				case "Unity":
				case "KSP": Log.Now("RemoveSkin({0}) failed: removing a built-in skin is prohibited.", SkinID); return false;
				default: Log.Now("RemoveSkin({0})", SkinID); return knownSkins.Remove(SkinID);
			}
		}
		internal static bool SkinExists(string SkinID) { Log.Debug("in SkinExists()"); return knownSkins.ContainsKey(SkinID); }

		internal static bool StyleExists(string SkinID, string StyleID)
		{
			return (SkinExists(SkinID) && StyleExists(knownSkins[SkinID], StyleID));
		}
		internal static bool StyleExists(GUISkin SkinToAction, string StyleID)
		{
			Log.Debug("in StyleExists(GUISkin s2a,)");
			if (SkinToAction.customStyles.Any(x => x.name == StyleID))
				return true;
			else
			{
				//Log.log("Unable to find Style: {0} in Skin: {1}", StyleID, SkinToAction.name);
				return false;
			}
			//return (SkinToAction.customStyles.Any(x => x.name == StyleID));
		}

		public static void AddStyle(GUIStyle NewStyle, string SkinId)
		{
			Log.Debug("in AddStyle(GUIStyle ns,string sid)");
			if (SkinExists(SkinId))
			{
				GUISkin skinTemp = knownSkins[SkinId];
				AddStyle(NewStyle, ref skinTemp);
			}
		}
		internal static void AddStyle(GUIStyle NewStyle, string SkinId, string StyleID)
		{
			Log.Debug("in AddStyle(GUIStyle ns,string skinID, string StyleID)");
			NewStyle.name = StyleID;
			AddStyle(NewStyle, SkinId);
		}
		internal static void AddStyle(GUIStyle NewStyle, ref GUISkin SkinToAction, string StyleID)
		{
			Log.Debug("in AddStyle(GUIStyle ns,ref GUISkin, string StyleID)");
			NewStyle.name = StyleID;				// set the name
			AddStyle(NewStyle, ref SkinToAction); 	// and push to the next method
		}
		internal static void AddStyle(GUIStyle NewStyle, ref GUISkin SkinToAction)
		{
			Log.Debug("in AddStyle(GUIStyle ns,ref GUISkin)");
			if (string.IsNullOrEmpty(NewStyle.name))
			{
				Log.Now("No Name Provided in the Style to add to {0}. Cannot add this.", SkinToAction.name);
				return;
			}
			List<GUIStyle> lstTemp = SkinToAction.customStyles.ToList<GUIStyle>(); // convert to a list


			if (lstTemp.Any(x => x.name == NewStyle.name))
			{			// add or edit the customstyle
				GUIStyle styleTemp = lstTemp.First(x => x.name == NewStyle.name);
				lstTemp.Remove(styleTemp); 								// if itexists then remove it first
			}

			lstTemp.Add(NewStyle); 										// add the new style
			SkinToAction.customStyles = lstTemp.ToArray<GUIStyle>();	// write the list back to the array
		}

		internal static void RemoveStyle(string SkinID, string StyleID)
		{
			Log.Debug("in RemoveStyle(string SkinID,string StyleID)");
			if (SkinExists(SkinID))
			{
				GUISkin skinTemp = knownSkins[SkinID];
				RemoveStyle(ref skinTemp, StyleID);
			}
		}
		internal static void RemoveStyle(ref GUISkin SkinToAction, string StyleID)
		{
			Log.Debug("in RemoveStyle(GUISkin s2a, string StyleID)");
			if (StyleExists(SkinToAction, StyleID))
			{
				List<GUIStyle> lstTemp = SkinToAction.customStyles.ToList<GUIStyle>();	// convert to a list
				GUIStyle styleTemp = lstTemp.First(x => x.name == StyleID); 			// find and ...
				lstTemp.Remove(styleTemp);												// ... remove the style
				SkinToAction.customStyles = lstTemp.ToArray<GUIStyle>(); 				// write back
			}
		}

		internal static GUIStyle GetStyle(GUISkin SkinToAction, string StyleID)
		{
			Log.Debug("in GetStyle(GUISkin s2a, string StyleID)");
			if (StyleExists(SkinToAction, StyleID))
				return SkinToAction.customStyles.First(x => x.name == StyleID);
			else
				return null;
		}
	}
}
