using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class GameUI : MonoBehaviour
{
    ///////////////////////////////////////////////////////////////
    // VARIABLES
    ///////////////////////////////////////////////////////////////

    public TextMeshProUGUI winText;
    public Transform playerTimerParent;

    PlayerUIContainer[] playerContainers;
    
    public static GameUI instance;

    ////////////////////////////////////////////////////////////////

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        InitializePlayerUI();
    }

    void InitializePlayerUI()
    {
        playerContainers = new PlayerUIContainer[ playerTimerParent.childCount ];

        ////////////////////////////////////////////////////////////////
        // Loop through all containers
        ////////////////////////////////////////////////////////////////

        for ( int player = 0; player < playerTimerParent.childCount; player++ )
        {
            PlayerUIContainer container = new PlayerUIContainer();
            container.hatTimeSlider = playerTimerParent.GetChild( player ).Find( "Hat Time Slider" ).GetComponent<Slider>();
            container.nameText = playerTimerParent.GetChild( player ).Find( "PlayerName" ).GetComponent<TextMeshProUGUI>();
            container.obj = playerTimerParent.GetChild( player ).gameObject;
            container.winsText = playerTimerParent.GetChild( player ).Find( "PlayerWins" ).GetComponent<TextMeshProUGUI>();
            playerContainers[ player ] = container;

            if ( player < PhotonNetwork.PlayerList.Length )
            {
                container.obj.SetActive( true );
                string nickname = PhotonNetwork.PlayerList[ player ].NickName;
                if( nickname.Length > 22 )
                    container.nameText.text = nickname.Substring( 0, 22 );
                else
                    container.nameText.text = nickname;
                container.hatTimeSlider.maxValue = GameManager.instance.timeToWin;
                container.winsText.text = "0";
            }
            else
            {
                container.obj.SetActive( false );
            }
        }
        
        ////////////////////////////////////////////////////////////////
    }

    ////////////////////////////////////////////////////////////////

    private void Update()
    {
        UpdatePlayerUI();
    }

    void UpdatePlayerUI()
    {
        for ( int player = 0; player < GameManager.instance.players.Length; player++ )
        {
            if( GameManager.instance.players[ player ] != null )
            {
                playerContainers[ player ].hatTimeSlider.value = GameManager.instance.players[ player ].curHatTime;
            }
        }
    }

    public void SetPlayerWinsText( int playerId )
    {
        playerContainers[ playerId - 1 ].winsText.text = GameManager.instance.GetPlayer( playerId ).wins.ToString();
    }

    public void SetWinText( string winnerName )
    {
        winText.text = winnerName + " has won!!!!!";
        winText.gameObject.SetActive( true );
    }

    public void ResetUI()
    {
        winText.text = " ";
        winText.gameObject.SetActive( false );
    }

    ////////////////////////////////////////////////////////////////
}

[System.Serializable]
public class PlayerUIContainer
{
    public GameObject obj;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI winsText;
    public Slider hatTimeSlider;
}