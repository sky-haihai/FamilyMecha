using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XiheFramework;

public class LeftArmUI : UIBehaviour {
    public Slider chargeBar;
    public Image crosshair;

    public override void Start() {
        base.Start();
    }

    private void LateUpdate() {
        var point = Game.Blackboard.GetData<Vector3>("LeftArm.AimHitWorldPos");
        if (Vector3.Distance(Vector3.zero, point) <= float.Epsilon) {
            crosshair.rectTransform.anchoredPosition = new Vector2(960f, 540f);
        }
        else {
            crosshair.rectTransform.anchoredPosition = Camera.main.WorldToScreenPoint(point);
        }
    }
}