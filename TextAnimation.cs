using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// Changes the <see cref="TMP_Text"/> in intrvals.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public sealed class TextAnimation : MonoBehaviour
    {
        [Tooltip("Delay per change")]
        public int delay;

        [Tooltip("Texts to cycle through")]
        public string[] texts;

        private TMP_Text _text;
        private CancellationTokenSource _cts;

        private void Awake() => _text = GetComponent<TMP_Text>();

        private void OnEnable() => StartLoop((_cts = new()).Token);

        private void OnDisable() => _cts?.Cancel();

        private async void StartLoop(CancellationToken ctsToken)
        {
            int index = 0;
            while (!ctsToken.IsCancellationRequested)
            {
                _text.text = texts[index];
                index = (index + 1) % texts.Length;
                await Task.Delay(delay, ctsToken);
            }
        }
    }
}