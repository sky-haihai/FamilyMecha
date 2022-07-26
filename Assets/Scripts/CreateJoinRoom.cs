using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class CreateJoinRoom : MonoBehaviourPunCallbacks {
    public string createRoomName;
    public string joinRoomName;

    public void CreateRoom() {
        PhotonNetwork.CreateRoom(createRoomName);
    }

    public void JoinRoom() {
        PhotonNetwork.JoinRoom(joinRoomName);
    }

    public override void OnJoinedRoom() {
        base.OnJoinedRoom();
        
        PhotonNetwork.LoadLevel("Game");
    }
}