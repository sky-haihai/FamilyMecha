using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XiheFramework;
using Object = System.Object;

public abstract class PartControllerBase : MonoBehaviour {
    [Header("Part Setting")]
    public MechaSeatType type;

    public bool enableControl;

    public float maxHp;
    public float downThreshold;//how much damage to take before fall off
    
    protected float currentHp;
    protected float currentDown;

    private bool m_DeadLastFrame;

    protected virtual void Start() {
        Game.Event.Subscribe("Mecha.OnSwitchSeat", OnSwitchSeat);
        Game.Event.Subscribe("Hit.OnHit", OnHit);

        currentHp = maxHp;
        currentDown = 0;
    }

    protected virtual void Update() {
        CheckDeath();
    }

    protected abstract void Die();

    protected abstract void OnEnterSeat();


    protected abstract void OnHit(Collider hitCollider, HitEntity hitEntity);

    private void OnHit(object sender, object e) {
        var ns = sender as Collider;
        var ne = e as HitEntity;
        if (ns == null) {
            return;
        }

        if (ne == null) {
            return;
        }

        OnHit(ns, ne);
    }

    private void OnSwitchSeat(object sender, object e) {
        var ne = (MechaSeatType) e;
        if (ne != type) {
            enableControl = false;
            return;
        }

        OnEnterSeat();

        //activate control
        enableControl = true;
    }

    private void CheckDeath() {
        //delay death by 1 frame
        //let some skill get a chance to heal up
        if (currentHp<=0) {
            if (m_DeadLastFrame) {
                Die();
            }
            else {
                m_DeadLastFrame = true;
            }
        }
    }
}