#region license
/*
 * [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 * 
 * SCANpalette - manages colors and palettes of colors
 *
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
*/
#endregion

using System;
using UnityEngine;
using SCANsat.SCAN_Data;
using SCANsat.SCAN_Platform.Palettes;
using SCANsat.SCAN_Platform.Palettes.ColorBrewer;

namespace SCANsat.SCAN_UI.UI_Framework
{
	public class SCANpalette
	{

		// Basic Colors
		// 	(these are here just so that all references to some color are the same throughout
		//		SCANsat)
		public static Color black 	= Color.black;
		public static Color white 	= Color.white;
		public static Color red		= Color.red;
		public static Color grey		= Color.grey;
		public static Color clear	= Color.clear;
		public static Color magenta	= Color.magenta;
		public static Color yellow	= Color.yellow;
		public static Color cyan		= Color.cyan;
		public static Color blue		= Color.blue;
		public static Color green	= Color.green;
		public static Color mechjebYellow = new Color(1.0f, 0.56f, 0.0f);

		public static Color lerp (Color a, Color b, float t) {
			return Color.Lerp (a,b,t);
		}

		// XKCD Colors
		// 	(these are collected here for the same reason)

		public static Color xkcd_Amber		= XKCDColors.Amber;
		public static Color xkcd_ArmyGreen		= XKCDColors.ArmyGreen;
		public static Color xkcd_PukeGreen		= XKCDColors.PukeGreen;
		public static Color xkcd_Lemon		= XKCDColors.Lemon;
		public static Color xkcd_OrangeRed 	= XKCDColors.OrangeRed;
		public static Color xkcd_CamoGreen		= XKCDColors.CamoGreen;
		public static Color xkcd_Marigold		= XKCDColors.Marigold;
		public static Color xkcd_Puce			= XKCDColors.Puce;
		public static Color xkcd_DarkTeal 		= XKCDColors.DarkTeal;
		public static Color xkcd_DarkPurple	= XKCDColors.DarkPurple;
		public static Color xkcd_DarkGrey		= XKCDColors.DarkGrey;
		public static Color xkcd_LightGrey		= XKCDColors.LightGrey;
		public static Color xkcd_PurplyPink	= XKCDColors.PurplyPink;
		public static Color xkcd_Magenta		= XKCDColors.Magenta;
		public static Color xkcd_YellowGreen	= XKCDColors.YellowGreen;
		public static Color xkcd_LightRed		= XKCDColors.LightRed;
		public static Color xkcd_Cerulean		= XKCDColors.Cerulean;
		public static Color xkcd_Yellow		= XKCDColors.Yellow;
		public static Color xkcd_Red			= XKCDColors.Red;
		public static Color xkcd_White		= XKCDColors.White;


		// colourblind barrier-free colours, according to Masataka Okabe and Kei Ito
		// http://jfly.iam.u-tokyo.ac.jp/color/

		public static Color cb_skyBlue 		= new Color(0.35f, 0.7f, 0.9f);		// sky blue
		public static Color cb_bluishGreen 	= new Color(0f, 0.6f, 0.5f);		// bluish green
		public static Color cb_yellow 		= new Color(0.95f, 0.9f, 0.25f);	// yellow
		public static Color cb_blue 			= new Color(0f, 0.45f, 0.7f);		// blue
		public static Color cb_vermillion 		= new Color(0.8f, 0.4f, 0f);		// vermillion
		public static Color cb_reddishPurple	= new Color(0.8f, 0.6f, 0.7f);		// reddish purple
		public static Color cb_orange 		= new Color(0.9f, 0.6f, 0f);		// orange

		/* SOLARIZED colors: currently unused, so commented out */
		/*
		public static Color sol_base03 	= new Color32(0,43,54,255);
		public static Color sol_base02 	= new Color32(7,54,66,255);
		public static Color sol_base01 	= new Color32(88,110,117,255);
		public static Color sol_base00 	= new Color32(101,123,131,255);
		public static Color sol_base0 	= new Color32(131,148,150,255);
		public static Color sol_base1 	= new Color32(147,161,161,255);
		public static Color sol_base2 	= new Color32(238,232,213,255);
		public static Color sol_base3 	= new Color32(253,246,227,255);
		public static Color sol_yellow 	= new Color32(181,137,0,255);
		public static Color sol_orange 	= new Color32(203,75,22,255);
		public static Color sol_red 		= new Color32(45,220,50,255);
		public static Color sol_magenta 	= new Color32(211,54,130,255);
		public static Color sol_violet 	= new Color32(108,113,196,255);
		public static Color sol_blue 		= new Color32(38,139,210,255);
		public static Color sol_cyan 		= new Color32(42,161,152,255);
		public static Color sol_green 	= new Color32(133,153,0,255);
		*/
		public static Color[] heightGradient = {
			xkcd_ArmyGreen,
			xkcd_Yellow,
			xkcd_Red,
			xkcd_Magenta,
			xkcd_White,
			xkcd_White
		};

