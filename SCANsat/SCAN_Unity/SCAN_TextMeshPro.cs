#region license
/*The MIT License (MIT)
CWTextMeshProHolder - An extension of TextMeshProUGUI for updating certain elements of the text

Copyright (c) 2016 DMagic

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
#endregion

using SCANsat.Unity;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace SCANsat.SCAN_Unity
{
	public class SCAN_TextMeshPro : TextMeshProUGUI
	{
		private TextHandler _handler;

		new private void Awake()
		{
			base.Awake();

			_handler = GetComponent<TextHandler>();

			if (_handler == null)
				return;

			_handler.SetNormalColor(color);

			_handler.OnColorUpdate.AddListener(new UnityAction<Color>(UpdateColor));
			_handler.OnTextUpdate.AddListener(new UnityAction<string>(UpdateText));
			_handler.OnFontChange.AddListener(new UnityAction<int>(UpdateFontSize));
		}

		public void Setup(TextHandler h)
		{
			_handler = h;

			_handler.OnColorUpdate.AddListener(new UnityAction<Color>(UpdateColor));
			_handler.OnTextUpdate.AddListener(new UnityAction<string>(UpdateText));
			_handler.OnFontChange.AddListener(new UnityAction<int>(UpdateFontSize));
		}

		private void UpdateColor(Color c)
		{
			color = c;
		}

		private void UpdateText(string t)
		{
			text = t;

			_handler.PreferredSize = new Vector2(preferredWidth, preferredHeight);
		}

		private void UpdateFontSize(int i)
		{
			fontSize += i;
		}
	}
}
