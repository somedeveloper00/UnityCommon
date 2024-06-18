using AnimFlex.Sequencer;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;

namespace UnityCommon
{
    /// <summary>
    /// A container that can be switched ON and OFF, like how a game object can be activated and deactivated. This container 
    /// helps with efficiently handling an animation for turning objects ON and OFF.
    /// </summary>
    public sealed partial class SwitchContainer : MonoBehaviour
    {
        public bool SkipAnimations;

        [Tooltip("If not empty, it'll be activated/deactivated after timelines are finished or on activation/deactivation setter immediately")]
        [SerializeField] private GameObject targetGameObject;

        [Tooltip("If not empty, it'll be enabled/disabled after timelines are finished or on activation/deactivation setter immediately")]
        [SerializeField] private Canvas targetCanvas;

        [Tooltip("If not empty, it'll be played when the state of this container switches from deactivated to activated")]
        [SerializeField] private PlayableDirector inTimeline;

        [Tooltip("If not empty, it'll be played when the state of this container switches from activated to deactivated")]
        [SerializeField] private PlayableDirector outTimeline;

        [Tooltip("If not empty, it'll be played when the state of this container switches from deactivated to activated")]
        [FormerlySerializedAs("inSequencer")]
        [SerializeField] private SequenceAnim inSequence;

        [Tooltip("If not empty, it'll be played when the state of this container switches from activated to deactivated")]
        [FormerlySerializedAs("outSequencer")]
        [SerializeField] private SequenceAnim outSequence;

        private ISwitchContainerAnimation _inAnim, _outAnim;
        private bool _wasActivated;

        /// <summary>
        /// Whether this <see cref="SwitchContainer"/> is ON. Optimized to be called every frame.
        /// </summary>
        public bool Activated
        {
            get => _wasActivated;
            set
            {
                if (_wasActivated == value)
                {
                    // dont proceed if animation is on-going
                    if ((_wasActivated || (_outAnim?.IsPlaying()) != true) && (!_wasActivated || (_inAnim?.IsPlaying()) != true))
                    {
                        EnforceActivationResultState(value);
                    }
                }
                else if (value)
                {
                    if (_outAnim?.IsPlaying() == true)
                    {
                        _outAnim.Stop();
                    }
                    EnforceActivationResultState(true);
                    if (!SkipAnimations && _inAnim is not null)
                    {
                        _inAnim.Play();
                    }
                }
                else
                {
                    if (_inAnim?.IsPlaying() == true)
                    {
                        _inAnim.Stop();
                    }
                    if (!SkipAnimations && _outAnim is not null)
                    {
                        _outAnim.Play();
                    }
                    else
                    {
                        EnforceActivationResultState(false);
                    }
                }

                _wasActivated = value;
            }
        }

        private void EnforceActivationResultState(bool value)
        {
            if (targetCanvas)
            {
                targetCanvas.enabled = value;
            }
            else if (targetGameObject)
            {
                targetGameObject.SetActive(value);
            }
        }

        private void Awake()
        {
            if (inTimeline)
            {
                _inAnim = new SwitchContainerAnimation_Timeline(inTimeline);
            }
            else if (inSequence)
            {
                _inAnim = new SwitchContainerAnimation_SequenceAnim(inSequence);
            }
            if (outTimeline)
            {
                _outAnim = new SwitchContainerAnimation_Timeline(outTimeline);
            }
            else if (outSequence)
            {
                _outAnim = new SwitchContainerAnimation_SequenceAnim(outSequence);
            }

            _outAnim?.AddCompletedCallback(() => EnforceActivationResultState(false));
        }

        private void Reset()
        {
            targetCanvas = GetComponent<Canvas>();
            targetGameObject = gameObject;
        }
    }
}