#define VS_SHADERMODEL vs_5_0
#define PS_SHADERMODEL ps_5_0

#include "lightUtil.fxh"

float4x4 world;
float4x4 view;
float4x4 projection;
float4x4 inverseTransposeWorld;

float3 color;
float filter;
struct VertexShaderInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL0; 
    float2 TexCoord: TEXCOORD;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
    float4 Normal : TEXCOORD1;
    float4 WorldPos : TEXCOORD2;
};
struct PSO
{
    float4 color : COLOR0;
    float4 normal : COLOR1;
    float4 position : COLOR2;
    float4 bloomFilter : COLOR3;
};

texture colorTexture;
sampler2D colorSampler = sampler_state
{
    Texture = (colorTexture);
    AddressU = WRAP;
    AddressV = WRAP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

texture normalMap;
sampler2D normalSampler = sampler_state
{
    Texture = (normalMap);
    AddressU = WRAP;
    AddressV = WRAP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};


float2 tiling;

VertexShaderOutput ColorVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    float4 worldPosition = mul(input.Position, world);
    float4 viewPosition = mul(worldPosition, view);
    float4 screenPos = mul(viewPosition, projection);
    output.WorldPos = worldPosition;
    output.Position = screenPos;
    output.Normal = mul(float4(input.Normal, 1), inverseTransposeWorld);
    output.TexCoord = input.TexCoord * tiling;
    return output;
}
PSO ColorPS(VertexShaderOutput input)
{
    PSO output;
    float3 n = normalize(input.Normal.xyz);
  
    float3 normal = (n + 1.0) * 0.5;

    output.color = float4(color, KD);
    output.normal = float4(normal, KS);
    output.position = float4(input.WorldPos.xyz, shininess);
    output.bloomFilter = float4(0, 0, 0, 1);
    return output;
}
PSO NumPS(VertexShaderOutput input)
{
    PSO output;
    float3 n = normalize(input.Normal.xyz);
  
    float3 normal = (n + 1.0) * 0.5;
    
    float3 texColor = tex2D(colorSampler, input.TexCoord).rgb;
    texColor += color * .25f;
    
    if (distance(color, float3(1, 1, 1)) < .1f)
        output.color = float4(texColor, KD);
    else
        output.color = float4(texColor, 0);
    
    output.normal = float4(normal, KS);
    output.position = float4(input.WorldPos.xyz, shininess);
    output.bloomFilter = float4(0, 0, 0, 1);
    return output;
}
PSO LightDisPS(VertexShaderOutput input)
{
    PSO output;
    float3 n = normalize(input.Normal.xyz);
  
    float3 normal = (n + 1.0) * 0.5;
        
    output.color = float4(color, 0);
    output.normal = float4(0, 0, 0, 1); //rgb=0 light dis
    output.position = float4(input.WorldPos.xyz, 0);
    output.bloomFilter = float4(color, 1);
    return output;
}
PSO TexPS(VertexShaderOutput input)
{
    PSO output;
    float3 n = normalize(input.Normal.xyz);
  
    float3 normal = (n + 1.0) * 0.5;

    float3 texColor = tex2D(colorSampler, input.TexCoord).rgb;
    
    output.color = float4(texColor, KD);
    output.normal = float4(normal, KS);
    output.position = float4(input.WorldPos.xyz, shininess);
    output.bloomFilter = float4(0,0,0, 1);
    return output;
}
float3 getNormalFromMap(float2 textureCoordinates, float3 worldPosition, float3 worldNormal)
{
    float3 tangentNormal = tex2D(normalSampler, textureCoordinates).xyz * 2.0 - 1.0;

    float3 Q1 = ddx(worldPosition);
    float3 Q2 = ddy(worldPosition);
    float2 st1 = ddx(textureCoordinates);
    float2 st2 = ddy(textureCoordinates);

    worldNormal = normalize(worldNormal.xyz);
    float3 T = normalize(Q1 * st2.y - Q2 * st1.y);
    float3 B = -normalize(cross(worldNormal, T));
    float3x3 TBN = float3x3(T, B, worldNormal);

    return normalize(mul(tangentNormal, TBN));
}

PSO TexNormalPS(VertexShaderOutput input)
{
    PSO output;
    //float3 n = normalize(input.Normal.xyz);
    float3 n = getNormalFromMap(input.TexCoord, input.WorldPos.xyz, normalize(input.Normal.xyz));
    float3 normal = (n + 1.0) * 0.5;

    float2 dx = ddx(input.TexCoord.xy);
    float2 dy = ddy(input.TexCoord.xy);
    // rotated grid uv offsets
    float2 uvOffsets = float2(0.125, 0.375);
    float4 offsetUV = float4(0.0, 0.0, 0.0, -1);
    // supersampled using 2x2 rotated grid
    half4 col = 0;
    offsetUV.xy = input.TexCoord.xy + uvOffsets.x * dx + uvOffsets.y * dy;
    col += tex2Dbias(colorSampler, offsetUV);
    offsetUV.xy = input.TexCoord.xy - uvOffsets.x * dx - uvOffsets.y * dy;
    col += tex2Dbias(colorSampler, offsetUV);
    offsetUV.xy = input.TexCoord.xy + uvOffsets.y * dx - uvOffsets.x * dy;
    col += tex2Dbias(colorSampler, offsetUV);
    offsetUV.xy = input.TexCoord.xy - uvOffsets.y * dx + uvOffsets.x * dy;
    col += tex2Dbias(colorSampler, offsetUV);
    col *= 0.25;
    float3 texColor = col.rgb;
    
    //float3 texColor = tex2D(colorSampler, input.TexCoord).rgb;
    
    output.color = float4(texColor, KD);
    output.normal = float4(normal, KS);
    output.position = float4(input.WorldPos.xyz, shininess);
    output.bloomFilter = float4(0, 0, 0, 1);
    return output;
}


technique basic_color
{
    pass P0
    {
        AlphaBlendEnable = FALSE;
        VertexShader = compile VS_SHADERMODEL ColorVS();
        PixelShader = compile PS_SHADERMODEL ColorPS();
    }
}
technique number
{
    pass P0
    {
        AlphaBlendEnable = FALSE;
        VertexShader = compile VS_SHADERMODEL ColorVS();
        PixelShader = compile PS_SHADERMODEL NumPS();
    }
}

technique color_lightDis
{
    pass P0
    {
        //FillMode = WIREFRAME;
        
        AlphaBlendEnable = FALSE;
        VertexShader = compile VS_SHADERMODEL ColorVS();
        PixelShader = compile PS_SHADERMODEL LightDisPS();
    }
};


technique colorTex_lightEn
{
    pass P0
    {
        AlphaBlendEnable = FALSE;
        VertexShader = compile VS_SHADERMODEL ColorVS();
        PixelShader = compile PS_SHADERMODEL TexPS();
    }
};

technique colorTexNormal_lightEn
{
    pass P0
    {
        AlphaBlendEnable = FALSE;
        VertexShader = compile VS_SHADERMODEL ColorVS();
        PixelShader = compile PS_SHADERMODEL TexNormalPS();
    }
};
