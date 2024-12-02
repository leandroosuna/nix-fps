#define VS_SHADERMODEL vs_5_0
#define PS_SHADERMODEL ps_5_0

texture Texture;
float time;
float2 playerPos;

float rotation; 
sampler TextureSampler = sampler_state
{
    texture = (Texture);
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

struct VSI
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};
struct VSO
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
};

technique Skybox
{
    pass Pass1
    {
        VertexShader = compile VS_SHADERMODEL VS();
        PixelShader = compile PS_SHADERMODEL PS();
    }
}

VSO VS(VSI input)
{
    VSO output = (VSO) 0;

    output.Position = input.Position;
    output.TexCoord = input.TexCoord;

    return output;
}

float2 RotateTexCoord(float2 texCoord, float2 center, float angle)
{
    float2 translated = texCoord - center;

    float cosAngle = cos(angle);
    float sinAngle = sin(angle);

    float2 rotated;
    rotated.x = translated.x * cosAngle - translated.y * sinAngle;
    rotated.y = translated.x * sinAngle + translated.y * cosAngle;

    return rotated + center;
}

#define MAX_PLAYERS 64 // Maximum number of players

float2 playerPositions[MAX_PLAYERS]; // Array of player positions
int numPlayers; // Number of active players

float2 localPlayerPos;

float PIOver2; 

float4 PS(VSO input) : COLOR
{
    float2 center = float2(0.5, 0.5); // Assuming the texture's center is at (0.5, 0.5)
    float2 rotatedTexCoord = RotateTexCoord(input.TexCoord, center, rotation);
    
    float4 sampledColor = tex2D(TextureSampler, rotatedTexCoord);
    float3 color = sampledColor.rgb;

    if (color.r < 0.01 && color.g < 0.01 && color.b < 0.01)
        discard;

    float distanceToPlayer = length(localPlayerPos - RotateTexCoord(input.TexCoord, center, PIOver2 + rotation));
    if (distanceToPlayer <= 0.015) 
    {
        return float4(1, 1, 1, 1);
    }
    
    for (int i = 0; i < numPlayers; i++)
    {
        float distanceToPlayer = length(playerPositions[i] - RotateTexCoord(input.TexCoord, center, PIOver2 + rotation));
        if (distanceToPlayer <= 0.020) 
        {
            return lerp(float4(color, 1), float4(1, 0, 0, 1), clamp((sin(time) - .25) * 4, 0, 1));
        }
    }

    return float4(color, 1);
}
