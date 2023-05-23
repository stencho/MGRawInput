float2 offset;

float2 billboard_size;
float2 draw_offset = float2(0,0);
float2 draw_scale = float2(1,1);

float alpha_scissor = 0.5;

float opacity = 1;
float outline_width = 0;

bool invert_map = false;

bool enable_outline = false;

float4 inside_color;
float4 outside_color;
float4 outline_color; 

sampler2D SDFs : register(s0) {	
	texture = <SDFTEX>;
	MINFILTER = POINT;
	MAGFILTER = POINT;
	MIPFILTER = POINT;
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
};


float4x4 World;
float4x4 View;
float4x4 Projection;


struct VertexShaderInput
{
	float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION;
    float2 TexCoord : TEXCOORD0;
};
struct PSO
{
    float4 Color : COLOR0;
	float Depth : SV_Depth;
};


//vestigial vs
VertexShaderOutput VS(VertexShaderInput input) {
	VertexShaderOutput output = (VertexShaderOutput)0;

	float4x4 wvp = mul(World, mul(View, Projection));
	output.Position = mul(input.Position, wvp);
    output.TexCoord = input.TexCoord;

	return output;
}

//Pixel Shader
PSO PS(float4 position : SV_Position, float4 color : COLOR0, float2 TexCoords : TEXCOORD0)
{
	PSO output = (PSO)0;

	float a = (tex2D(SDFs, TexCoords).r);	

	if (invert_map)
		a = 1-a;		

	float4 rgba = 0;

	if (a > 1-alpha_scissor) {
		rgba = inside_color;
		
		if (enable_outline && a <  ( alpha_scissor * outline_width) ) rgba = outline_color;

	} else rgba = outside_color;

	output.Color = rgba * float4(1,1,1, opacity);
	output.Depth = 0;

	return output;
}


technique Default
{
	pass p0
	{
		PixelShader = compile ps_3_0 PS();
	}
}

technique Full
{
	pass p0
	{
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_3_0 PS();
	}
}