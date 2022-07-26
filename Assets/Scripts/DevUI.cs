using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XiheFramework;

public class DevUI : UIBehaviour {
    public Button switchBodyBtn;
    public Button switchLeftArmBtn;
    public Button switchRightArmBtn;

    public override void Start() {
        base.Start();

        switchBodyBtn.onClick.AddListener(() => { Game.Event.Invoke("Mecha.OnSwitchSeat", this, MechaSeatType.Body); });
        switchLeftArmBtn.onClick.AddListener(() => { Game.Event.Invoke("Mecha.OnSwitchSeat", this, MechaSeatType.LeftArm); });
        switchRightArmBtn.onClick.AddListener(() => { Game.Event.Invoke("Mecha.OnSwitchSeat", this, MechaSeatType.RightArm); });
    }
}