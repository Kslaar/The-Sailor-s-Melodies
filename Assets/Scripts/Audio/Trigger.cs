using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Trigger : MonoBehaviour
{
    public GameStateManager GameStateManager;
[SerializeField] private AudioSource targetAudio;
[SerializeField] private float fadeDuration = 2f; // Sekunden


private Coroutine fadeRoutine;


private void OnTriggerEnter(Collider other)
{
if (!other.CompareTag("Player")) return;
StartFadeIn();
}


private void OnTriggerExit(Collider other)
{
if (!other.CompareTag("Player")) return;
StartFadeOut();
}


private void StartFadeIn()
{
if (fadeRoutine != null) StopCoroutine(fadeRoutine);
fadeRoutine = StartCoroutine(FadeAudio(0f, 1f));
}


private void StartFadeOut()
{
if (fadeRoutine != null) StopCoroutine(fadeRoutine);
fadeRoutine = StartCoroutine(FadeAudio(1f, 0f));
}


private IEnumerator FadeAudio(float start, float end)
{
float t = 0f;


if (!targetAudio.isPlaying)
targetAudio.Play();


while (t < fadeDuration)
{
t += Time.deltaTime;
float normalized = t / fadeDuration;
targetAudio.volume = Mathf.Lerp(start, end, normalized);
yield return null;
}


targetAudio.volume = end;


if (end == 0f)
targetAudio.Stop(); // optional
}
public void MuteSound(){
if (fadeRoutine != null) StopCoroutine(fadeRoutine);

targetAudio.volume = 0f;
targetAudio.Stop();


}

}