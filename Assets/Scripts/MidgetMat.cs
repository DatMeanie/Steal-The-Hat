using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MidgetMat : PlayerController
{
    ///////////////////////////////////////////////////////////////
    // VARIABLES
    ///////////////////////////////////////////////////////////////

    [Header("Midget Mat Stats")]

    public float dashSpeed = 8.0f;
    public float dashTime = 0.5f;
    public float hatSlowdown = 1.0f;

    ///////////////////////////////////////////////////////////////

    bool isDashing = false;

    ///////////////////////////////////////////////////////////////

    protected override void Move()
    {
        ///////////////////////////////////////////////////////////////

        float moveX = Input.GetAxisRaw( "Horizontal" );
        float moveZ = Input.GetAxisRaw( "Vertical" );

        if ( isDashing == false )
        {
            moveDir = ( transform.forward * moveZ + transform.right * moveX );
            forwardDirection = moveDir;
            if ( hatObject.activeInHierarchy == false )
                moveDir *= moveSpeed;
            else
                moveDir *= moveSpeed - hatSlowdown;
        }
        else if ( isDashing == true )
        {
            moveDir = forwardDirection * dashSpeed;
        }

        ///////////////////////////////////////////////////////////////

        if ( isDashing == false && Input.GetKeyDown( KeyCode.LeftShift ) && hatObject.activeInHierarchy == false && moveX != 0.0f ||
            isDashing == false && Input.GetKeyDown( KeyCode.LeftShift ) && hatObject.activeInHierarchy == false && moveZ != 0.0f )
        {
            StartCoroutine( UseDash() );
        }

        ///////////////////////////////////////////////////////////////
        // Y AXIS
        ///////////////////////////////////////////////////////////////

        if ( Input.GetKeyDown( KeyCode.Space ) && timesJumped < maxJumps )
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

    ////////////////////////////////////////////////////////////////
    //
    //                  IENUMERATORS
    //
    ////////////////////////////////////////////////////////////////

    IEnumerator UseDash()
    {
        isDashing = true;
        yield return new WaitForSeconds( dashTime );
        isDashing = false;
    }

    ////////////////////////////////////////////////////////////////
}
