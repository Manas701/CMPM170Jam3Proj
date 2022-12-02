using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxLandingNoise : MonoBehaviour
{
    [Header("Components")]
    private bool wasInAir;
    [SerializeField] private BoxCollider2D hitbox;
    [SerializeField] private LayerMask groundLayer;

    [Header("Sound")]
    [SerializeField] private AudioSource audioPlayer;
    [SerializeField] private AudioClip landingSFX;



    private void Start()
    {
        wasInAir = !OnGround();
    }

    private void Update()
    {
        if (OnGround())
        {
            if (wasInAir)
            {
                audioPlayer.PlayOneShot(landingSFX);
            }
            wasInAir = false;
            
        } 
        else
        {
            wasInAir = true;
        }
    }

    private bool OnGround()
    {
        RaycastHit2D raycastHitGround = Physics2D.BoxCast(hitbox.bounds.center, hitbox.bounds.size + (hitbox.bounds.size / 20), 0, Vector2.zero, 0.05f, groundLayer);
        return raycastHitGround.collider != null;
    }
}
