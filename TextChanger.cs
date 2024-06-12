using System;
using TMPro;
using UnityEngine;

namespace TankGame.Client.Common
{
    /// <summary>
    /// Changes the text of a <see cref="TMP_Text"/>. Useful for use in animations and timelines.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public sealed class TextChanger : MonoBehaviour
    {
        [TextArea, SerializeField] private string[] texts;

        public void SetText(int index)
        {
            if (index < 0 || index >= texts.Length)
            {
                throw new IndexOutOfRangeException(nameof(index));
            }

            if (TryGetComponent<TMP_Text>(out var txt))
            {
                txt.text = texts[index];
            }
        }
    }
}