float alpha_scissor = 0.5;
float opacity = 1;
float outline_width = 0;

bool invert_map = false;
bool enable_outline = false;

float4 inside_color;
float4 outline_color;
float4 outside_color;

sampler2D SDFs : register(s0) {	
	texture = <SDFTEX>;
	MINFILTER = POINT;
	MAGFILTER = POINT;
	MIPFILTER = POINT;
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
};
 
float4 PS(float4 position : SV_Position, float4 color : COLOR0, float2 TexCoords : TEXCOORD0) : COLOR0 {
	float pixel = (tex2D(SDFs, TexCoords).r);	
	float4 rgba = 0;

	if (invert_map) pixel = 1 - pixel;

	//Map uninverted and 
	if (pixel > 1-alpha_scissor && !invert_map) {
		rgba = inside_color;
		
		if (enable_outline && pixel < (1-alpha_scissor) + outline_width)
			rgba = outline_color;

	//Map inverted and inside
	} else if (pixel > alpha_scissor && invert_map) {
		rgba = inside_color;

		if (enable_outline && pixel < alpha_scissor + outline_width)
			rgba = outline_color;
	
	//Outside
	} else rgba = outside_color;

	rgba.a *= opacity;
	return rgba;
}

technique Default {
	pass p0 {
		PixelShader = compile ps_3_0 PS();
	}
}