		public static Color[] redline;
        	public static Color gridFull; // resource colors
        	public static Color gridEmpty; //empty resource color

		public static Color[] small_redline;

		public static Color heightToColor(float val, int scheme, SCANdata data)
		{
			Color32[] c = data.TerrainConfig.ColorPal.colors;
			if (data.TerrainConfig.PalRev)
				c = data.TerrainConfig.ColorPal.colorsReverse;
			if (scheme == 0)
				return heightToColor(val, data.TerrainConfig.MaxTerrain, data.TerrainConfig.MinTerrain, data.TerrainConfig.ClampTerrain, data.TerrainConfig.PalDis, c);
			else
				return heightToColor(val, data.TerrainConfig.MaxTerrain, data.TerrainConfig.MinTerrain, data.TerrainConfig.PalDis);
		}

		private static Color heightToColor(float val, float max, float min, bool discrete)
		{
			Color c = black;
			float range = max - min;
			val -= min;
			if (discrete)
			{
				val = (greyScalePalette.colorsReverse.Length) * Mathf.Clamp(val, 0, range) / range;
				if (Math.Floor(val) > greyScalePalette.colorsReverse.Length - 1)
					val = greyScalePalette.colorsReverse.Length - 0.01f;
				c = greyScalePalette.colorsReverse[(int)Math.Floor(val)];
			}
			else
			{
				val = (greyScalePalette.colorsReverse.Length - 1) * Mathf.Clamp(val, 0, range) / range;
				if (Math.Floor(val) > greyScalePalette.colorsReverse.Length - 2)
					val = greyScalePalette.colorsReverse.Length - 1.01f;
				c = lerp(greyScalePalette.colorsReverse[(int)Math.Floor(val)], greyScalePalette.colorsReverse[(int)Math.Floor(val) + 1], val - (int)Math.Floor(val));
			}
			return c;
		}

		internal static Color heightToColor(float val, float max, float min, float? clamp, bool discrete, Color32[] p)
		{
			Color c = black;
			if (clamp != null)
			{
				if (clamp < min + 10f)
					clamp = min + 10f;
				if (clamp > max - 10f)
					clamp = max - 10f;
				if (val <= (float)clamp)
				{
					val -= min;
					val = Mathf.Clamp(val, 0, (float)clamp - min) / ((float)clamp - min);
					if (discrete)
						c = p[(int)Math.Round(val)];
					else
						c = lerp(p[0], p[1], val);
				}
				else
				{
					float newRange = max - (float)clamp;
					val -= (float)clamp;
					if (discrete)
					{
						val = (p.Length - 2) * Mathf.Clamp(val, 0, newRange) / newRange;
						if (Math.Floor(val) > p.Length - 3)
							val = p.Length - 0.01f;
						c = p[(int)Math.Floor(val) + 2];
					}
					else
					{
						val = (p.Length - 3) * Mathf.Clamp(val, 0, newRange) / newRange;
						if (Math.Floor(val) > p.Length - 4)
							val = p.Length - 3.01f;
						c = lerp(p[(int)Math.Floor(val) + 2], p[(int)Math.Floor(val) + 3], val - (int)Math.Floor(val));
					}
				}
			}
			else
			{
				float range = max - min;
				val -= min;
				if (discrete)
				{
					val = (p.Length) * Mathf.Clamp(val, 0, range) / range;
					if (Math.Floor(val) > p.Length - 1)
						val = p.Length - 0.01f;
					c = p[(int)Math.Floor(val)];
				}
				else
				{
					val = (p.Length - 1) * Mathf.Clamp(val, 0, range) / range;
					if (Math.Floor(val) > p.Length - 2)
						val = p.Length - 1.01f;
					c = lerp(p[(int)Math.Floor(val)], p[(int)Math.Floor(val) + 1], val - (int)Math.Floor(val));
				}
			}
			return c;
		}

