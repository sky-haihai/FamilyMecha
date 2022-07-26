using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using XiheFramework;
using Object = System.Object;

public class DefaultBodyPart : PartControllerBase {
    [Header("Keep Height")]
    public float desiredHeight = 7f;

    public float offsetStrength = 50f;
    public float dampingStrength = 10f;
    public float heightForceTolerance = 1f;

    [Header("Chest Movement")]
    public Transform chestBone;

    public float chestRotateSpeed;

    [Header("Hip Movement")]
    public float hipRotateSpeed;

    public float maxSpeed;
    public float acceleration;
    public float maxAccelerationForce;

    [Header("IK Foot")]
    public Transform leftIkTarget;

    public Transform rightIkTarget;
    public float stepDistance = 5f;
    public float stepHeight = 1f;
    public float stepDuration = 1f;
    public float stepOverShoot = 3f; //how much step over shoot by dir
    public float rayDistance = 100f;
    public Vector3 rayOffset = new Vector3(2, 0, 0);

    public LayerMask terrainLayer;

    [Header("Head Tracking")]
    public Transform headBone;

    public Transform headBoneTarget; //switch to vector3 data later
    public float headTrackingSpeed = 1f;

    [Header("Arm Connection")]
    public Transform leftArmPivit;

    public Transform rightArmPivit;

    //locomotion
    private Transform m_CachedTransform;
    private Vector3 m_GoalVelocity;

    //keep height
    private Rigidbody m_HipRb;

    //head tracking
    private Vector3 m_StartHeadDir;
    private Vector3 m_HeadTargetPos; //pass by blackboard later
    private float m_HeadTrackingLerp;

    //ik
    private bool m_IkMoving;

    protected override void Start() {
        base.Start();

        m_HipRb = GetComponent<Rigidbody>();
        m_CachedTransform = transform;
        m_StartHeadDir = headBone.forward;

        //share connection point
        Game.Blackboard.SetData("Team01.LeftArmPivit", leftArmPivit);
        Game.Blackboard.SetData("Team01.RightArmPivit", rightArmPivit);
    }

    protected override void Die() {
        gameObject.SetActive(false);
    }

    protected override void OnEnterSeat() {
    }

    protected override void OnHit(Collider hitCollider, HitEntity hitEntity) {
        //TODO: implement hit
    }

    protected override void Update() {
        LookAtTarget();
    }

    private void FixedUpdate() {
        KeepDesiredHeight();

        if (enableControl) {
            HandleMovement();
        }

        FindFootPosition(rayOffset, leftIkTarget);
        FindFootPosition(new Vector3(-rayOffset.x, rayOffset.y, rayOffset.z), rightIkTarget);
    }

    private void LateUpdate() {
    }

    // void AddRootMotion() {
    //     root.position = m_CachedTransform.position;
    //     m_CachedTransform.localPosition = Vector3.zero;
    //
    //     root.rotation = m_CachedTransform.rotation;
    //     m_CachedTransform.localRotation = Quaternion.identity;
    // }

    void LookAtTarget() {
        if (headBone == null) {
            return;
        }

        Vector3 dir;
        if (headBoneTarget == null) {
            dir = m_StartHeadDir;
        }
        else {
            dir = headBoneTarget.position - headBone.position;
        }

        // Vector3 dir = m_HeadTargetPos - headBone.position;
        var target = Quaternion.LookRotation(dir, transform.up);

        var rot = Quaternion.RotateTowards(headBone.rotation, target, headTrackingSpeed);

        headBone.rotation = rot;
    }

    private void HandleMovement() {
        var wasd = Game.Input.GetWASDInput(); //lower body
        var arrow = Game.Input.GetArrowInput(); //upper body

        //hip position
        //new goal vel at current update
        Vector3 newGoalVel = m_CachedTransform.forward * (wasd.y * maxSpeed);

        //interpolate from last udpate to current update to get a overall new goal vel 
        m_GoalVelocity = Vector3.MoveTowards(m_GoalVelocity, newGoalVel, acceleration * Time.fixedDeltaTime);

        var neededAcceleration = (m_GoalVelocity - m_HipRb.velocity) / Time.fixedDeltaTime;
        var maxAcceleration = maxAccelerationForce; //implement curve sampler later
        neededAcceleration = Vector3.ClampMagnitude(neededAcceleration, maxAcceleration);

        m_HipRb.AddForce(neededAcceleration * m_HipRb.mass);

        //hip rotation
        m_CachedTransform.Rotate(m_CachedTransform.up, wasd.x * hipRotateSpeed * Time.deltaTime);

        //chest rotation
        chestBone.Rotate(chestBone.up, arrow.x * chestRotateSpeed * Time.deltaTime);
    }

