using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;

// Generates a texture by writing to a render texture using GL
// 
// References:
// https://docs.unity3d.com/ScriptReference/GL.html
// https://forum.unity.com/threads/rendering-gl-to-a-texture2d-immediately-in-unity4.158918/
public static class MapTextureGenerator
{
    private static Material drawingMaterial;

    public static Texture2D generate(MapGraph map, Config conf)
    {
        CreateDrawingMaterial();
        var texture = RenderGLToTexture(map, conf.textureSize, conf.meshSize, drawingMaterial);

        return texture;
    }

    private static void CreateDrawingMaterial()
    {
        if (!drawingMaterial)
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            drawingMaterial = new Material(shader);
            drawingMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            drawingMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            drawingMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            drawingMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            drawingMaterial.SetInt("_ZWrite", 0);
        }
    }

    private static Texture2D RenderGLToTexture(MapGraph map, int textureSize, int meshSize, Material material)
    {
        // get a temporary RenderTexture
        RenderTexture renderTexture = RenderTexture.GetTemporary(textureSize, textureSize);

        // set the RenderTexture as global target (that means GL too)
        RenderTexture.active = renderTexture;

        // clear GL
        GL.Clear(false, true, Color.white);
        GL.sRGBWrite = false;

        // render GL immediately to the active render texture
        RenderObjectTexture(map, material, textureSize, meshSize);

        // read the active RenderTexture into a new Texture2D
        Texture2D newTexture = new Texture2D(textureSize, textureSize);
        newTexture.ReadPixels(new Rect(0, 0, textureSize, textureSize), 0, 0);

        // apply pixels and compress
        bool applyMipsmaps = false;
        newTexture.Apply(applyMipsmaps);
        bool highQuality = true;
        newTexture.Compress(highQuality);

        // clean up after the party
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(renderTexture);

        // return the goods
        return newTexture;
    }

    private static void RenderObjectTexture(MapGraph map, Material material, int textureSize, int meshSize)
    {
        material.SetPass(0);
        GL.PushMatrix();
        GL.LoadPixelMatrix(0, meshSize, 0, meshSize);
        GL.Viewport(new Rect(0, 0, textureSize, textureSize));

        GL.Begin(GL.TRIANGLES);
        foreach (var node in map.nodes)
        {
            var color = BiomeColors.colors.ContainsKey(node.nodeType) ? BiomeColors.colors[node.nodeType] : Color.red;
            GL.Color(color);

            foreach (var edge in node.GetEdges())
            {
                var start = edge.previous.destination;
                var end = edge.destination;
                GL.Vertex3(node.centerPoint.x, node.centerPoint.z, 0);
                GL.Vertex3(start.x, start.z, 0);
                GL.Vertex3(end.x, end.z, 0);
            }
        }
        GL.End();

        GL.PopMatrix();
    }
}
