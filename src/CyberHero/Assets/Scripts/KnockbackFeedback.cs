using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class KnockbackFeedback : MonoBehaviour
{
   [SerializeField] private Rigidbody2D rb;
   [SerializeField] float strength = 16, delay = .15f;

   public UnityEvent OnBegin, OnDone;

    public void PlayFeedback(GameObject sender){
        StopAllCoroutines();
        OnBegin.Invoke();
        Vector2 direction = (transform.position - sender.transform.position).normalized;
        rb.AddForce(direction*strength,ForceMode2D.Impulse);
        StartCoroutine(Reset());
    }

   private IEnumerator Reset() {
        yield return new WaitForSeconds(delay);
        rb.velocity = Vector3.zero;
        OnDone.Invoke();
   }
}
