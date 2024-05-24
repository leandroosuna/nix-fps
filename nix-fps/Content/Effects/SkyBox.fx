#define VS_SHADERMODEL vs_5_0
#define PS_SHADERMODEL ps_5_0

float4x4 world;
float4x4 view;
float4x4 projection;

float3 cameraPosition;

texture skyBoxTexture;
samplerCUBE skyBoxSampler = sampler_state
{
    texture = (skyBoxTexture);
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = Mirror;
    AddressV = Mirror;
};

texture skyBox2DTexture;
sampler skySphereSampler = sampler_state
{
    texture = (skyBox2DTexture);
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = mirror;
    AddressV = mirror;
};


struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TextureCoordinate : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float3 TextureCoordinate : TEXCOORD0;
};
struct SphereVSO
{
    float4 Position : POSITION0;
    float2 TextureCoordinate : TEXCOORD0;
};

struct PSO
{
    float4 color : COLOR0;
    float4 normal : COLOR1;
    float4 position : COLOR2;
    float4 bloomFilter : COLOR3;
};
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, world);
    float4 viewPosition = mul(worldPosition, view);
    output.Position = mul(viewPosition, projection);

    float4 vertexPosition = mul(input.Position, world);
    output.TextureCoordinate = vertexPosition.xyz - cameraPosition;

    return output;
}

PSO PixelShaderFunction(VertexShaderOutput input)
{
    PSO output;
    output.color = float4(texCUBE(skyBoxSampler, normalize(input.TextureCoordinate)).rgb, 0);
    
    //output.color = float4(1, 0, 0, 1);
    output.normal = float4(1, 1, 1, 1);
    output.position = float4(1, 1, 1, 1);
    output.bloomFilter = float4(0, 0, 0, 1);
    return output;
    
    //return float4(1, 0, 1, 1);
}



PSO SpherePS(VertexShaderOutput input)
{
    PSO output;
    output.color = float4(tex2D(skySphereSampler, normalize(input.TextureCoordinate)).rgb, 0);
    
    //output.color = float4(1, 0, 0, 1);
    output.normal = float4(1, 1, 1, 1);
    output.position = float4(1, 1, 1, 1);
    output.bloomFilter = float4(0, 0, 0, 1);
    return output;
    
    //return float4(1, 0, 1, 1);
}
technique Skybox
{
    pass Pass1
    {
        AlphaBlendEnable = FALSE;
        VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}

technique sphere2d
{
    pass Pass1
    {
        AlphaBlendEnable = FALSE;
        VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
        PixelShader = compile PS_SHADERMODEL SpherePS();
    }
}
