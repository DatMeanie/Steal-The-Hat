using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    ///////////////////////////////////////////////////////////////
    // VARIABLES
    ///////////////////////////////////////////////////////////////

    public static NetworkManager instance = null;
    public List<PlayerValues> players = new List<PlayerValues>();

    ///////////////////////////////////////////////////////////////

    void Awake()
    {
        if ( instance != this && instance != null )
            Destroy( gameObject );
        else
        {
            instance = this;
            DontDestroyOnLoad( gameObject );
        }
    }

    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }
    
    public PlayerValues GetPlayer( string nickName )
    {
        return players.First( x => x.name == nickName );
    }

    ///////////////////////////////////////////////////////////////

    public override void OnConnectedToMaster()
    {
        //Debug.Log( "Connected to master server" );
    }

    public override void OnCreatedRoom()
    {
        //Debug.Log( "Created room: " + PhotonNetwork.CurrentRoom.Name );
    }

    public override void OnPlayerLeftRoom( Player otherPlayer )
    {

    }

    ///////////////////////////////////////////////////////////////

    public void CreateRoom( string roomName )
    {
        PhotonNetwork.CreateRoom( roomName );
    }

    public void JoinRoom( string roomName )
    {
        PhotonNetwork.JoinRoom( roomName );
    }

    ///////////////////////////////////////////////////////////////

    public void SetPlayerValues( List<PlayerValues> values )
    {
        players = values;
    }

    [PunRPC]
    public void ChangeScene( string sceneName )
    {
        players = MainMenuManager.instance.players;
        PhotonNetwork.LoadLevel( sceneName );
    }
    
    ///////////////////////////////////////////////////////////////
}

///////////////////////////////////////////////////////////////

public class PlayerValues
{
    public string name = "Player";
    public int id = 1;
    public string playerPrefab = "Player";
}
