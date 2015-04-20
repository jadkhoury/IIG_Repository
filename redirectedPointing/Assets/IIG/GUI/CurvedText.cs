using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CurvedText : Text {

	protected override void OnFillVBO(List<UIVertex> vbo) {
		base.OnFillVBO(vbo);
		for (int i = 0; i < vbo.Count; i++) {
			UIVertex v = vbo[i];
			Vector3 p = v.position;
			
			// move your verts around here
			
			v.position = p;
		}
	}
}