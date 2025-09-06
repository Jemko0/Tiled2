#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix WorldViewProjection; //set by cpu

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TextureCoordinate : TEXCOORD0;
    
    float2 InstancePos : TEXCOORD1;
    float4 InstanceData : TEXCOORD2;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TextureCoordinate : TEXCOORD0;
    float4 InstanceData : TEXCOORD1;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output;
    
    //transform quad by inst offset
    
    float4 worldPos = input.Position;
    worldPos.xy += input.InstancePos;
    
    output.Position = mul(worldPos, WorldViewProjection);
    output.TextureCoordinate = input.TextureCoordinate;
    output.InstanceData = input.InstanceData;
    
    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    //use UV + instance data
    float2 uv = input.TextureCoordinate;
    float3 light = input.InstanceData.xyz;
    
    float r = light.r / 32.0f;
    float g = light.g / 32.0f;
    float b = light.b / 32.0f;
    
    float4 color = float4(r, g, b, 1.0f);
    
    return color;
}

technique BasicColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};