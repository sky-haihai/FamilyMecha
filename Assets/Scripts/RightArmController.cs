using System;
using UnityEngine;
using XiheFramework;

public class RightArmController : PartControllerBase {

    public Transform armPivit;
    protected override void Update() {
        FollowBody();
    }

    protected override void Die() {
         gameObject.SetActive(false);
    }

    private void FollowBody() {
        transform.position = armPivit.position;
        transform.rotation = armPivit.rotation;
    }

    protected override void OnEnterSeat() {
        
    }

    protected override void OnHit(Collider hitCollider, HitEntity hitEntity) {
        throw new NotImplementedException();
    }
}