    void KeepDesiredHeight() {
        if (m_HipRb == null) {
            return;
        }

        if (Physics.Raycast(m_HipRb.transform.position, Vector3.down, out RaycastHit hit, rayDistance, terrainLayer.value)) {
            var offsetForce = (hit.distance - desiredHeight) * offsetStrength;
            var velocity = m_HipRb.GetPointVelocity(transform.position);
            float yVel = Vector3.Dot(Vector3.down, velocity);
            var dampingForce = yVel * dampingStrength;
            var force = offsetForce - dampingForce;

            //ignore tiny force to prevent jitter movement
            if (Mathf.Abs(force) > heightForceTolerance) {
                m_HipRb.AddForce(Vector3.down * force);
            }
        }
    }

    private void FindFootPosition(Vector3 localStartPos, Transform ikTarget) {
        if (ikTarget == null) {
            return;
        }

        if (m_IkMoving) {
            return;
        }

        var globalStartPos = m_CachedTransform.TransformPoint(localStartPos);
        if (Physics.Raycast(globalStartPos, -transform.up, out RaycastHit hit, rayDistance, terrainLayer)) {
            //hit too far(player y axis too big
            if (hit.distance > 8f) {
                var target = globalStartPos + Vector3.down * desiredHeight;
                ikTarget.position = target;
                ikTarget.rotation = m_CachedTransform.rotation;
            }
            else if (Vector3.Distance(hit.point, ikTarget.position) > stepDistance) {
                var pos = hit.point + Vector3.up * stepHeight;
                StartCoroutine(MoveTowardHit(ikTarget, pos, hit.normal));
            }
        }
    }

    IEnumerator MoveTowardHit(Transform ikTarget, Vector3 hitPoint, Vector3 normal) {
        m_IkMoving = true;

        Quaternion startRot = ikTarget.rotation;
        Vector3 startPos = ikTarget.position;

        Quaternion endRot = Quaternion.LookRotation(m_CachedTransform.forward, normal);
        Vector3 endPos = hitPoint + (hitPoint - startPos).normalized * stepOverShoot;
        // Vector3 endPos = (hitPoint - startPos).normalized * stepDistance + startPos;

        float timeElapsed = 0; //time elapsed

        while (timeElapsed < stepDuration) {
            float lerp = timeElapsed / stepDuration;

            // Interpolate position and rotation
            var pos = Vector3.Lerp(startPos, endPos, lerp);
            pos.y += Mathf.Sin(lerp * Mathf.PI) * stepHeight;
            ikTarget.position = pos;

            ikTarget.rotation = Quaternion.Slerp(startRot, endRot, lerp);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        //hard set position and rotation since there should be less than Time.delta*100 percent(about 1.67%) of difference left at most
        ikTarget.position = endPos;
        ikTarget.rotation = endRot;

        m_IkMoving = false;
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.blue;

        var startPosL = transform.TransformPoint(rayOffset);
        if (Physics.Raycast(startPosL, -transform.up, out RaycastHit hitL, rayDistance)) {
            // Debug.Log("hitL " + hitL.collider.gameObject.name);
            Gizmos.DrawLine(startPosL, hitL.point);
            Gizmos.DrawWireSphere(hitL.point, 1f);
        }

        var startPosR = transform.TransformPoint(new Vector3(-rayOffset.x, rayOffset.y, rayOffset.z));
        if (Physics.Raycast(startPosR, -transform.up, out RaycastHit hitR, rayDistance)) {
            // Debug.Log("hitR " + hitR.collider.gameObject.name);
            Gizmos.DrawLine(startPosR, hitR.point);
            Gizmos.DrawWireSphere(hitR.point, 1f);
        }
    }
}