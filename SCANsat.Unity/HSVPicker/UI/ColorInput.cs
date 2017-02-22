using UnityEngine;
using UnityEngine.UI;
using System;
using SCANsat.Unity.HSVPicker.Enum;

namespace SCANsat.Unity.HSVPicker.UI
{
	/// <summary>
	/// Displays one of the color values of aColorPicker
	/// </summary>
	[RequireComponent(typeof(InputField))]
	public class ColorInput : MonoBehaviour
	{
		public ColorPicker hsvpicker;

		/// <summary>
		/// Which value this slider can edit.
		/// </summary>
		public ColorValues type;

		private InputField inputField;

		private void Awake()
		{
			inputField = GetComponent<InputField>();

			inputField.onValueChanged.AddListener(InputChanged);
		}

		private void OnDestroy()
		{
			inputField.onValueChanged.RemoveListener(InputChanged);
		}

		private void InputChanged(string input)
		{
			if (string.IsNullOrEmpty(input))
				return;

			float original = 0;

			switch(type)
			{
				case ColorValues.R:
					original = hsvpicker.R;
					break;
				case ColorValues.G:
					original = hsvpicker.G;
					break;
				case ColorValues.B:
					original = hsvpicker.B;
					break;
			}

			float f = original;

			if (!float.TryParse(input, out f))
				return;

			if (f < 0)
				return;
			else if (input.StartsWith("1."))
				f = 1;
			else if (f >= 1 && f <= 255)
			{
				f = Mathf.RoundToInt(f);

				f /= 255;
			}
			else if (f > 255)
				return;

			hsvpicker.AssignColor(type, f);
		}
	}
}
