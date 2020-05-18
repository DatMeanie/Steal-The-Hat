using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Security.Authentication;

public class GameManager : MonoBehaviourPunCallbacks
{
    ///////////////////////////////////////////////////////////////
    // VARIABLES
    ///////////////////////////////////////////////////////////////

    [Header("Stats")]
    
    public float timeToWin = 15.0f;
    public float invincibleDuration = 1.0f;

    public bool gameEnded = false;

    [Header("Players")]
    public Transform[] spawnPoints;
    public PlayerController[] players;
    public int playerWithHat = 404;

    public GameObject hat;
    public GameObject mainCamera;

    ///////////////////////////////////////////////////////////////

    float hatPickupTime = 0.0f;
    int playersInGame = 0;

    bool hatOnAPlayer = false;
    bool gameStarted = false;

    ///////////////////////////////////////////////////////////////

    public static GameManager instance;

    ///////////////////////////////////////////////////////////////

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        players = new PlayerController[ PhotonNetwork.PlayerList.Length ];
        photonView.RPC( "ImInGame", RpcTarget.AllBuffered );
    }
    
    ////////////////////////////////////////////////////////////////

    [PunRPC]
    void ImInGame()
    {
        playersInGame++;
        if( playersInGame == PhotonNetwork.PlayerList.Length )
        {
            SpawnPlayer();
            mainCamera.SetActive( false );
            GetComponent<AudioSource>().Play();
        }
    }
    
    ///////////////////////////////////////////////////////////////

    void SpawnPlayer()
    {
        GameObject playerObj = PhotonNetwork.Instantiate( "Midget Mat", spawnPoints[ Random.Range( 0, spawnPoints.Length ) ].position, Quaternion.identity );

        PlayerController controller = playerObj.GetComponent<PlayerController>();
        controller.photonView.RPC( "Initialize", RpcTarget.All, PhotonNetwork.LocalPlayer );
    }
    
    ///////////////////////////////////////////////////////////////

    public void RespawnPlayer( int playerId )
    {
        GetPlayer( playerId ).Respawn( spawnPoints[ Random.Range( 0, spawnPoints.Length ) ].position, Quaternion.identity );
        if ( playerId == playerWithHat )
            photonView.RPC( "SpawnHat", RpcTarget.All );
    }

    public void RespawnPlayer( GameObject playerObj )
    {
        playerObj.GetComponent<PlayerController>().Respawn( spawnPoints[ Random.Range( 0, spawnPoints.Length ) ].position, Quaternion.identity );
        if ( GetPlayer( playerObj ).id == playerWithHat )
            photonView.RPC( "SpawnHat", RpcTarget.All );
    }

    ///////////////////////////////////////////////////////////////
    // GET A PLAYER
    ///////////////////////////////////////////////////////////////

    public PlayerController GetPlayer( int playerId )
    {
        return players.First( x => x.id == playerId );
    }

    public PlayerController GetPlayer( GameObject playerObj )
    {
        return players.First( x => x.gameObject == playerObj );
    }

    ///////////////////////////////////////////////////////////////
    // HAT STUFF
    ///////////////////////////////////////////////////////////////
    
    [PunRPC]
    public void GiveHat( int playerId )
    {
        // remove the hat from the current hatted player
        if( hatOnAPlayer == true )
        {
            GetPlayer( playerWithHat ).SetHat( false );
        }

        //give the hat to the new player

        playerWithHat = playerId;
        GetPlayer( playerId ).SetHat( true );
        hatPickupTime = Time.time;
        hatOnAPlayer = true;
    }
    
    [PunRPC]
    public void SpawnHat()
    {
        if( hatOnAPlayer == true )
            GetPlayer( playerWithHat ).SetHat( false );
        hat.SetActive( true );
        playerWithHat = 404;
        hatOnAPlayer = false;
        hatPickupTime = 0.0f;
    }

    ///////////////////////////////////////////////////////////////

    public bool CanGetHat()
    {
        if ( Time.time > hatPickupTime + invincibleDuration )
            return true;
        else
            return false;
    }
    
    ///////////////////////////////////////////////////////////////

    [PunRPC]
    void WinGame( int playerId )
    {
        gameEnded = true;
        PlayerController player = GetPlayer( playerId );
        player.wins++;

        //set up the UI
        GameUI.instance.SetWinText( player.photonPlayer.NickName );
        GameUI.instance.SetPlayerWinsText( playerId );

        Invoke( "NextRound", 2.0f );
    }

    [PunRPC]
    public void NextRound()
    {
        SpawnHat();
        GameUI.instance.ResetUI();
        gameEnded = false;
        foreach( PlayerController player in players )
        {
            player.ResetPlayer();
        }
    }

    void GoBackToMenu()
    {
        PhotonNetwork.LeaveRoom();
        NetworkManager.instance.ChangeScene( "Main Menu" );
    }
   
    ///////////////////////////////////////////////////////////////
}
