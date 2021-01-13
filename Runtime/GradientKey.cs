using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Toorah.Gradients
{
    [System.Serializable]
    public class GradientKey
    {
        public Color color;
        [Range(0, 1)]
        public float position;
        public GradientKey(Color col, float pos)
        {
            color = col;
            position = pos;
        }
    }
}
