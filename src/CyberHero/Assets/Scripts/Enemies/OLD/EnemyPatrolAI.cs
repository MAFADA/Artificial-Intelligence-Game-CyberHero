using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System;

public class EnemyPatrolAI : MonoBehaviour
{

    [Header("AI Patrol")]
    [SerializeField] private Transform leftEdge;
    [SerializeField] private Transform rightEdge;

    [SerializeField] private Transform enemy;
    [SerializeField] private float speedPatrol;
    private Vector3 initScale;
    private bool movingLeft;
    [SerializeField] private float idleDuration;
    private float idleTimer;


    [Header("AI Movement")]
    [SerializeField] Transform target;
    [SerializeField] float speed = 200f;
    [SerializeField] float nextWaypointDistance = 3f;
    [SerializeField] Transform enemyVisual;
    [SerializeField] float detectionArea;
    private Path path;
    private int currentWaypoint = 0;
    private bool endOfPath = false;
    // [SerializeField] private Animator animator;

    private Seeker seeker;
    private Rigidbody2D rb;
    private float distanceToPlayer;
    public AIState aiState;

    public enum AIState
    {
        Patrol,
        Chase,
    }

    void Start()
    {
        // seeker for pathfinding
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        initScale = enemy.localScale;

        // repeat calling function UpdatePath
        InvokeRepeating(methodName: "UpdatePath", 0f, 0.5f);
    }

    void UpdatePath()
    {
        if (seeker.IsDone())
        {
            if (seeker == null || transform == null)
            {
                return;
            }
            //Draw the path
            seeker.StartPath(rb.position, target.position, OnPathComplete);
        }
    }

    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    void FixedUpdate()
    {
        switch (aiState)
        {
            case AIState.Patrol:

                distanceToPlayer = Vector2.Distance(rb.position, target.position);

                if (distanceToPlayer >= detectionArea)
                {
                    if (movingLeft)
                    {
                        if (enemy.position.x >= leftEdge.position.x)
                        {
                            MoveInDirection(-1);
                        }
                        else
                        {
                            DirectionChange();
                        }

                    }
                    else
                    {
                        if (enemy.position.x <= rightEdge.position.x)
                        {
                            MoveInDirection(1);
                        }
                        else
                        {
                            DirectionChange();
                        }
                    }
                }

                if (distanceToPlayer <= detectionArea)
                {
                    aiState = AIState.Chase;
                }

                break;

            case AIState.Chase:

                if (path == null)
                {
                    return;
                }

                if (currentWaypoint >= path.vectorPath.Count)
                {
                    endOfPath = true;
                    return;
                }
                else
                {
                    endOfPath = false;
                }

                Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
                Vector2 force = direction * speed * Time.deltaTime;

                if (distanceToPlayer <= detectionArea)
                {
                    //move the enemy to target
                    rb.AddForce(force);
                }

                // jarak rigidbody terhadap satu point dalam waypoint 
                float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

                if (distance < nextWaypointDistance)
                {
                    currentWaypoint++;
                }


                if (force.x >= 0.01f)
                {
                    enemyVisual.localScale = new Vector3(-1f, 1f, 1f);
                }
                else if (force.x <= -0.01f)
                {
                    enemyVisual.localScale = new Vector3(1f, 1f, 1f);
                }

                distanceToPlayer = Vector2.Distance(rb.position, target.position);
                if (distanceToPlayer > detectionArea)
                {
                    aiState = AIState.Patrol;
                }
                break;
        }

    }

    private void DirectionChange()
    {
        // animator.SetBool("isRunning", false);

        idleTimer += Time.deltaTime;

        if (idleTimer >= idleDuration)
            movingLeft = !movingLeft;

        movingLeft = !movingLeft;
    }

    private void MoveInDirection(int _direction)
    {
        if (target == null)
        {
            return;
        }
        
        idleTimer = 0;
        // animator.SetBool("isRunning", true);

        // enemy face direction
        enemy.localScale = new Vector3(-Mathf.Abs(initScale.x) * _direction,
        initScale.y, initScale.z);

        //move to the direction
        enemy.position = new Vector3(enemy.position.x + Time.deltaTime * _direction * speedPatrol,
        enemy.position.y, enemy.position.z);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionArea);
    }
}