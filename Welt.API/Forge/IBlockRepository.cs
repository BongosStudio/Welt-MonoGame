﻿using System;

namespace Welt.API.Forge
{
    /// <summary>
    /// Providers block providers for a server.
    /// </summary>
    public interface IBlockRepository
    {
        /// <summary>
        /// Gets this repository's block provider for the specified block ID. This may return null
        /// if the block ID in question has no corresponding block provider.
        /// </summary>
        IBlockProvider GetBlockProvider(ushort id);
        /// <summary>
        /// Registers a new block provider. This overrides any existing block providers that use the
        /// same block ID.
        /// </summary>
        void RegisterBlockProvider(IBlockProvider provider);
    }
}