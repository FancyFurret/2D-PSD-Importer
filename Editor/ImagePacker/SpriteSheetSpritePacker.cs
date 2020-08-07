using System;
using System.Linq;
using UnityEngine;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor.U2D.Common;

namespace UnityEditor.U2D.PSD
{
    public static class SpriteSheetSpritePacker
    {
        /// <summary>
        /// Packs image buffer into a single buffer. Image buffers are assumed to be 4 bytes per pixel in RGBA format
        /// </summary>
        /// <param name="buffers">Image buffers to pack</param>
        /// <param name="width">Image buffers width</param>
        /// <param name="height">Image buffers height</param>
        /// <param name="padding">Padding between each packed image</param>
        /// <param name="outPackedBuffer">Packed image buffer</param>
        /// <param name="outPackedBufferWidth">Packed image buffer's width</param>
        /// <param name="outPackedBufferHeight">Packed iamge buffer's height</param>
        /// <param name="outPackedRect">Location of each image buffers in the packed buffer</param>
        /// <param name="outUVTransform">Translation data from image original buffer to packed buffer</param>
        public static void Pack(NativeArray<Color32>[] buffers, int width, int height, int padding, out NativeArray<Color32> outPackedBuffer, out int outPackedBufferWidth, out int outPackedBufferHeight, out RectInt[] outPackedRect, out Vector2Int[] outUVTransform)
        {
            UnityEngine.Profiling.Profiler.BeginSample("Pack");
            // Determine the area that contains data in the buffer
            outPackedBuffer = default(NativeArray<Color32>);
            try
            {
                var tightRects = buffers.Select(b => new RectInt(0, 0, width, height)).ToArray();
                ImagePacker.Pack(tightRects, padding, out outPackedRect, out outPackedBufferWidth, out outPackedBufferHeight);
                outUVTransform = new Vector2Int[tightRects.Length];
                for (int i = 0; i < outUVTransform.Length; ++i)
                {
                    outUVTransform[i] = new Vector2Int(outPackedRect[i].x - tightRects[i].x, outPackedRect[i].y - tightRects[i].y);
                }
                outPackedBuffer = new NativeArray<Color32>(outPackedBufferWidth * outPackedBufferHeight, Allocator.Temp);

                ImagePacker.Blit(outPackedBuffer, outPackedRect, outPackedBufferWidth, buffers, tightRects, width, padding);
            }
            catch (Exception ex)
            {
                if (outPackedBuffer.IsCreated)
                    outPackedBuffer.Dispose();
                throw ex;
            }
            finally
            {
                UnityEngine.Profiling.Profiler.EndSample();
            }
        }
    }
}