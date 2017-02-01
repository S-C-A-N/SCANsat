#region license
/*The MIT License (MIT)
TextHandler - Script for handling Text object replacement with Text Mesh Pro

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

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace SCANsat.Unity
{
	public class TextHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IScrollHandler
	{
		[SerializeField]
		private bool m_Outline = false;
		[SerializeField]
		private float m_OutlineWidth = 0;
		[SerializeField]
		private bool m_Highlighter = false;
		[SerializeField]
		private Color m_HighlightColor = Color.white;

		public class OnTextEvent : UnityEvent<string> { }

		public class OnColorEvent : UnityEvent<Color> { }

		public class OnFontEvent : UnityEvent<int> { }

		private OnTextEvent _onTextUpdate = new OnTextEvent();
		private OnColorEvent _onColorUpdate = new OnColorEvent();
		private OnFontEvent _onFontChange = new OnFontEvent();

		private Vector2 _preferredSize = new Vector2();
		private Color _normalColor = Color.white;
		private ScrollRect scroller;

		public void SetNormalColor(Color c)
		{
			_normalColor = c;
		}

		public void SetScroller(ScrollRect s)
		{
			scroller = s;
		}

		public bool Outline
		{
			get { return m_Outline; }
		}

		public float OutlineWidth
		{
			get { return m_OutlineWidth; }
		}

		public Vector2 PreferredSize
		{
			get { return _preferredSize; }
			set { _preferredSize = value; }
		}

		public UnityEvent<string> OnTextUpdate
		{
			get { return _onTextUpdate; }
		}

		public UnityEvent<Color> OnColorUpdate
		{
			get { return _onColorUpdate; }
		}

		public UnityEvent<int> OnFontChange
		{
			get { return _onFontChange; }
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (!m_Highlighter)
				return;

			OnColorUpdate.Invoke(m_HighlightColor);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (!m_Highlighter)
				return;

			OnColorUpdate.Invoke(_normalColor);
		}

		public void OnScroll(PointerEventData eventData)
		{
			if (scroller == null)
				return;

			scroller.OnScroll(eventData);
		}
	}
}
