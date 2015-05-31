#region license
/* 
 * [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 *
 * SCANuiColorPicker - object to handle color selection from an HSV color picker
 * 
 * Copyright (c)2014 David Grandy <david.grandy@gmail.com>;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
 */
#endregion

using UnityEngine;

namespace SCANsat.SCAN_UI.UI_Framework
{
	public class SCANuiColorPicker
	{
		private float valSlider, oldValSlider;
		private bool lowColorChange, oldColorState;
		private Color c = new Color();
		private Color colorLow = new Color();
		private Color colorHigh = new Color();
		private Texture2D minColorPreview = new Texture2D(1, 1);
		private Texture2D minColorOld = new Texture2D(1, 1);
		private Texture2D maxColorPreview = new Texture2D(1, 1);
		private Texture2D maxColorOld = new Texture2D(1, 1);

		public Color ColorLow
		{
			get { return colorLow; }
		}

		public Color ColorHigh
		{
			get { return colorHigh; }
		}

		public bool LowColorChange
		{
			get { return lowColorChange; }
		}

		internal SCANuiColorPicker (Color low, Color high, bool changeLow)
		{
			colorLow = c= low;
			c = colorLow.maxBright();
			colorHigh = high;
			lowColorChange = oldColorState = changeLow;
			valSlider = oldValSlider = colorLow.Brightness().Mathf_Round(2) * 100f;
		}

		internal void drawColorSelector(Rect R)
		{
			GUILayout.Space(20);
			GUILayout.BeginVertical();
				GUILayout.Label("Color Selection", SCANskins.SCAN_headline);
				GUILayout.BeginHorizontal();
					GUILayout.Space(30);
					GUILayout.Label(SCANskins.SCAN_BigColorWheel);
					Rect r = GUILayoutUtility.GetLastRect();
					GUILayout.EndHorizontal();
			GUILayout.EndVertical();

			Rect s = new Rect(r.x + 170, r.y + 100, 80, 30);
			GUI.Label(s, "Value: " + valSlider.ToString("N0") + "%", SCANskins.SCAN_whiteReadoutLabel);

			s.x += 80;
			s.y -= 90;
			s.width = 30;
			s.height = 200;
			valSlider = GUI.VerticalSlider(s, valSlider, 100, 0, SCANskins.SCAN_vertSlider, SCANskins.SCAN_sliderThumb).Mathf_Round(0);

			SCANuiUtil.drawVerticalSliderLabel(s, "0%", "100%");

			if (GUI.RepeatButton(r, "", SCANskins.SCAN_colorWheelButton))
			{
				int a = (int)Input.mousePosition.x;
				int b = Screen.height - (int)Input.mousePosition.y;

				c = SCANskins.SCAN_BigColorWheel.GetPixel(a - (int)R.x - (int)r.x, -(b - (int)R.y - (int)r.y));

				if (lowColorChange)
					colorLow = c * new Color(valSlider / 100f, valSlider / 100f, valSlider / 100f);
				else
					colorHigh = c * new Color(valSlider / 100f, valSlider / 100f, valSlider / 100f);
			}

			r.x -= 55;
			r.y += 145;
			r.width = 60;
			r.height = 30;

			colorSwatches(r, "Low", ref lowColorChange, true, minColorPreview, minColorOld, colorLow);

			r.x += 150;
			colorSwatches(r, "High", ref lowColorChange, false, maxColorPreview, maxColorOld, colorHigh);

			r.x -= 60;
			r.y += 30;
			GUI.Label(r, "New", SCANskins.SCAN_headlineSmall);

			r.y += 32;
			GUI.Label(r, "Old", SCANskins.SCAN_headlineSmall);
		}

		private void colorSwatches(Rect R, string Title, ref bool Active, bool Low, Texture2D Preview, Texture2D Current, Color New)
		{
			bool active;
			if (Low)
				active = Active;
			else
				active = !Active;

			active = GUI.Toggle(R, active, Title);

			if (Low)
				Active = active;
			else
				Active = !active;

			R.x += 10;
			R.y += 30;

			Preview.SetPixel(0, 0, New);
			Preview.Apply();
			GUI.DrawTexture(R, Preview);

			R.y += 32;
			GUI.DrawTexture(R, Current);
		}

		internal void brightnessChanged()
		{
			if (oldValSlider != valSlider)
			{
				oldValSlider = valSlider;
				if (lowColorChange)
					colorLow = c * new Color(valSlider / 100f, valSlider / 100f, valSlider / 100f);
				else
					colorHigh = c * new Color(valSlider / 100f, valSlider / 100f, valSlider / 100f);
			}
		}

		internal void colorStateChanged()
		{
			if (oldColorState != lowColorChange)
			{
				oldColorState = lowColorChange;
				if (lowColorChange)
				{
					c = colorLow.maxBright();
					valSlider = colorLow.Brightness().Mathf_Round(2) * 100f;
				}
				else
				{
					c = colorHigh.maxBright();
					valSlider = colorHigh.Brightness().Mathf_Round(2) * 100f;
				}
				oldValSlider = valSlider;
			}
		}

		internal void updateOldSwatches()
		{
			minColorOld.SetPixel(0, 0, colorLow);
			minColorOld.Apply();

			maxColorOld.SetPixel(0, 0, colorHigh);
			maxColorOld.Apply();
		}
	}
}
