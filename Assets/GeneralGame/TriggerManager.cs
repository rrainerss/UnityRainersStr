    using UnityEngine;

    public class TriggerManager : MonoBehaviour
    {
        public bool isStartZone;

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return; //Must be the player going through

            if (isStartZone) //Start or finish trigger
            {
                GameManager.Instance.StartRace();
            }
            else
            {
                GameManager.Instance.FinishRace();
            }
        }
    }