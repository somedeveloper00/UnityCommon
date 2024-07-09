using System;
using AnimFlex.Sequencer;

namespace UnityCommon
{
    public sealed partial class SwitchContainer
    {
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