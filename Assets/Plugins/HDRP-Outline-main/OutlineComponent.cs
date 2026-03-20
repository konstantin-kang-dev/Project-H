using System;
using UnityEngine;

namespace Modules.Rendering.Outline
{
    public class OutlineComponent : MonoBehaviour
    {
        LayerMask _initialLayer;
        LayerMask _outlineLayer;

        private void Awake()
        {
            _initialLayer = gameObject.layer;
            _outlineLayer = LayerMask.NameToLayer("Outline");
        }

        private void OnEnable()
        {
            SetOutline(true);
        }

        private void OnDisable()
        {
            SetOutline(false);
        }

        public void SetOutline(bool value)
        {
            gameObject.layer = value ? _outlineLayer : _initialLayer;
        }
    }
}