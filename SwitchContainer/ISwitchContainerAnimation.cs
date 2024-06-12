using System;

namespace TankGame.Client.Common
{
    /// <summary>
    /// An interface for playing animations for <see cref="SwitchContainer"/>
    /// </summary>
    public interface ISwitchContainerAnimation
    {
        void AddCompletedCallback(Action callback);
        bool IsPlaying();
        void Play();
        void Stop();
    }

}