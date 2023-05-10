﻿using AutomaticTypeMapper;
using CommunityToolkit.HighPerformance;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace EOLib.Graphics
{
    [MappedType(BaseType = typeof(INativeGraphicsManager), IsSingleton = true)]
    public sealed class NativeGraphicsManager : INativeGraphicsManager
    {
        private readonly ConcurrentDictionary<GFXTypes, ConcurrentDictionary<int, Texture2D>> _cache;

        private readonly INativeGraphicsLoader _gfxLoader;
        private readonly IGraphicsDeviceProvider _graphicsDeviceProvider;

        public NativeGraphicsManager(INativeGraphicsLoader gfxLoader, IGraphicsDeviceProvider graphicsDeviceProvider)
        {
            _cache = new ConcurrentDictionary<GFXTypes, ConcurrentDictionary<int, Texture2D>>();
            _gfxLoader = gfxLoader;
            _graphicsDeviceProvider = graphicsDeviceProvider;
        }

        public Texture2D TextureFromResource(GFXTypes file, int resourceVal, bool transparent = false, bool reloadFromFile = false)
        {
            if (_cache.ContainsKey(file) && _cache[file].ContainsKey(resourceVal))
            {
                if (reloadFromFile)
                {
                    _cache[file][resourceVal]?.Dispose();
                    _cache[file].Remove(resourceVal, out _);
                }
                else
                {
                    return _cache[file][resourceVal];
                }
            }

            var ret = LoadTexture(file, resourceVal, transparent);
            if (_cache.ContainsKey(file) ||
                _cache.TryAdd(file, new ConcurrentDictionary<int, Texture2D>()))
            {
                _cache[file].TryAdd(resourceVal, ret);
            }

            return ret;
        }

        private Texture2D LoadTexture(GFXTypes file, int resourceVal, bool transparent)
        {
            var rawData = _gfxLoader.LoadGFX(file, resourceVal);

            if (rawData.IsEmpty)
                return new Texture2D(_graphicsDeviceProvider.GraphicsDevice, 1, 1);

            Action<byte[]> processAction = null;

            if (transparent)
            {
                // for all gfx: 0x000000 is transparent
                processAction = data => CrossPlatformMakeTransparent(data, Color.Black);

                // for hats: 0x080000 is transparent
                if (file == GFXTypes.FemaleHat || file == GFXTypes.MaleHat)
                {
                    processAction = data => CrossPlatformMakeTransparent(data,
                        // TODO: 0x000000 is supposed to clip hair below it
                        new Color(0xff000000),
                        new Color(0xff080000),
                        new Color(0xff000800),
                        new Color(0xff000008));
                }
            }

            using var ms = rawData.AsStream();
            var ret = Texture2D.FromStream(_graphicsDeviceProvider.GraphicsDevice, ms, processAction);

            return ret;
        }

        private static unsafe void CrossPlatformMakeTransparent(byte[] data, params Color[] transparentColors)
        {
            fixed (byte* ptr = data)
            {
                for (int i = 0; i < data.Length; i += 4)
                {
                    uint* addr = (uint*)(ptr + i);
                    if (transparentColors.Contains(new Color(*addr)))
                        *addr = 0;
                }
            }
        }

        public void Dispose()
        {
            foreach (var text in _cache.SelectMany(x => x.Value.Values))
                text.Dispose();

            _cache.Clear();
        }
    }

    [Serializable]
    public class GFXLoadException : Exception
    {
        public GFXLoadException(int resource, GFXTypes gfx)
            : base($"Unable to load graphic {resource + 100} from file gfx{(int) gfx:000}.egf") { }
    }
}
