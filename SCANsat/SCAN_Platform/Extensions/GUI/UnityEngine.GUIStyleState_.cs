using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Text;

using Log = SCANsat.SCAN_Platform.Logging.ConsoleLogger;

namespace UnityEngine
{
	public static class GUIStyleState_ {
		internal static Color white = Color.white;
		internal static Color black = Color.black;
		internal static Color clear = Color.clear;

		// background
		// textColor

		public static string backgroundPP(this GUIStyleState s, params object[] strs) {
			return "";
		}
		public static string PP(this GUIStyleState s, params object[] strs) {
			string bkg = "null";
			string colorName = "";
			
			if (s.textColor == black && s.background == null) { return ""; }
			if (s.background != null) bkg = s.background.ToString ();
			
			if (Color_.knownColors.TryGetValue (s.textColor, out colorName)) {
				return "{ GUIss: textColor: " + colorName + " bkg: " + bkg + " }";
			} else {
				return "{ GUIss: textColor: " + s.textColor.ToString () + " bkg: " + bkg + " }";
			}
		}
		
		public static void PPP(this GUIStyleState s, string breadcrumbs) {
			string bkg = "null";
			string colorName = "";
			
			if (s.textColor == black && s.background == null) { return; }
			if (s.background != null) bkg = s.background.ToString ();
			
			if (Color_.knownColors.TryGetValue (s.textColor, out colorName)) {
				Log.Debug("{0} {{ GUIss: textColor: {1} bkg: {2} }}", breadcrumbs, colorName, bkg);
			} else {
				Log.Debug("{0} {{ GUIss: textColor: {1} bkg: {2} }}", breadcrumbs, s.textColor.ToString(), bkg);
			}
		}
	}
}

