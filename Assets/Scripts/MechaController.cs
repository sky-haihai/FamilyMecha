using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XiheFramework;

public enum MechaSeatType {
    Null = 0,
    Body,
    LeftArm,
    RightArm,
    EscapePod,
}

public class MechaController : MonoBehaviour {
    // public MechaSeatType seatType; //body, left, right,

    public Rigidbody rootRb;

    private Vector3 m_RootVelocity;
    private StateMachine m_SeatStateMachine;

    private void Start() {
        m_SeatStateMachine = Game.Fsm.CreateStateMachine("Mecha.Seat");
        m_SeatStateMachine.AddState("Idle", new IdleState(m_SeatStateMachine, this));
        m_SeatStateMachine.AddState("Body", new BodyState(m_SeatStateMachine, this));
        m_SeatStateMachine.AddState("LeftArm", new LeftArmState(m_SeatStateMachine, this));
        m_SeatStateMachine.AddState("RightArm", new RightArmState(m_SeatStateMachine, this));
        m_SeatStateMachine.SetDefaultState("Idle");
        m_SeatStateMachine.Start();
    }

    private void FixedUpdate() {
        if (Input.GetKey(KeyCode.W)) {
            rootRb.AddForce(transform.forward * RigidBodyHelper.GetRequiredAcceleraton(10f, rootRb.drag), ForceMode.Acceleration);
        }
    }

    private void Update() {
        // m_RootVelocity = Game.Blackboard.GetData<Vector3>("Mecha.RootVelocity");
        //
        // rootRb.velocity += m_RootVelocity;
    }

    private class IdleState : State<MechaController> {
        public IdleState(StateMachine parentStateMachine, MechaController owner) : base(parentStateMachine, owner) {
        }

        public override void OnEnter() {
        }

        public override void OnUpdate() {
        }

        public override void OnExit() {
        }
    }

    private class BodyState : State<MechaController> {
        public BodyState(StateMachine parentStateMachine, MechaController owner) : base(parentStateMachine, owner) {
        }

        public override void OnEnter() {
        }

        public override void OnUpdate() {
        }

        public override void OnExit() {
        }
    }

    private class LeftArmState : State<MechaController> {
        public LeftArmState(StateMachine parentStateMachine, MechaController owner) : base(parentStateMachine, owner) {
        }

        public override void OnEnter() {
        }

        public override void OnUpdate() {
        }

        public override void OnExit() {
        }
    }

    private class RightArmState : State<MechaController> {
        public RightArmState(StateMachine parentStateMachine, MechaController owner) : base(parentStateMachine, owner) {
        }

        public override void OnEnter() {
        }

        public override void OnUpdate() {
        }

        public override void OnExit() {
        }
    }
}