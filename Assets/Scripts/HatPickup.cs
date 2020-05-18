using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HatPickup : MonoBehaviourPunCallbacks
{
    private void OnTriggerEnter( Collider other )
    {
        if ( other.gameObject.CompareTag( "Player" ) )
        {
            GameManager.instance.photonView.RPC( "GiveHat", RpcTarget.All, other.gameObject.GetComponent<PlayerController>().id );
            gameObject.SetActive( false );
        }
    }
}
