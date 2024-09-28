#define VS_SHADERMODEL vs_5_0
#define PS_SHADERMODEL ps_5_0

float4x4 WorldViewProjection;
float3 Color;

struct VertexShaderInput
{
    float4 Position : POSITION;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float3 Color : TEXCOORD0;
};
struct PSO
{
    float4 color : COLOR0;
    float4 normal : COLOR1;
    float4 position : COLOR2;
    float4 bloomFilter : COLOR3;
};

VertexShaderOutput MainVertexShader(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;
    
    // Project position
    output.Position = mul(input.Position, WorldViewProjection);

    
    return output;
}


PSO MainPixelShader(VertexShaderOutput input) : COLOR
{
    PSO output = (PSO) 0;
    
    output.color = float4(Color, 0);            //disable lighting
    output.bloomFilter = float4(0, 0, 0, 1);    //dont add to filter
    
    return output;
}

PSO BackgroundPixelShader(VertexShaderOutput input) : COLOR
{
    PSO output = (PSO) 0;
    
    output.color = float4(Color * 0.5, 0);      //disable lighting
    output.bloomFilter = float4(0, 0, 0, 1);    //dont add to filter
    
    return output;
}


technique Gizmos
{
    pass Background
    {
        VertexShader = compile VS_SHADERMODEL MainVertexShader();
        PixelShader = compile PS_SHADERMODEL BackgroundPixelShader();
    }
    pass Foreground
    {
        VertexShader = compile VS_SHADERMODEL MainVertexShader();
        PixelShader = compile PS_SHADERMODEL MainPixelShader();
    }
};