			//int sealevel = 0;
			//if (val <= sealevel) {
			//	val = (Mathf.Clamp (val , -1500 , sealevel) + 1500) / 1000f;
			//	c = lerp (xkcd_DarkPurple , xkcd_Cerulean , val);
			//} else {
			//	val = (heightGradient.Length - 2) * Mathf.Clamp (val , sealevel , (sealevel + 7500)) / (sealevel + 7500.0f);
			//	c = lerp (heightGradient [(int)val] , heightGradient [(int)val + 1] , val - (int)val);
			//}
		//	return c;
		//}

		public static string colorHex ( Color32 c ) {
			return "#" + c.r.ToString ("x2") + c.g.ToString ("x2") + c.b.ToString ("x2");
		}
		public static string colored ( Color c , string text ) {
			return "<color=\"" + colorHex (c) + "\">" + text + "</color>";
		}

		internal static Color c_good {
			get {
				if (SCANcontroller.controller.colours != 1) 	return cb_bluishGreen;
				else 								return cb_skyBlue;
			}
		}
		internal static Color c_bad
		{
			get
			{
				return cb_orange;
			}
		}
		internal static Color c_ugly {
			get {
				if (SCANcontroller.controller.colours != 1)	return xkcd_LightRed;
				else 								return cb_yellow;
			}
		}

		public static Color picker(Rect r, Color c) {
			GUILayout.BeginArea (r,"","Box");
			GUILayout.BeginHorizontal ();
			// R
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal ();
			// G
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal ();
			// B
			GUILayout.EndHorizontal();

			GUILayout.EndArea ();
			return c;
		}

		public static void swatch(Color c) {
			Texture2D tex = new Texture2D (20,20);
			GUILayout.BeginVertical ("Box",new GUILayoutOption[]{GUILayout.Width (22),GUILayout.Height (22)});
			GUI.color = c;
			GUILayout.Label (tex);
			GUI.color = Color.white;
			GUILayout.EndVertical ();
		}

		private static _Palettes currentPaletteSet;
		//private static _Palettes divPaletteSet;
		//private static _Palettes qualPaletteSet;
		//private static _Palettes seqPaletteSet;
		//private static _Palettes fixedPaletteSet;
		//private static Palette currentHeightPalette;
		private static Palette greyScalePalette = BrewerPalettes.Greys(9);

		private static _Palettes generatePaletteSet(int size, Palette.Kind type)
		{
			PaletteLoader.generatePalettes(type, size);
			return new _Palettes(PaletteLoader.palettes.ToArray(), type, size);
		}

		internal static _Palettes setCurrentPalettesType(Palette.Kind type, int size)
		{
			switch (type)
			{
				case Palette.Kind.Fixed:
					return generatePaletteSet(0, Palette.Kind.Fixed);
				default:
					return generatePaletteSet(size, type);
			}
			//switch (type)
			//{
			//	case Palette.Kind.Diverging:
			//		return generatePaletteSet(size, type);
			//		//return divPaletteSet; 
			//	case Palette.Kind.Qualitative:
			//		//return qualPaletteSet;
			//	case Palette.Kind.Sequential:
			//		//return seqPaletteSet;
			//	case Palette.Kind.Fixed:
			//		//return fixedPaletteSet;
			//	default:
			//		//return divPaletteSet;
			//}
		}

		public static Palette GreyScalePalette
		{
			get { return greyScalePalette; }
		}

		public static _Palettes CurrentPalettes
		{
			get { return currentPaletteSet; }
			internal set
			{
				currentPaletteSet = value;
			}
		}

		public static string getPaletteTypeName
		{
			get { return currentPaletteSet.paletteType.ToString(); }
		}
		
		//public static _Palettes DivPaletteSet
		//{
		//	get { return divPaletteSet; }
		//	internal set { divPaletteSet = value; }
		//}

		//public static _Palettes QualPaletteSet
		//{
		//	get { return qualPaletteSet; }
		//	internal set { qualPaletteSet = value; }
		//}

		//public static _Palettes SeqPaletteSet
		//{
		//	get { return seqPaletteSet; }
		//	internal set { seqPaletteSet = value; }
		//}

		//public static _Palettes FixedPaletteSet
		//{
		//	get { return fixedPaletteSet; }
		//	internal set { fixedPaletteSet = value; }
		//}

		//public static Palette CurrentPalette
		//{
		//	get { return currentHeightPalette; }
		//	internal set { currentHeightPalette = value; }
		//}

	}
}

