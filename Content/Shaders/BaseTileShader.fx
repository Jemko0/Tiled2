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
    float tileType = input.InstanceData.x;
    
    float3 color = float4(1.0, 1.0, 1.0, 1.0);

    if(tileType == 0)
    {
        color = float4(0.2, 0.2, 0.8, 0);
        //discard;
    }
    
    
    if(tileType == 1) //grass
    {
        color = float4(0, 1, 0, 0);
    }
    
    if (tileType == 2) //dirt
    {
        color = float4(0.45, 0.33, 0.25, 1.0);
    }
    
    if (tileType == 3) //stone
    {
        color = float4(0.2, 0.2, 0.2, 1.0);
    }
    
    return float4(color, 1.0);
}

technique BasicColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};