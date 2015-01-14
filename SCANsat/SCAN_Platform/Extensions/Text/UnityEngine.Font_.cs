using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace UnityEngine
{
	public static class Font_ // : Object
	{
//		var characterInfo			Access an array of all characters contained in the font texture.
//		var dynamic					Is the font a dynamic font.
//		var material				The material used for the font display.
//		var textureRebuildCallback	Set a function to be called when the dynamic font texture is rebuilt.
//		
//		GetCharacterInfo			Get rendering info for a specific character.
//		HasCharacter				Does this font have a specific character?
//		RequestCharactersInTexture	Request characters to be added to the font texture (dynamic fonts only).
//		
//		GetMaxVertsForString		Returns the maximum number of verts that the text generator may return for a given string.

		public static Font[] knownFonts (this Font f) {
			return (Font[])UnityEngine.Resources.FindObjectsOfTypeAll( typeof(UnityEngine.Font) );
		}
	}
}

