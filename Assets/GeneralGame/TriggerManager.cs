    using UnityEngine;

    public class TriggerManager : MonoBehaviour
    {
        public bool isStartZone;

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            if (isStartZone)
            {
                GameManager.Instance.StartRace();
            }
            else
            {
                GameManager.Instance.FinishRace();
            }
        }
    }