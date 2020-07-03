using UnityEngine;

namespace KartGame.KartSystems
{
    /// <summary>
    /// This class produces audio for various states of the vehicle's movement.
    /// </summary>
    public class ArcadeEngineAudio : MonoBehaviour
    {
        [Tooltip("What audio clip should play when the kart starts?")]
        public AudioSource StartSound;
        [Tooltip("What audio clip should play when the kart does nothing?")]
        public AudioSource IdleSound;
        [Tooltip("What audio clip should play when the kart moves around?")]
        public AudioSource RunningSound;

        ArcadeKart arcadeKart;

        void Awake()
        {
            arcadeKart = GetComponentInParent<ArcadeKart>();
        }

        void Update()
        {
            float kartSpeed     = arcadeKart != null ? arcadeKart.LocalSpeed() : 0;
            RunningSound.volume = Mathf.Lerp(.1f, 1, kartSpeed / 2);
            RunningSound.pitch  = Mathf.Lerp(.8f, 1.4f, kartSpeed / 4 + Mathf.Sin(Time.time) * .1f);
            IdleSound.volume    = Mathf.Lerp(.5f, 0, kartSpeed);
        }
    }
}
