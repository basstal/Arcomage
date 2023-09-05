// using NOAH.Core;
using UnityEngine;

namespace NOAH.UI
{
    public class SceneUI : MonoBehaviour // DelegatedBehaviour
    {
        [SerializeField] private Vector2 m_instructionOffset = Vector2.zero;

        public Vector2 InstructionOffset => m_instructionOffset;
    }
}
