using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XiheFramework;

public class ShootBeam : HitEntity {
    public LayerMask hitMask;
    public LayerMask damageMask;
    public LineRenderer beamLine;
    public MeshCollider beamLineCollider;

    public float duration;
    public float maxDistance;

    public void Init(float duration, float maxDistance) {
        this.duration = duration;
        this.maxDistance = maxDistance;
    }

    public override void Play() {
        Destroy(gameObject, duration);


        beamLine.SetPosition(1, Vector3.forward * maxDistance);

        Mesh mesh = new Mesh();
        beamLine.BakeMesh(mesh);
        beamLineCollider.sharedMesh = mesh;

        beamLineCollider.convex = true;
        beamLineCollider.isTrigger = true;
    }

    private void OnTriggerStay(Collider other) {
        if (damageMask.Contains(other.gameObject.layer)) {
            Debug.Log("damage" + other.name);
        }
        else {
            Debug.Log("no damage" + other.name);
        }

        //Game.Hit.ApplyHit(this, other);
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * maxDistance);
    }
}