


float3 Additive(float3 color0, float3 color1) {
	return color0 + color1;
}

float3 Subtract(float3 color0, float3 color1) {
	return color0 - color1;
}

float3 Screen(float3 color0, float3 color1) {
	return (float3)1 - (((float3)1 - color0) * ((float3)1 - color1));
}

float4 Alpha(float4 color0, float4 color1) {

	// http://neareal.com/2428/

	/*float alpha = color0.a + color1.a * (1 - color0.a);
	float3 rgb = (color0.rgb * color0.a + color1.rgb * color1.a * (1 - color0.a)) / alpha;

	if (alpha == 0) rgb = (float3)0;*/

	float3 rgb = color1.rgb * (1 - color0.a) + color0.rgb * color0.a;
	float alpha = color0.a + color1.a;

	return float4(rgb, alpha);

}