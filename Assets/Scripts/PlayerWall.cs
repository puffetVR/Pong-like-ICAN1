using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWall : Base
{
    public Player player { get; private set; }

    public int wallHealth = 3;
    public SpriteRenderer wallSprite;
    public BoxCollider2D wallCollider;
    public Animator wallAnimator;

    public void SetPlayerOwner(Player owner)
    {
        player = owner;
    }

    private void Update()
    {
        if (IsInit == false) return;

        WallHealthHandling();
    }

    private void WallHealthHandling()
    {
        if (wallHealth <= 0)
        {
            WallStateHandler(false);
            return;
        }

        WallStateHandler(true);

        switch (wallHealth)
        {
            case 1:
                wallSprite.color = Game.LoHealthColor;
                break;
            case 2:
                wallSprite.color = Game.MedHealthColor;
                break;
            case 3:
                wallSprite.color = Game.HiHealthColor;
                break;
            default:
                wallSprite.color = new Color(1, .5f, 1, 1) ;
                break;
        }
    }

    public void DamageWall()
    {
        wallHealth--;
        wallAnimator.SetTrigger("Hit");

        WallHealthHandling();
    }

    public void WallReset()
    {
        wallHealth = 3;
        WallStateHandler(true);
    }

    void WallStateHandler(bool state)
    {
        wallSprite.enabled = state;
        wallCollider.enabled = state;
    }
}
