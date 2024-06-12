using System;
using AnimFlex.Sequencer;
using UnityEngine;
using UnityEngine.Playables;

namespace UnityCommon
{
    /// <summary>
    /// A container that can be switched ON and OFF, like how a game object can be activated and deactivated. This container 
    /// helps with efficiently handling an animation for turning objects ON and OFF.
    /// </summary>
    public sealed class SwitchContainer : MonoBehaviour
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
        [SerializeField] private SequenceAnim inSequence;

        [Tooltip("If not empty, it'll be played when the state of this container switches from activated to deactivated")]
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
                    if (!_wasActivated && _outAnim?.IsPlaying() == true)
                    {
                        return;
                    }
                    if (_wasActivated && _inAnim?.IsPlaying() == true)
                    {
                        return;
                    }

                    EnforceActivationResultState(value);
                    return;
                }

                if (value)
                {
                    if (_outAnim?.IsPlaying() == true)
                    {
                        _outAnim.Stop();
                    }
                    EnforceActivationResultState(true);
                    if (!SkipAnimations)
                    {
                        _inAnim?.Play();
                    }
                }
                else
                {
                    if (_inAnim?.IsPlaying() == true)
                    {
                        _inAnim.Stop();
                    }
                    if (!SkipAnimations)
                    {
                        _outAnim?.Play();
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


        public readonly struct SwitchContainerAnimation_Timeline : ISwitchContainerAnimation
        {
            public readonly PlayableDirector playableDirector;

            public SwitchContainerAnimation_Timeline(PlayableDirector playableDirector) => this.playableDirector = playableDirector;
            public bool IsPlaying() => playableDirector.state == PlayState.Playing;
            public void Play() => playableDirector.Play();
            public void Stop() => playableDirector.Stop();

            public void AddCompletedCallback(Action callback)
            {
                var director = playableDirector;
                director.paused += _ =>
                {
                    if (director.time == director.duration)
                    {
                        callback();
                    }
                };
            }
        }

        public readonly struct SwitchContainerAnimation_SequenceAnim : ISwitchContainerAnimation
        {
            public readonly SequenceAnim sequenceAnim;

            public SwitchContainerAnimation_SequenceAnim(SequenceAnim sequenceAnim) => this.sequenceAnim = sequenceAnim;
            public bool IsPlaying() => sequenceAnim.IsPlaying();
            public void Play() => sequenceAnim.PlaySequence();
            public void Stop() => sequenceAnim.StopSequence();

            public void AddCompletedCallback(Action callback)
            {
                var seq = sequenceAnim;
                seq.sequence.onComplete += callback.Invoke;
            }
        }
    }
}