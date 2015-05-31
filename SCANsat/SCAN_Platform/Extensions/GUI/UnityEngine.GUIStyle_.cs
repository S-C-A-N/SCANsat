using System;
using System.Collections.Generic;

using UnityEngine;
using Log = SCANsat.SCAN_Platform.Logging.ConsoleLogger;


namespace UnityEngine {
	public static class GUIStyle_ {
		internal static List<GUIStyle> knownStyles { get; set; }
		
		public static GUIStyle dumpStyle(this GUIStyle s, string breadcrumbs) {
			var hash = s.GetHashCode ();

			if (knownStyles.Contains(s)) { Log.Debug("{0} => #[{2:X8}] {1}  skipped", breadcrumbs, s.ToString(), hash); return s; }



			//Log.Debug("{0} => #[{2:X8}] {1}", breadcrumbs, s.ToString(), s.GetHashCode());
			//Log.Debug("             ------------------------------------------------------------------------------------------------ ");
			//Log.Debug("              -> contentOffset => {0}", s.contentOffset);
			//Log.Debug("             ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ ");
			//Log.Debug("              -> border    => {0}", s.border);
			//Log.Debug("              -> padding   => {0}", s.border);
			//Log.Debug("              -> margin    => {0}", s.border);
			//Log.Debug("              -> overflow  => {0}", s.border);
			//// print all of the non-trivialGUIStyleState components
			//Log.Debug("             ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ ");
			//s.active.PPP	("              -> active    => ");
			//s.onActive.PPP	("              -> onActive  => ");
			//s.normal.PPP	("              -> normal    => ");
			//s.onNormal.PPP	("              -> onNormal  => ");
			//s.hover.PPP		("              -> hover     => ");
			//s.onHover.PPP	("              -> onHover   => ");
			//s.focused.PPP	("              -> focused   => ");
			//s.onFocused.PPP	("              -> onFocused => ");
			//Log.Debug("             ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ ");
			//Log.Debug("              -> alignment => {0}", s.alignment);
			//Log.Debug("              -> wordwrap  => {0}", s.alignment);
			//Log.Debug("              -> clipping  => {0}", s.clipping);
			//Log.Debug("              -> richText? => {0}", s.richText);
			//Log.Debug("              -> lineHeight=> {0}", s.lineHeight);
			//Log.Debug("              -> font      => {0}", s.fontPP());
			//Log.Debug("              -> fontSize  => {0}", s.fontSize);
			//Log.Debug("              -> fontStyle => {0}", s.fontStyle);
			//Log.Debug("             ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ ");
			//Log.Debug("              -> fixedW, stretchW?  => {0} ({1})", s.fixedWidth, s.stretchWidth);
			//Log.Debug("              -> fixedH, stretchH?  => {0} ({1})", s.fixedHeight, s.stretchHeight);
			//Log.Debug("              -> isHdepW? => {0}", s.isHeightDependantOnWidth);
			//Log.Debug("             ------------------------------------------------------------------------------------------------ ");
			
			return s;
		}
		
		
		public static string fontPP(this GUIStyle s) {
			string font = "null";
			if (s.font != null) font = s.font.ToString ();
			return font;
		}
		
		public static GUIStyle PaddingChange(this GUIStyle g, Int32 PaddingValue)
		{
			GUIStyle gReturn = new GUIStyle(g);
			gReturn.padding = new RectOffset(PaddingValue, PaddingValue, PaddingValue, PaddingValue);
			return gReturn;
		}
		public static GUIStyle PaddingChangeBottom(this GUIStyle g, Int32 PaddingValue)
		{
			GUIStyle gReturn = new GUIStyle(g);
			gReturn.padding.bottom = PaddingValue;
			return gReturn;
		}
	}
}

