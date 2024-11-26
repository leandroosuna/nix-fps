#define VS_SHADERMODEL vs_5_0
#define PS_SHADERMODEL ps_5_0

matrix World;
matrix WorldViewProjection;

bool isDeferred;
float3 CameraPosition;

struct PSOMRT
{
    float4 color : COLOR0;
    float4 normal : COLOR1;
    float4 position : COLOR2;
    float4 bloomFilter : COLOR3;
};


texture SkyBoxTexture;
samplerCUBE SkyBoxSampler = sampler_state
{
    texture = (SkyBoxTexture);
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = Mirror;
    AddressV = Mirror;
};

struct VSIsky
{
    float4 Position : POSITION0;
};
struct VSOsky
{
    float4 Position : SV_POSITION;
    float3 TextureCoordinates : TEXCOORD0;
};
technique Skybox
{
    pass Pass1
    {
        AlphaBlendEnable = FALSE;
        VertexShader = compile VS_SHADERMODEL SkyboxVS();
        PixelShader = compile PS_SHADERMODEL SkyboxPS();
    }
}
VSOsky SkyboxVS(VSIsky input)
{
    VSOsky output = (VSOsky) 0;

    output.Position = mul(input.Position, WorldViewProjection);
    float4 VertexPosition = mul(input.Position, World);
    output.TextureCoordinates = VertexPosition.xyz - CameraPosition;

    return output;
}
PSOMRT SkyboxPS(VSOsky input)
{
    PSOMRT output = (PSOMRT) 0;
    float3 col = texCUBE(SkyBoxSampler, normalize(input.TextureCoordinates)).rgb;
    output.color = float4(col, !isDeferred);
    output.normal = float4(0, 0, 0, 1);
    output.position= float4(0, 0, 0, 0);
    output.bloomFilter = float4(0,0,0, 1);
    
    return output;
}
