// ---------------------------------------------------------
// Ejemplo shader Minimo:
// ---------------------------------------------------------

/**************************************************************************************/
/* Variables comunes */
/**************************************************************************************/

//Matrices de transformacion
float4x4 matWorld; //Matriz de transformacion World
float4x4 matWorldView; //Matriz World * View
float4x4 matWorldViewProj; //Matriz World * View * Projection
float4x4 matInverseTransposeWorld; //Matriz Transpose(Invert(World))

//Textura para DiffuseMap
texture texDiffuseMap;
sampler2D diffuseMap = sampler_state
{
	Texture = (texDiffuseMap);
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

float time = 0;

/**************************************************************************************/
/* RenderScene */
/**************************************************************************************/

//Input del Vertex Shader
struct VS_INPUT
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
	float2 Texcoord : TEXCOORD0;
};

//Output del Vertex Shader
struct VS_OUTPUT
{
	float4 Position :        POSITION0;
	float2 Texcoord :        TEXCOORD0;
	float4 Color :			COLOR0;
	float3 Pos :			TEXCOORD1;
};

//Vertex Shader
VS_OUTPUT vs_main(VS_INPUT Input)
{
	VS_OUTPUT Output;
    Output.Position = mul(Input.Position, matWorldViewProj);
    Output.Pos = Input.Position;
    Output.Color = Input.Color;
    Output.Texcoord = Input.Texcoord;
	return(Output);
}

//Pixel Shader
float4 gameover(float2 Texcoord: TEXCOORD0, float3 Pos : TEXCOORD1) : COLOR0
{
	float4 coordenada = float4(tex2D(diffuseMap, Texcoord));
	coordenada.r -= 4*time + cos(Pos.xyz) - 2;
	coordenada.g -= 4*time + cos(Pos.xyz) - 2;
	coordenada.b -= 4*time + cos(Pos.xyz) - 2;
	return coordenada;
}

// ------------------------------------------------------------------
technique RenderScene
{
	pass Pass_0
	{
		VertexShader = compile vs_2_0 vs_main();
		PixelShader = compile ps_2_0 gameover();
	}
}