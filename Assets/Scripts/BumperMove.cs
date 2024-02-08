using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BumperMove : Base
{
    public float maxDistance = 2f;
    float targetY, currentY, targetMoveSpeed, currentMoveSpeed, baseMoveSpeed = 5f;

    public void ResetBumper()
    {
        int ZeroOrOne = Random.Range(0, 2);
        targetY = ZeroOrOne == 0 ? -maxDistance : maxDistance;
        transform.position = new Vector2(0f, targetY);
        currentY = transform.position.y;
        currentMoveSpeed = baseMoveSpeed;
        targetMoveSpeed = baseMoveSpeed * 2;
    }

    void Update()
    {
        if (!IsInit || !Game.Ball.IsMoving) return;

        Translater();
    }

    void Translater()
    {
        if (transform.position.y >= maxDistance) targetY = -maxDistance;
        else if (transform.position.y <= -maxDistance) targetY = maxDistance;

        currentMoveSpeed = Mathf.MoveTowards(currentMoveSpeed, targetMoveSpeed, Game.deltaTime / 4f);

        currentY = Mathf.MoveTowards(currentY, targetY, Game.deltaTime * currentMoveSpeed);

        transform.position = new Vector2(0f, currentY);



    }
}
