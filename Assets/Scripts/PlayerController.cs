using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    ///////////////////////////////////////////////////////////////
    // VARIABLES
    ///////////////////////////////////////////////////////////////

    [Header("Base Stats")]

    public int maxJumps = 3;
    
    public float moveSpeed = 5.0f;
    public float jumpForce = 5.0f;
    public float gravity = 9.81f;

    public GameObject hatObject = null;
    protected CharacterController controller = null;
    public Player photonPlayer = null;
    public TextMeshPro nameText = null;

    ///////////////////////////////////////////////////////////////

    protected Vector3 moveDir = new Vector3();
    protected Vector3 forwardDirection = new Vector3();

    protected float ySpeed = 0.0f;
    [HideInInspector] public float curHatTime = 0.0f;
    
    [HideInInspector] public int id = 0;
    [HideInInspector] public int wins = 0;
    protected int timesJumped = 0;

    protected new bool enabled = false;
    protected bool jumpedRecently = false;

    ///////////////////////////////////////////////////////////////

    [PunRPC]
    public virtual void Initialize( Player player )
    {
        photonPlayer = player;
        id = player.ActorNumber;
        GameManager.instance.players[ id - 1 ] = this;
        
        string nickname = player.NickName;
        if ( nickname.Length > 55 )
            nameText.text = nickname.Substring( 0, 55 );
        else
            nameText.text = nickname;

        if ( photonView.IsMine == false )
        {
            transform.Find( "Camera" ).gameObject.SetActive( false );
        }
    }
    
    ////////////////////////////////////////////////////////////

    protected virtual void Start()
    {
        enabled = true;
        controller = GetComponent<CharacterController>();
    }
    
    ///////////////////////////////////////////////////////////////

    protected virtual void Update()
    {
        if( PhotonNetwork.IsMasterClient )
        {
            if( curHatTime >= GameManager.instance.timeToWin && GameManager.instance.gameEnded == false )
            {
                GameManager.instance.gameEnded = true;
                GameManager.instance.photonView.RPC( "WinGame", RpcTarget.All, id );
            }
        }

        if( enabled == true && photonView.IsMine == true )
        {
            Move();

            // track the amount of time we're wearing the hat
            if( hatObject.activeInHierarchy )
            {
                curHatTime += Time.deltaTime;
            }
        }
    }
    
    ////////////////////////////////////////////////////////////

    protected virtual void FixedUpdate()
    {
        if ( photonView.IsMine == false )
            return;

        ////////////////////////////////////////////////////////////
        // CHECK FOR COLLISION FROM ABOVE
        ////////////////////////////////////////////////////////////

        RaycastHit hit;
        if ( Physics.Raycast( transform.position, Vector3.up, out hit, 1.2f ) == true )
        {
            if ( ySpeed > 0.0f && hit.collider.CompareTag( "Wall" ) == true )
                ySpeed = 0.0f;
        }

        ////////////////////////////////////////////////////////////
    }

    ///////////////////////////////////////////////////////////////
    // INPUT
    ///////////////////////////////////////////////////////////////

    protected virtual void Move()
    {
        ///////////////////////////////////////////////////////////////

        float moveX = Input.GetAxisRaw( "Horizontal" );
        float moveZ = Input.GetAxisRaw( "Vertical" );

        moveDir = ( transform.forward * moveZ + transform.right * moveX ) * moveSpeed;

        ///////////////////////////////////////////////////////////////
        // Y AXIS
        ///////////////////////////////////////////////////////////////

        if ( Input.GetKeyDown( KeyCode.Space ) && timesJumped < maxJumps  )
        {
            ySpeed = jumpForce;
            timesJumped++;
            StartCoroutine( UseJumpRecently() );
        }

        if ( controller.isGrounded == true && jumpedRecently == false )
        {
            timesJumped = 0;
            ySpeed = 0.0f;
        }

        ySpeed -= gravity * Time.deltaTime;
        moveDir.y += ySpeed;
        
        ///////////////////////////////////////////////////////////////

        controller.Move( moveDir * Time.deltaTime );
        
        ///////////////////////////////////////////////////////////////
    }
    
    ///////////////////////////////////////////////////////////////

    public virtual void Respawn( Vector3 position, Quaternion rotation )
    {
        controller.enabled = false;
        
        moveDir = Vector3.zero;
        transform.position = position;
        transform.rotation = rotation;
        ySpeed = 0.0f;
        timesJumped = 0;
        controller.enabled = true;
    }

    public virtual void ResetPlayer()
    {
        curHatTime = 0.0f;
        GameManager.instance.RespawnPlayer( id );
    }

    ///////////////////////////////////////////////////////////////
    // HAT FUNCTIONS
    ///////////////////////////////////////////////////////////////

    public void SetHat( bool hasHat )
    {
        hatObject.SetActive( hasHat );
    }

    ////////////////////////////////////////////////////////////////

    protected virtual void OnControllerColliderHit( ControllerColliderHit hit )
    {
        if ( photonView.IsMine == false )
            return;

        ///////////////////////////////////////////////////////////////

        if ( hit.gameObject.CompareTag( "Player" ) )
        {
            if ( GameManager.instance.GetPlayer( hit.gameObject ).id == GameManager.instance.playerWithHat )
            {
                if ( GameManager.instance.CanGetHat() )
                {
                    GameManager.instance.photonView.RPC( "GiveHat", RpcTarget.All, id );
                }
            }
        }

        ///////////////////////////////////////////////////////////////
    }

    ///////////////////////////////////////////////////////////////

    public void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info )
    {
        if( stream.IsWriting )
        {
            stream.SendNext( curHatTime );
        }
        else if( stream.IsReading )
        {
            curHatTime = (float)stream.ReceiveNext();
        }
    }

    ////////////////////////////////////////////////////////////////
    //
    //                  IENUMERATORS
    //
    ////////////////////////////////////////////////////////////////

    protected virtual IEnumerator UseJumpRecently()
    {
        jumpedRecently = true;
        yield return new WaitForSeconds( 0.05f );
        jumpedRecently = false;
    }

    ////////////////////////////////////////////////////////////////
}
