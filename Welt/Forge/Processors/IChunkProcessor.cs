﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Welt.Core.Forge;
using Welt.Forge.Builders;

namespace Welt.Forge.Processors
{
    /// <summary>
    ///     Base interface that dictates how and when a chunk is processed. 
    /// </summary>
    public interface IChunkProcessor
    {
        /// <summary>
        ///     Gets the current status of the processor.
        /// </summary>
        ProcessorStatus Status { get; }

        // TODO: perhaps have it grab a pre-existing ChunkBuilder object and adjust those buffers?
        /// <summary>
        ///     Creates a <see cref="Task{ChunkBuilder}"/> object from the given <paramref name="chunk"/>.
        /// </summary>
        /// <param name="chunk"></param>
        /// <returns></returns>
        Task<ChunkBuilder> ProcessChunk(Chunk chunk);
    }
}
