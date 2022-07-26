using System;
using UnityEngine;
using XiheFramework;

public class BodyController : PartControllerBase {
    [Header("Locomotion")]
    public Transform upperBody;

    public Transform lowerBody;

    public Rigidbody bodyRb;

    public float upperRotateSpeed = 10f;
    public float bodyRotateSpeed = 10f;
    public float moveSpeed = 10f;

    public float thrusterSpeed = 10f;

    [Header("Hip Movement")]
    public Rigidbody lowerRb;

    public float desiredHeight = 5f;
    public float springStrength = 10f;
    public float dampingStrength = 10f;

    [Header("IK")]
    public float stepDistance = 5;

    public float stepHeight = 1;
    public float footHeight = 1;

    public float ikSpeed = 1;
    public LayerMask terrainLayer;

    public Vector2 footOffset; //x right y forward
    public Transform leftFootTarget;
    public Transform rightFootTarget;

    private Transform m_CachedTransform;

    private float m_ThrusterCharge;
    private Quaternion m_Rotation;

    //ik temp
    private Vector3 m_CurrentLeftFootPos;
    private Vector3 m_CurrentRightFootPos;
    private Vector3 m_LastLeftFootDest;
    private Vector3 m_LastRightFootDest;
    private Vector3 m_NewLeftFootDest; //destination
    private Vector3 m_NewRightFootDest;

    //rotation destinations
    private Quaternion m_CurrentLeftFootRot;
    private Quaternion m_CurrentRightFootRot;
    private Quaternion m_LastLeftFootRot;
    private Quaternion m_LastRightFootRot;
    private Quaternion m_NextLeftFootRot;
    private Quaternion m_NextRightFootRot;

    private float m_LeftFootLerp;
    private float m_RightFootLerp;
    private bool m_LastUpdatedLeft = true; //switch between two legs (one by one)

    protected override void Start() {
        base.Start();

        m_CachedTransform = transform;

        Debug.Log("Set Start Pos");
        m_NewLeftFootDest = leftFootTarget.position;
        m_NewRightFootDest = rightFootTarget.position;

        m_LastLeftFootRot = leftFootTarget.rotation;
        m_LastRightFootRot = rightFootTarget.rotation;
        m_NextLeftFootRot = leftFootTarget.rotation;
        m_NextRightFootRot = rightFootTarget.rotation;
    }

    protected override void Die() {
        gameObject.SetActive(false);
    }

    protected override void OnEnterSeat() {
        
    }

    protected override void OnHit(Collider hitCollider, HitEntity hitEntity) {
        throw new NotImplementedException();
    }

    private void Update() {
        if (enableControl) {
            ClearMovement();
            HandleBodyMovement();
            HandleThruster();
        }

        UpdateHipRb();

        UpdateLeftFoot();
        UpdateRightFoot();
    }

    private void UpdateHipRb() {
        if (Physics.Raycast(lowerRb.transform.position, Vector3.down, out RaycastHit hit, 100f, terrainLayer.value)) {
            // Game.Blackboard.SetData("Debug.HitDist", hit.distance);
            // Game.Blackboard.SetData("Debug.desiredHeight", desiredHeight);
            // Game.Blackboard.SetData("Debug.springStrength", springStrength);
            //

            var offsetForce = (hit.distance - desiredHeight) * springStrength;
            // Game.Blackboard.SetData("Debug.offsetForce", offsetForce);
            var velocity = lowerRb.GetPointVelocity(lowerBody.position);
            float yVel = Vector3.Dot(Vector3.down, velocity);
            // Game.Blackboard.SetData("Debug.yVel", yVel);
            var dampingForce = yVel * dampingStrength;
            // Game.Blackboard.SetData("Debug.dampingForce", dampingForce);
            var force = offsetForce - dampingForce;
            // Game.Blackboard.SetData("Debug.force", force);

            lowerRb.AddForce(Vector3.down * force);
        }
    }

    void UpdateLeftFoot() {
        leftFootTarget.position = m_CurrentLeftFootPos;
        leftFootTarget.rotation = m_CurrentLeftFootRot;

        if (m_LastUpdatedLeft) {
            return;
        }

        Ray leftFootRay = new Ray(lowerBody.position - lowerBody.right * footOffset.x + lowerBody.forward * footOffset.y, Vector3.down);

        if (Physics.Raycast(leftFootRay, out RaycastHit leftFootInfo, 100f, terrainLayer.value)) {
            if (Vector3.Distance(leftFootInfo.point, m_NewLeftFootDest) > stepDistance) {
                m_NewLeftFootDest = leftFootInfo.point + Vector3.up * footHeight;
                m_NextLeftFootRot = m_CachedTransform.rotation;
                m_LeftFootLerp = 0;
            }
        }

        if (m_LastUpdatedLeft) {
            return;
        }

        if (m_LeftFootLerp < 1) {
            var footPos = Vector3.Lerp(m_LastLeftFootDest, m_NewLeftFootDest, m_LeftFootLerp);
            footPos.y += Mathf.Sin(m_LeftFootLerp * Mathf.PI) * stepHeight;
            m_CurrentLeftFootPos = footPos;

            var footRot = Quaternion.Lerp(m_LastLeftFootRot, m_NextLeftFootRot, m_LeftFootLerp);
            m_CurrentLeftFootRot = footRot;

            m_LeftFootLerp += Time.deltaTime * ikSpeed;
        }
        else {
            m_LastLeftFootDest = m_NewLeftFootDest;
            m_LastLeftFootRot = m_NextLeftFootRot;

            m_LastUpdatedLeft = true;
        }
    }

