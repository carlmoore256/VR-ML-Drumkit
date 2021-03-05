using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drum : MonoBehaviour
{
    public AudioClip sample;
    // make drum swell when struck
    public bool swellEffectEnabled;
    public float swellAmount = 0.01f;
    Coroutine swellEffect;
    AudioSource audioSource;


    void Start()
    {
        // audioSource = GetComponent<AudioSource>();
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
    }

    void PlaySound(float velocity)
    {
        audioSource.PlayOneShot(sample, 1f);
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("stick"))
        {
            // float velocity = collision.relativeVelocity.magnitude;
            float velocity = 1f;
            DrumHit(velocity);
        }
    }

    void DrumHit(float velocity)
    {
        PlaySound(velocity);

        if(swellEffectEnabled)
        {
            if(swellEffect != null)
                StopCoroutine(swellEffect);
            swellEffect = StartCoroutine(SwellEffect(velocity));
        }
    }


    IEnumerator SwellEffect(float velocity)
    {
        Vector3 origScale = transform.parent.GetComponent<MoveableProp>().currentScale;

        Vector3 newScale = origScale * (1f + swellAmount);
        transform.parent.localScale = newScale;

        while(transform.parent.localScale.x > origScale.x + 0.001f)
        {
            transform.parent.localScale = Vector3.Lerp(transform.parent.localScale, origScale, Time.deltaTime * 5f);
            yield return new WaitForSeconds(0.01f);
        }

        transform.parent.localScale = origScale;
    }
}
