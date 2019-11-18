using UnityEngine;
using System;
using UnityEngine.Events;

namespace SCANsat.Unity.HSVPicker
{
	[Serializable]
	public class ColorChangedEvent : UnityEvent<Color>
	{
		
	}
}
