#region license
/* 
 * [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 *
 * SCANuiSlider - object to handle drawing and labeling GUI sliders
 * 
 * Copyright (c)2014 David Grandy <david.grandy@gmail.com>;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
 */
#endregion

using UnityEngine;

namespace SCANsat.SCAN_UI.UI_Framework
{
	public class SCANuiSlider
	{
		private float minValue, maxValue, currentValue;
		private float oldValue;
		private float width;
		private float extraLabelWidth;
		private string title, units, tooltip;
		private int precision;

		public float MinValue
		{
			get { return minValue; }
			internal set
			{
				if (value < maxValue)
					minValue = value;
			}
		}

		public float MaxValue
		{
			get { return maxValue; }
			internal set
			{
				if (value > minValue)
					maxValue = value;
			}
		}

		//public float CurrentValue
		//{
		//	get { return currentValue; }
		//	internal set { currentValue = value; }
		//}

		public SCANuiSlider(float Min, float Max, float Value, string Title, string Units, string Tooltip, int Prec, float w = 260, float extraLabel = 0)
		{
			minValue = Min;
			maxValue = Max;
			currentValue = Value;
			oldValue = Value;
			title = Title;
			units = Units;
			tooltip = Tooltip;
			precision = Prec;
			width = w;
		}

		public float drawSlider(bool under, ref float value)
		{
			string s = title + value + units;
			GUILayout.Label(under ? new GUIContent(s) : new GUIContent(s, tooltip), SCANskins.SCAN_whiteReadoutLabel);

			Rect r = GUILayoutUtility.GetLastRect();
			r.x += 110 + extraLabelWidth;
			r.width = width;

			if (under)
				GUI.Label(r, "", SCANskins.SCAN_horSlider);
			else
				value = GUI.HorizontalSlider(r, value, minValue, maxValue).Mathf_Round(precision);

			int i = precision <= 0 ? 0 : precision;
			string labelPrecision = "F" + i.ToString();
			SCANuiUtil.drawSliderLabel(r, minValue.ToString(labelPrecision) + units, maxValue.ToString(labelPrecision) + units);

			currentValue = value;

			return value;
		}

		public bool valueChanged()
		{
			if (oldValue != currentValue)
			{
				oldValue = currentValue;
				return true;
			}

			return false;
		}
	}
}
