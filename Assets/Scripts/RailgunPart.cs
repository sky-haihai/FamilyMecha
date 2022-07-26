using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XiheFramework;

public class RailgunPart : PartControllerBase {
    [Header("Aim")]
    public Transform railgunBone;

    public Transform aimTarget;
    public float angleLimit = 10f;
    public float maxAimSpeed;

    [Header("Snap")]
    public Transform snapTarget;

    public Transform armBone;
    public Rigidbody armRb;
    public float maxSnapSpeed = 2f;
    public float snapAcceleration = 100f;
    public float maxSnapForce = 75f;
    public float startSnapDistance = 20f;
    public float forceSnapDistance = 3f;
    public float forceSnapTolerance = 0.1f;

    [Header("Railgun")]
    public Transform railgunBeamPivit; //where the shot start

    public ShootBeam shootBeam;
    public LayerMask hitLayer; //what layer to apply damage

    [Header("Hitbox")]
    public Collider pilotHitbox;

    public Collider railgunHitbox;
    public Collider forearmHitbox;
    public Collider armHitbox;

    [Header("Defense")]
    public float pilotDef = 20;

    public float railgunDef = 100;
    public float forearmDef = 150;
    public float armDef = 200;

    //aim
    private Vector3 m_AimTargetLocalPos;

    //snap
    //private Transform m_SnapPivit;
    private Vector3 m_GoalVel;
    private bool m_Connected;

    //fire
    private float m_RailgunCharge;

    //debug
    private Vector2 m_AimWorldPos;
    private Vector3 m_AimHitWorldPos;

    protected override void Start() {
        base.Start();

        m_AimTargetLocalPos = aimTarget.localPosition;
    }

    protected override void Die() {
        gameObject.SetActive(false);
    }

    protected override void OnEnterSeat() {
    }

    protected override void OnHit(Collider hitCollider, HitEntity hitEntity) {
        if (hitCollider == pilotHitbox) {
            Debug.Log("pilot got hit");
            DamageFormulaHelper.GetDamage(hitEntity.damage, pilotDef);
        }

        if (hitCollider == railgunHitbox) {
            DamageFormulaHelper.GetDamage(hitEntity.damage, railgunDef);
        }

        if (hitCollider == forearmHitbox) {
            DamageFormulaHelper.GetDamage(hitEntity.damage, forearmDef);
        }

        if (hitCollider == armHitbox) {
            DamageFormulaHelper.GetDamage(hitEntity.damage, armDef);
        }
    }

    private void Update() {
        if (enableControl) {
            HandleAimInput();
            HandleFireInput();
        }

        SetAimRotation();
    }

    private void FixedUpdate() {
        FollowBody();
        HandleAimInput();
        UpdateCrosshair();
    }

    private void FollowBody() {
        if (m_Connected) {
            return;
        }

        if (snapTarget == null) return;

        var delta = snapTarget.position - armBone.position;

        //already close enough
        if (delta.magnitude < forceSnapTolerance) {
            armBone.position = snapTarget.position;
            armBone.rotation = snapTarget.rotation;
            armRb.isKinematic = true;
            m_Connected = true;
            return;
        }

        //within the force snap distance
        if (delta.magnitude <= forceSnapDistance) {
            DisableRagdoll();

            armBone.position = Vector3.Lerp(armBone.position, snapTarget.position, 0.1f);
            armBone.rotation = Quaternion.Slerp(armBone.rotation, snapTarget.rotation, 0.1f);
            return;
        }

        //far away, add connect force
        if (delta.magnitude <= startSnapDistance) {
            EnableRagdoll();

            var goalVel = delta.normalized * maxSnapSpeed;
            m_GoalVel = Vector3.MoveTowards(m_GoalVel, goalVel, snapAcceleration * Time.fixedDeltaTime);

            var neededVel = goalVel - armRb.velocity;
            var force = Vector3.ClampMagnitude(neededVel, maxSnapForce);

            armRb.AddForce(force * armRb.mass);
        }
    }

    void EnableRagdoll() {
        armRb.isKinematic = false;
    }

    void DisableRagdoll() {
        armRb.isKinematic = true;
    }

    private void HandleAimInput() {
        var input = Game.Input.GetWASDInput();
        input = Vector2.ClampMagnitude(input, angleLimit);
        m_AimTargetLocalPos += input.ToVector3(V2ToV3Type.XY);

        aimTarget.localPosition = Vector3.MoveTowards(aimTarget.localPosition, m_AimTargetLocalPos, maxAimSpeed);
    }

    private void SetAimRotation() {
        railgunBone.forward = aimTarget.position - railgunBone.position;
    }

    private void UpdateCrosshair() {
        if (Physics.Raycast(railgunBone.position, railgunBone.up * 0.01f, out RaycastHit hit, 200f, hitLayer)) {
            Game.Blackboard.SetData("LeftArm.AimHitWorldPos", hit.point);
            m_AimHitWorldPos = hit.point;
        }
        else {
            Game.Blackboard.SetData("LeftArm.AimHitWorldPos", aimTarget.position);
            m_AimHitWorldPos = aimTarget.position;
        }
    }

    private void HandleFireInput() {
        if (Game.Input.GetKey("Fire")) {
            m_RailgunCharge += Time.deltaTime;
            m_RailgunCharge = Mathf.Clamp(m_RailgunCharge, 0f, 10f);
        }

        if (Game.Input.GetKeyUp("Fire")) {
            var duration = m_RailgunCharge / 2f;
            var distance = m_RailgunCharge * 10f;

            ApplyHit(distance, duration);
        }
    }

    void ApplyHit(float distance, float duration) {
        var beam = Instantiate(shootBeam, railgunBeamPivit.position, railgunBeamPivit.rotation, railgunBeamPivit);
        beam.Init(duration, distance);
        beam.Play();

        //reset charge
        m_RailgunCharge = 0f;
    }

    private void OnDrawGizmos() {
        if (railgunBone != null) {
            Gizmos.DrawLine(railgunBone.position, m_AimWorldPos);
            Gizmos.DrawSphere(m_AimWorldPos, 1f);

            Gizmos.DrawLine(railgunBone.position, m_AimHitWorldPos);
            Gizmos.DrawSphere(m_AimHitWorldPos, 1f);

            if (aimTarget != null) {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(railgunBone.position, aimTarget.position);
                Gizmos.DrawWireSphere(aimTarget.position, 0.5f);
            }
        }
    }
}