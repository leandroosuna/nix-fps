﻿#define VS_SHADERMODEL vs_5_0
#define PS_SHADERMODEL ps_5_0

#define MAX_BONES 240

#include "lightUtil.fxh"

float4x4 world;
float4x4 view;
float4x4 projection;
float4x4 inverseTransposeWorld;

float4x4 Bones[MAX_BONES];

struct VSI
{
    float4 Position : POSITION;
    float3 Normal : NORMAL;
    float2 TexCoord : TEXCOORD0;
    uint4 Indices : BLENDINDICES;
    float4 Weights : BLENDWEIGHT;
};

struct VSO
{
	float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
    float4 Normal : TEXCOORD1;
    float3 WorldPos : TEXCOORD2;
};

void Skin(inout VSI vin, uniform int boneCount)
{
    float4x3 skinning = 0;

    [unroll]
    for (int i = 0; i < boneCount; i++)
    {
        skinning += Bones[vin.Indices[i]] * vin.Weights[i];
    }

    vin.Position.xyz = mul(vin.Position, skinning);
    vin.Normal = mul(vin.Normal, (float3x3) skinning);
}


VSO MainVS(VSI input)
{
	VSO output = (VSO)0;
    
    Skin(input, 4);
    
    float4 worldPosition = mul(input.Position, world);
    float4 viewPosition = mul(worldPosition, view);	
    float4 screenPos = mul(viewPosition, projection);
    
    output.WorldPos = worldPosition.xyz;
    output.TexCoord = input.TexCoord;
    output.Position = screenPos;
    output.Normal = mul(float4(input.Normal, 1), inverseTransposeWorld);
    
    return output;
}

texture colorTexture;
sampler colorSampler = sampler_state
{
    Texture = (colorTexture);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

float4 MainPS(VSO input) : COLOR
{
    return float4(1, 0, 1, 1);

}

struct PSO
{
    float4 color : COLOR0;
    float4 normal : COLOR1;
    float4 position : COLOR2;
};

PSO MRTPS(VSO input)
{
    float3 color = tex2D(colorSampler, input.TexCoord).rgb;
    float3 normal = normalize(input.Normal.xyz);

    float3 normalColor = 0.5f * (normal + 1.0f);
    
    PSO output;

    output.color = float4(color, KD); //alpha: KD
    output.normal = float4(normalColor, KS); //alpha KS
    output.position = float4(input.WorldPos, shininess); //shininess 20/60
    
    return output;
}


technique BasicSkin
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};

technique SkinMRT
{
    pass P0
    {
        AlphaBlendEnable = FALSE;
        
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MRTPS();
    }
};
