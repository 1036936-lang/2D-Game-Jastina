using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class FreezeEnemies : MonoBehaviour
{
    private bool isFreezing;

    private readonly List<PausedEnemy> pausedEnemies = new();

    private class PausedEnemy
    {
        public EnemyChase movement;
        public Rigidbody2D body;
        public RigidbodyConstraints2D originalConstraints;
    }

    private void Update()
    {
        if (Keyboard.current == null || Time.timeScale == 0f)
            return;

        if (Keyboard.current.fKey.wasPressedThisFrame && !isFreezing)
        {
            StartCoroutine(PauseEnemies());
        }
    }

    private IEnumerator PauseEnemies()
    {
        isFreezing = true;

        float finishTime = Time.realtimeSinceStartup + 3f;

        while (Time.realtimeSinceStartup < finishTime)
        {
            EnemyChase[] enemies =
                FindObjectsByType<EnemyChase>(
                    FindObjectsSortMode.None
                );

            foreach (EnemyChase enemy in enemies)
            {
                if (!enemy.enabled || IsAlreadyPaused(enemy))
                    continue;

                PausedEnemy saved = new PausedEnemy
                {
                    movement = enemy,
                    body = enemy.GetComponent<Rigidbody2D>()
                };

                if (saved.body != null)
                {
                    saved.originalConstraints = saved.body.constraints;
                    saved.body.linearVelocity = Vector2.zero;
                    saved.body.angularVelocity = 0f;
                    saved.body.constraints =
                        RigidbodyConstraints2D.FreezeAll;
                }

                pausedEnemies.Add(saved);
                enemy.enabled = false;
            }

            yield return null;
        }

        ResumeEnemies();
    }

    private bool IsAlreadyPaused(EnemyChase enemy)
    {
        foreach (PausedEnemy saved in pausedEnemies)
        {
            if (saved.movement == enemy)
                return true;
        }

        return false;
    }

    private void ResumeEnemies()
    {
        foreach (PausedEnemy saved in pausedEnemies)
        {
            if (saved.body != null)
            {
                saved.body.constraints = saved.originalConstraints;
            }

            if (saved.movement != null)
            {
                saved.movement.enabled = true;
            }
        }

        pausedEnemies.Clear();
        isFreezing = false;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        ResumeEnemies();
    }
}