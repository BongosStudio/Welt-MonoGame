﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Welt.Graphics;

namespace Welt.Forge.Renderers
{
    public class Mesh<TVertexType> : IDisposable where TVertexType : struct, IVertexType
    {
        /// <summary>
        ///     The total amount of vertices rendered for this type of mesh.
        /// </summary>
        public static int VerticiesRendered { get; set; }
        /// <summary>
        ///     The total amount of indices rendered for this type of mesh.
        /// </summary>
        public static int IndiciesRendered { get; set; }

        /// <summary>
        ///     Resets the rendered counts to 0.
        /// </summary>
        public static void ResetStats()
        {
            VerticiesRendered = 0;
            IndiciesRendered = 0;
        }

        /// <summary>
        /// The maximum number of submeshes stored in a single mesh.
        /// </summary>
        public const int SubmeshLimit = 16;

        // Used for synchronous access to the graphics device.
        private static readonly object _syncLock =
            new object();

        private WeltGame m_Game;
        private GraphicsDevice m_GraphicsDevice;
        private int m_Submeshes = 0;
        private bool m_IsReady = false;
        protected VertexBuffer _vertices; // ChunkMesh uses these but external classes shouldn't, so I've made them protected.
        protected IndexBuffer[] _indices;

        private bool m_RecalculateBounds; // Whether this mesh should recalculate its bounding box when changed.

        /// <summary>
        /// Gets or sets the vertices in this mesh.
        /// </summary>
        public TVertexType[] Vertices
        {
            set
            {
                if (value.Length == 0) return;
                if (_vertices != null)
                    _vertices.Dispose();

                _vertices = new VertexBuffer(m_GraphicsDevice, typeof(TVertexType),
                        value.Length, BufferUsage.WriteOnly);
                _vertices.SetData(value);
                m_IsReady = true;

                //if (_recalculateBounds)
                //    BoundingBox = RecalculateBounds(value);
            }
        }

        public bool IsReady => m_IsReady;

        public int Submeshes => m_Submeshes;

        /// <summary>
        /// Gets the bounding box for this mesh.
        /// </summary>
        public BoundingBox BoundingBox { get; protected set; }

        /// <summary>
        /// Gets whether this mesh is disposed of.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Creates a new mesh.
        /// </summary>
        public Mesh(WeltGame game, int submeshes = 1, bool recalculateBounds = true)
        {
            if ((submeshes < 0) || (submeshes >= SubmeshLimit))
                throw new ArgumentOutOfRangeException();

            m_Game = game;
            m_GraphicsDevice = game.GraphicsDevice;
            _indices = new IndexBuffer[submeshes];
            m_RecalculateBounds = recalculateBounds;
        }

        /// <summary>
        /// Creates a new mesh.
        /// </summary>
        public Mesh(WeltGame game, TVertexType[] vertices,
                int submeshes = 1, bool recalculateBounds = true) : this(game, submeshes, recalculateBounds)
        {
            Vertices = vertices;
        }

        /// <summary>
        /// Creates a new mesh.
        /// </summary>
        public Mesh(WeltGame game, TVertexType[] vertices,
                short[] indices, bool recalculateBounds = true) : this(game, 1, recalculateBounds)
        {
            Vertices = vertices;
            SetSubmesh(0, indices);
        }

        /// <summary>
        /// Sets a submesh in this mesh.
        /// </summary>
        public void SetSubmesh(int index, short[] indices)
        {
            if ((index < 0) || (index > _indices.Length))
                throw new ArgumentOutOfRangeException();

            lock (_syncLock)
            {
                if (_indices[index] != null)
                    _indices[index].Dispose();

                _indices[index] = new IndexBuffer(m_GraphicsDevice, IndexElementSize.SixteenBits,
                        (indices.Length + 1), BufferUsage.WriteOnly);
                _indices[index].SetData(indices);
                if (index + 1 > m_Submeshes)
                    m_Submeshes = index + 1;
            }
        }

        /// <summary>
        /// Draws this mesh using the specified effect.
        /// </summary>
        /// <param name="effect">The effect to use.</param>
        public void Draw(Effect effect)
        {
            if (effect == null)
                throw new ArgumentException();

            for (int i = 0; i < _indices.Length; i++)
                Draw(effect, i);
        }

        /// <summary>
        /// Draws a submesh in this mesh using the specified effect.
        /// </summary>
        /// <param name="effect">The effect to use.</param>
        /// <param name="index">The submesh index.</param>
        public void Draw(Effect effect, int index)
        {
            if (effect == null)
                throw new ArgumentException();

            if ((index < 0) || (index > _indices.Length))
                throw new ArgumentOutOfRangeException();
            if (_vertices == null)
                throw new NullReferenceException("Vertices is null");
            if (_indices[index] == null)
                throw new NullReferenceException("Specified indices are null");
            if (_vertices.IsDisposed || _indices[index].IsDisposed || _indices[index].IndexCount < 3)
                return; // Invalid state for rendering, just return.

            effect.GraphicsDevice.SetVertexBuffer(_vertices);
            effect.GraphicsDevice.Indices = _indices[index];
            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                effect.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _vertices.VertexCount);
            }
            VerticiesRendered += _vertices.VertexCount;
            IndiciesRendered += _indices[index].IndexCount;
        }

        /// <summary>
        /// Returns the total vertices in this mesh.
        /// </summary>
        public int GetTotalVertices()
        {
            if (_vertices == null)
                return 0;

            lock (_syncLock)
                return _vertices.VertexCount;
        }

        /// <summary>
        /// Returns the total indices for all the submeshes in this mesh.
        /// </summary>
        public int GetTotalIndices()
        {
            lock (_syncLock)
            {
                int sum = 0;
                foreach (var element in _indices)
                    sum += (element != null) ? element.IndexCount : 0;
                return sum;
            }
        }

        /// <summary>
        /// Recalculates the bounding box for this mesh.
        /// </summary>
        /// <param name="vertices">The vertices in this mesh.</param>
        /// <returns></returns>
        protected virtual BoundingBox RecalculateBounds(TVertexType[] vertices)
        {
            return new BoundingBox();
        }

        /// <summary>
        /// Disposes of this mesh.
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed)
                return;

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of this mesh.
        /// </summary>
        /// <param name="disposing">Whether Dispose() called the method.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_GraphicsDevice = null; // Lose the reference to our graphics device.

                if (_vertices != null)
                    _vertices.Dispose();
                foreach (var element in _indices)
                {
                    if (element != null)
                        element.Dispose();
                }
            }
        }

        /// <summary>
        /// Finalizes this mesh.
        /// </summary>
        ~Mesh()
        {
            Dispose(false);
        }
    }
}
