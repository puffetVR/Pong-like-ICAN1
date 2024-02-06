using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWall : Base
{
    private int wallHealth = 3;
    public SpriteRenderer wallSprite;
    public BoxCollider2D wallCollider;

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
                break;
        }
    }

    public void DamageWall()
    {
        wallHealth--;

        WallHealthHandling();
    }

    void WallStateHandler(bool state)
    {
        wallSprite.enabled = state;
        wallCollider.enabled = state;
    }
}
