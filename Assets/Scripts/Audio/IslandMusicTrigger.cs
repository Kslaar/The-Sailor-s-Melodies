using UnityEngine;

public class IslandMusicTrigger : MonoBehaviour
{
    public AK.Wwise.State islandState;
    public AK.Wwise.State explorationState;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            islandState.SetValue();

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            explorationState.SetValue();

        }

    }

}
