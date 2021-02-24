float4 Gradient(float2 uv, float4 Colors[100], float Positions[100], float count)
{
	float4 col = 0;

	if (count == 1)
	{
		return Colors[0];
	}
	else
	{
		for (int i = 0; i < count - 1; i++)
		{
			float4 c1 = Colors[i];
			float p1 = Positions[i];

			float4 c2 = Colors[i + 1];
			float p2 = Positions[i + 1];

			if (i == 0 && uv.x < p1) {
				col = Colors[0];
			}
			else if (i + 1 == count - 1 && uv.x >= p2) {
				col = Colors[count - 1];
			}

			if (uv.x >= p1 && uv.x < p2)
			{
				col = lerp(c1, c2, (uv.x - p1) / (p2 - p1));
			}
		}

	}


	return col;
}

float4 GradientBars(float2 uv, float4 Colors[100], float Positions[100], float count)
{
	float4 col = 0;

	if (count == 1)
	{
		return Colors[0];
	}
	else
	{
		for (int i = 0; i < count - 1; i++)
		{
			float4 c1 = Colors[i];
			float p1 = Positions[i];

			float4 c2 = Colors[i + 1];
			float p2 = Positions[i + 1];

			if (i == 0 && uv.x < p1) {
				col = Colors[0];
			}
			else if (i + 1 == count - 1 && uv.x >= p2) {
				col = Colors[count - 1];
			}

			if (uv.x >= p1 && uv.x < p2)
			{
				col = c1;
			}
		}

	}


	return col;
}