    void UpdateRightFoot() {
        rightFootTarget.position = m_CurrentRightFootPos;
        rightFootTarget.rotation = m_CurrentRightFootRot;

        if (!m_LastUpdatedLeft) {
            return;
        }

        Ray rightFootRay = new Ray(lowerBody.position + lowerBody.right * footOffset.x + lowerBody.forward * footOffset.y, Vector3.down);

        if (Physics.Raycast(rightFootRay, out RaycastHit rightFootInfo, 100f, terrainLayer.value)) {
            if (Vector3.Distance(rightFootInfo.point, m_NewRightFootDest) > stepDistance) {
                m_NewRightFootDest = rightFootInfo.point + Vector3.up * footHeight;
                m_NextRightFootRot = m_CachedTransform.rotation;
                m_RightFootLerp = 0;
            }
        }

        //last updated right foot so let left foot update
        if (!m_LastUpdatedLeft) {
            return;
        }

        if (m_RightFootLerp < 1) {
            var footPos = Vector3.Lerp(m_LastRightFootDest, m_NewRightFootDest, m_RightFootLerp);
            footPos.y += Mathf.Sin(m_RightFootLerp * Mathf.PI) * stepHeight;
            m_CurrentRightFootPos = footPos;

            var footRot = Quaternion.Lerp(m_LastRightFootRot, m_NextRightFootRot, m_RightFootLerp);
            m_CurrentRightFootRot = footRot;

            m_RightFootLerp += Time.deltaTime * ikSpeed;
        }
        else {
            m_LastRightFootDest = m_NewRightFootDest;
            m_LastRightFootRot = m_NextRightFootRot;

            m_LastUpdatedLeft = false;
        }
    }

    private void ClearMovement() {
    }

    private void HandleThruster() {
        if (Game.Input.GetKey("ChargeThruster")) {
            m_ThrusterCharge += Time.deltaTime;
        }

        if (Game.Input.GetKeyUp("ChargeThruster")) {
            var force = m_CachedTransform.up * (m_ThrusterCharge * thrusterSpeed) / 2f + upperBody.forward * (m_ThrusterCharge * thrusterSpeed);
            Game.Blackboard.SetData("Debug.ThrusterForce", force.magnitude);
            bodyRb.AddForce(m_CachedTransform.up * (m_ThrusterCharge * thrusterSpeed) / 2f + upperBody.forward * (m_ThrusterCharge * thrusterSpeed));
        }

        Game.Blackboard.SetData("Debug.ChargeThruster", m_ThrusterCharge);
    }

    private void HandleBodyMovement() {
        var wasd = Game.Input.GetWASDInput(); //lower body
        var arrow = Game.Input.GetArrowInput(); //upper body

        //rotate upper body
        upperBody.Rotate(upperBody.up, arrow.x * upperRotateSpeed * Time.deltaTime);
        //rotate lower body
        m_CachedTransform.Rotate(m_CachedTransform.up, wasd.x * bodyRotateSpeed * Time.deltaTime);
        //move lower body
    }

    private void OnDrawGizmos() {
        if (lowerBody == null) {
            return;
        }

        Gizmos.color = Color.red;
        var leftShift = lowerBody.position - lowerBody.right * footOffset.x + lowerBody.forward * footOffset.y;
        var rightShift = lowerBody.position + lowerBody.right * footOffset.x + lowerBody.forward * footOffset.y;
        Gizmos.DrawLine(lowerBody.position, leftShift);
        Gizmos.DrawLine(lowerBody.position, rightShift);
        Gizmos.DrawLine(leftShift, leftShift - lowerBody.up * 100f);
        Gizmos.DrawLine(rightShift, rightShift - lowerBody.up * 100f);

        Gizmos.DrawSphere(m_NewLeftFootDest, 1f);
        Gizmos.DrawSphere(m_NewRightFootDest, 1f);
        // Gizmos.DrawLine(m_CachedTransform.position, transform.position + transform.forward * stepDistance);
    }
}