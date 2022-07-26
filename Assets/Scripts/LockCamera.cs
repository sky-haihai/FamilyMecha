using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockCamera : MonoBehaviour {
    public Transform pivit;

    private void Update() {
        transform.position = pivit.position;
        transform.rotation = pivit.rotation;
    }
}
