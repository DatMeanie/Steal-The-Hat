using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.ComponentModel;

public class MainMenuManager : MonoBehaviourPunCallbacks
{
    ///////////////////////////////////////////////////////////////
    // VARIABLES
    ///////////////////////////////////////////////////////////////

    [Header("Screens")]
    public GameObject mainScreen;
    public GameObject lobbyScreen;

    [Header("Main Screen")]
    public Button createRoomButton;
    public Button joinRoomButton;

    [Header("Lobby Screen")]
    public TextMeshProUGUI playerListText;
    public TMP_Dropdown levelDropdown;
    public Button startGameButton;

    [Header("Player choose Screen")]
    public Image prefabImage = null;
    public TextMeshProUGUI prefabNameText = null;
    public TextMeshProUGUI prefabDescText = null;
    public List<PlayerPrefabUI> playerPrefabs = new List<PlayerPrefabUI>();
    [HideInInspector] public List<PlayerValues> players = new List<PlayerValues>();

    string level = "Level 1";
    int selectedPlayerPrefab = 0;

    public static MainMenuManager instance;

    ///////////////////////////////////////////////////////////////

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        createRoomButton.interactable = false;
        joinRoomButton.interactable = false;
        SetScreen( mainScreen );

        prefabImage.sprite = playerPrefabs[ 0 ].sprite;
        prefabNameText.text = playerPrefabs[ 0 ].name;
        prefabDescText.text = playerPrefabs[ 0 ].description.Replace( "\\n", "\n" );
        selectedPlayerPrefab = 0;
    }

    public override void OnConnectedToMaster()
    {
        createRoomButton.interactable = true;
        joinRoomButton.interactable = true;
    }

    void SetScreen( GameObject screen )
    {
        mainScreen.SetActive( false );
        lobbyScreen.SetActive( false );
        screen.SetActive( true );
    }

    ///////////////////////////////////////////////////////////////
    //
    //                  ROOM FUNCTIONS
    //
    ///////////////////////////////////////////////////////////////

    public void OnCreateRoom( TMP_InputField roomNameInput )
    {
        NetworkManager.instance.CreateRoom( roomNameInput.text );
    }
   
    ///////////////////////////////////////////////////////////////

    public void JoinRoom( TMP_InputField roomNameInput )
    {
        NetworkManager.instance.JoinRoom( roomNameInput.text );
    }
    
    ///////////////////////////////////////////////////////////////

    public void OnLeaveLobby()
    {
        PhotonNetwork.LeaveLobby();
        SetScreen( mainScreen );
    }
    
    ///////////////////////////////////////////////////////////////

    public override void OnJoinedRoom()
    {
        SetScreen( lobbyScreen );
        photonView.RPC( "UpdateLobbyUI", RpcTarget.All );
        photonView.RPC( "ChangePlayerPrefab", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer.ActorNumber, playerPrefabs[ selectedPlayerPrefab ].name );
    }

    ///////////////////////////////////////////////////////////////

    public void OnPlayerPrefabSwitch( int amount )
    {
        selectedPlayerPrefab += amount;
        if ( selectedPlayerPrefab >= playerPrefabs.Count )
            selectedPlayerPrefab = 0;
        if ( selectedPlayerPrefab < 0 )
            selectedPlayerPrefab = playerPrefabs.Count - 1;

        prefabImage.sprite = playerPrefabs[ selectedPlayerPrefab ].sprite;
        prefabNameText.text = playerPrefabs[ selectedPlayerPrefab ].name;
        prefabDescText.text = playerPrefabs[ selectedPlayerPrefab ].description.Replace( "\\n", "\n" );
        photonView.RPC( "ChangePlayerPrefab", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer.ActorNumber, playerPrefabs[ selectedPlayerPrefab ].name );
    }

    [PunRPC]
    void ChangePlayerPrefab( int playerId, string prefab )
    {
        players[ playerId - 1 ].playerPrefab = prefab;
    }

    ///////////////////////////////////////////////////////////////

    public void OnPlayerNameUpdate( TMP_InputField playerNameInput )
    {
        PhotonNetwork.NickName = playerNameInput.text;
    }

    ///////////////////////////////////////////////////////////////

    // This function will be called on all clients
    public override void OnPlayerLeftRoom( Player otherPlayer )
    {
        UpdateLobbyUI();
    }
    
    ///////////////////////////////////////////////////////////////

    [PunRPC]
    public void UpdateLobbyUI()
    {
        playerListText.text = "";

        // display all players currently in the lobby
        foreach ( Player player in PhotonNetwork.PlayerList )
        {
            playerListText.text += player.NickName + "\n";
            int index = players.FindIndex( x => x.id == player.ActorNumber );
            if( index < 0 )
            {
                players.Add( new PlayerValues() );
                players[ players.Count - 1 ].id = player.ActorNumber;
            }
        }

        // only the host can start the game
        if ( PhotonNetwork.IsMasterClient == true )
        {
            levelDropdown.gameObject.SetActive( true );
            startGameButton.interactable = true;
        }
        else
        {
            levelDropdown.gameObject.SetActive( false );
            startGameButton.interactable = false;
        }
    }
    
    ///////////////////////////////////////////////////////////////

    public void OnLevelChange( TMP_Dropdown levelDropdown )
    {
        level = levelDropdown.options[ levelDropdown.value ].text;
    }
    
    ///////////////////////////////////////////////////////////////

    public void OnStartGameButton()
    {
        NetworkManager.instance.photonView.RPC( "ChangeScene", RpcTarget.All, level );
    }

    ///////////////////////////////////////////////////////////////
}

///////////////////////////////////////////////////////////////

[System.Serializable]
public class PlayerPrefabUI
{
    public Sprite sprite;
    public string name;
    public string description;
}