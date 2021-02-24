using System;
using System.Collections.Generic;
using UnityEngine;

namespace Toorah.Gradients
{
    [CreateAssetMenu(fileName = "New Gradient Asset", menuName = "Gradient/Create Gradient")]
    public class GradientAsset : ScriptableObject
    {
        public List<GradientKey> keys = new List<GradientKey>();
		public Mode mode = Mode.Blend;

		public enum Mode
        {
			Blend,
			Linear
        }

        private void OnEnable()
        {
			if (keys.Count == 0)
            {
				keys.Add(new GradientKey(Color.white, 0));
				keys.Add(new GradientKey(Color.white, 1));
            }
		}

        public Color Evaluate(float position)
        {
			Color col = Color.clear;

			if (keys.Count == 1)
			{
				return keys[0].color;
			}
			else
			{
                switch (mode)
                {
                    case Mode.Blend:
						for (int i = 0; i < keys.Count - 1; i++)
						{
							Color c1 = keys[i].color;
							float p1 = keys[i].position;

							Color c2 = keys[i + 1].color;
							float p2 = keys[i + 1].position;

							if (i == 0 && position < p1)
							{
								col = keys[0].color;
							}
							else if (i + 1 == keys.Count - 1 && position >= p2)
							{
								col = keys[keys.Count - 1].color;
							}

							if (position >= p1 && position < p2)
							{
								col = Color.Lerp(c1, c2, (position - p1) / (p2 - p1));
							}
						}
						break;
                    case Mode.Linear:
						for (int i = 0; i < keys.Count - 1; i++)
						{
							Color c1 = keys[i].color;
							float p1 = keys[i].position;

							Color c2 = keys[i + 1].color;
							float p2 = keys[i + 1].position;

							if (i == 0 && position < p1)
							{
								col = keys[0].color;
							}
							else if (i + 1 == keys.Count - 1 && position >= p2)
							{
								col = keys[keys.Count - 1].color;
							}

							if (position >= p1 && position < p2)
							{
								col = c1;
							}
						}
						break;
                

                
				}

			}


			return col;
		}
    }
}