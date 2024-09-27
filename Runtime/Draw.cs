using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Drawbug
{
    public class Draw : IDisposable
    {
        private static Draw _instance;

        private DrawCommandBuffer _commandBuffer;
        private DrawCommandBuffer _fixedCommandBuffer;
        private RenderData _renderData;
        private readonly WireRender _wireRender;
        private readonly SolidRender _solidRender;

        private DrawCommandBufferTimed _timedCommandBuffer;
        private float _currentDuration = 0;
        
        internal Draw()
        {
            if(_instance != null)
                return;
            
            _instance = this;
            
            _commandBuffer = new DrawCommandBuffer(2048);
            _fixedCommandBuffer = new DrawCommandBuffer(1024);
            _timedCommandBuffer = new DrawCommandBufferTimed(200);
            _currentDuration = 0;
            
            _renderData = new RenderData
            {
                WireBuffer = new WireBuffer(2048),
                SolidBuffer = new SolidBuffer(2048, 6144),
                StyleData = new NativeList<DrawCommandBuffer.StyleData>(1024, Allocator.Persistent),
                //TODO)) Within the job, when you exceed the position or triangle buffer capacity, Submit the data and subtract the amount of data sent for return
                TempFloat3Buffer = new NativeArray<float3>(2048, Allocator.Persistent, NativeArrayOptions.UninitializedMemory),
                TempTriangleBuffer = new NativeArray<int>(10000, Allocator.Persistent, NativeArrayOptions.UninitializedMemory),
            };
            _renderData.CreatePtr();
            _wireRender = new WireRender();
            _solidRender = new SolidRender();
        }

        internal unsafe void BuildData()
        {
            if (_fixedCommandBuffer.HasData)
                _commandBuffer.AnotherBuffer(_fixedCommandBuffer);

            _timedCommandBuffer.SendCommandsToBuffer(ref _commandBuffer);
                
            if (_commandBuffer.HasData)
            {
                _renderData.ProcessCommands(_commandBuffer.GetBuffer());
            }
        }

        internal void GetDataResults()
        {
            if (_commandBuffer.HasData)
            {
                _renderData.GetCommandResults();
                var styleData = _renderData.StyleData.ToArray(Allocator.Temp);
                _wireRender.UpdateBuffer(_renderData.WireBuffer, styleData, _renderData.WireBuffer.Length);
                _solidRender.UpdateBuffer(_renderData.SolidBuffer, styleData, _renderData.SolidBuffer.TrianglesLength);
            }
        }

        internal void Render(UnityEngine.Rendering.CommandBuffer cmd)
        {
            if (_commandBuffer.HasData)
            {
                _wireRender.Render(cmd);
                _solidRender.Render(cmd);
            }
        }
        
        internal void Render(UnityEngine.Rendering.RasterCommandBuffer cmd)
        {
            if (_commandBuffer.HasData)
            {
                _wireRender.Render(cmd);
                _solidRender.Render(cmd);
            }
        }

        public void Dispose()
        {
            _commandBuffer.Dispose();
            _fixedCommandBuffer.Dispose();
            _timedCommandBuffer.Dispose();
            _renderData.Dispose();
            _wireRender.Dispose();
            _solidRender.Dispose();
            _instance = null;
        }
        
        internal void Clear()
        {
            _commandBuffer.Clear();
            _currentDuration = 0;
        }
        
        internal void ClearFixed()
        {
            _fixedCommandBuffer.Clear();
            _currentDuration = 0;
        }
        
        public void UpdateTimedBuffers(float deltaTime)
        {
            _timedCommandBuffer.UpdateTimes(deltaTime);
        }

        private static unsafe DrawCommandBuffer* CurrentCommandBuffer
        {
            get
            {
                DrawbugManager.Initialize();
                var currentDuration = _instance._currentDuration;

                if (currentDuration > 0)
                    return _instance._timedCommandBuffer.GetBuffer(currentDuration);

                fixed (DrawCommandBuffer* fixedBuffer = &_instance._fixedCommandBuffer)
                fixed (DrawCommandBuffer* commandBuffer = &_instance._commandBuffer)
                {
                    return Time.inFixedTimeStep ? fixedBuffer : commandBuffer;
                }
            }
        }

        /// <summary>
        /// Reset all modifications made in current frame.
        /// </summary>
        public static unsafe void Reset()
        {
            CurrentCommandBuffer->ResetStyle();
            CurrentCommandBuffer->DrawMode(DrawMode.Wire);
            CurrentCommandBuffer->Matrix(float4x4.identity);
            _instance._currentDuration = 0;
        }

        /// <summary>
        /// Color to be used to draw the next shapes.<br/>
        /// Is recommended to use <see cref="WithColor"/> instead of this property.<br/><br/>
        /// Otherwise<br/>
        /// You must call <see cref="Reset"/> when you are finished drawing.
        /// <example>
        /// <code>
        /// Draw.Color = Color.red;
        /// Draw.Line(Vector3.zero, Vector3.up);
        /// Draw.Reset();
        /// </code>
        /// </example>
        /// </summary>
        public static unsafe Color Color
        {
            get => CurrentCommandBuffer->PendingStyle.color;
            set => CurrentCommandBuffer->StyleColor(value);
        }
        
        /// <summary>
        /// Whether the next shapes should be displayed in front of any object.<br/>
        /// Is recommended to use <see cref="WithForward"/> instead of this property.<br/><br/>
        /// Otherwise<br/>
        /// You must call <see cref="Reset"/> when you are finished drawing.
        /// <example>
        /// <code>
        /// Draw.Forward = true;
        /// Draw.Line(Vector3.zero, Vector3.up);
        /// Draw.Reset();
        /// </code>
        /// </example>
        /// </summary>
        public static unsafe bool Forward
        {
            get => CurrentCommandBuffer->PendingStyle.forward;
            set => CurrentCommandBuffer->StyleForward(value);
        }
        
        /// <summary>
        /// The way in which the next shapes should be drawn. (Some shapes may not be available in all modes)<br/>
        /// Is recommended to use <see cref="WithDrawMode"/> instead of this property.<br/><br/>
        /// Otherwise<br/>
        /// You must call <see cref="Reset"/> when you are finished drawing.
        /// <example>
        /// <code>
        /// Draw.WithDrawMode(DrawMode.Solid);
        /// Draw.Box(Vector3.zero, Vector3.one);
        /// Draw.Reset();
        /// </code>
        /// </example>
        /// </summary>
        public static unsafe DrawMode DrawMode
        {
            get => CurrentCommandBuffer->CurrentDrawMode;
            set => CurrentCommandBuffer->DrawMode(value);
        }
        
        /// <summary>
        /// The duration that the next shapes will be drawn in seconds.<br/>
        /// Is recommended to use <see cref="WithDuration"/> instead of this property.<br/><br/>
        /// Otherwise<br/>
        /// You must call <see cref="Reset"/> when you are finished drawing.
        /// <example>
        /// <code>
        /// Draw.Duration = 1;
        /// Draw.Line(Vector3.zero, Vector3.up);
        /// Draw.Reset();
        /// </code>
        /// </example>
        /// </summary>
        public static float Duration
        {
            get
            {
                DrawbugManager.Initialize();
                return _instance._currentDuration;
            }
            set
            {
                DrawbugManager.Initialize();
                _instance._currentDuration = value;
            }
        }
        
        /// <summary>
        /// All positions and rotations of the next shapes will be multiplied by this matrix.<br/>
        /// Is recommended to use <see cref="WithMatrix"/> instead of this property.<br/><br/>
        /// Otherwise<br/>
        /// You must call <see cref="Reset"/> when you are finished drawing.
        /// <example>
        /// <code>
        /// Draw.Matrix = transform.localToWorldMatrix;
        /// Draw.Line(Vector3.zero, Vector3.up);
        /// Draw.Reset();
        /// </code>
        /// </example>
        /// <seealso cref="InLocalSpace"/>
        /// </summary>
        public static unsafe float4x4 Matrix
        {
            get => CurrentCommandBuffer->CurrentMatrix;
            set => CurrentCommandBuffer->Matrix(value);
        }
        
        /// <summary>
        /// A safe scope to modify properties and ensure that Reset will be called when exiting the scope.
        /// <code>
        /// using (Draw.WithColor(Color.red))
        /// {
        ///     Draw.Line(Vector3.zero, Vector3.up);
        ///     Draw.Box(Vector3.zero, Vector3.one);
        /// }
        /// </code>
        /// </summary>
        /// <typeparam name="T">The type of the property to modify.</typeparam>
        public readonly struct DrawScope<T> : IDisposable
        {
            private readonly T _before;
            private readonly Action<T> _setter;
            public DrawScope(T current, T newValue, Action<T> setter)
            {
                _before = current;
                _setter = setter;
                _setter.Invoke(newValue);
            }
            
            public void Dispose()
            {
                _setter.Invoke(_before);
            }
        }
        
        /// <summary>
        /// Safe scope to draw multiple shapes with the same color.
        /// <code>
        /// using (Draw.WithColor(Color.red))
        /// {
        ///     Draw.Line(Vector3.zero, Vector3.up);
        ///     Draw.Box(Vector3.zero, Vector3.one);
        /// }
        /// </code>
        /// </summary>
        public static DrawScope<Color> WithColor(Color newValue)
        {
            return new DrawScope<Color>(Color, newValue, previousValue => Color = previousValue);
        }
        
        /// <summary>
        /// Safe scope to draw multiple shapes in front of any object.
        /// <code>
        /// using (Draw.WithForward())
        /// {
        ///     Draw.Line(Vector3.zero, Vector3.up);
        ///     Draw.Box(Vector3.zero, Vector3.one);
        /// }
        /// </code>
        /// </summary>
        public static DrawScope<bool> WithForward(bool newValue = true)
        {
            return new DrawScope<bool>(Forward, newValue, previousValue => Forward = previousValue);
        }
        
        /// <summary>
        /// Safe scope to draw multiple shapes with the same draw mode.
        /// <code>
        /// using (Draw.WithDrawMode(DrawMode.Solid))
        /// {
        ///     Draw.Line(Vector3.zero, Vector3.up);
        ///     Draw.Box(Vector3.zero, Vector3.one);
        /// }
        /// </code>
        /// </summary>
        public static DrawScope<DrawMode> WithDrawMode(DrawMode newValue)
        {
            return new DrawScope<DrawMode>(DrawMode, newValue, previousValue => DrawMode = previousValue);
        }
        
        /// <summary>
        /// Safe scope to draw multiple shapes with the same duration.
        /// <code>
        /// using (Draw.WithDuration(1))
        /// {
        ///     Draw.Line(Vector3.zero, Vector3.up);
        ///     Draw.Box(Vector3.zero, Vector3.one);
        /// }
        /// </code>
        /// </summary>
        public static DrawScope<float> WithDuration(float newValue)
        {
            return new DrawScope<float>(Duration, newValue, previousValue => Duration = previousValue);
        }
        
        /// <summary>
        /// Safe scope to draw multiple shapes with the same matrix.<br/>
        /// All positions and rotations of the next shapes will be multiplied by this matrix.
        /// <code>
        /// using (Draw.WithMatrix(transform.localToWorldMatrix))
        /// {
        ///     Draw.Line(Vector3.zero, Vector3.up);
        ///     Draw.Box(Vector3.zero, Vector3.one);
        /// }
        /// </code>
        /// <seealso cref="InLocalSpace"/>
        /// </summary>
        public static DrawScope<float4x4> WithMatrix(float4x4 newValue)
        {
            return new DrawScope<float4x4>(Matrix, newValue, previousValue => Matrix = previousValue);
        }
        
        /// <summary>
        /// Safe scope to draw multiple shapes in local space.<br/>
        /// All next shapes will be in local space of the given transform.
        /// <code>
        /// using (Draw.InLocalSpace(transform))
        /// {
        ///     Draw.Line(Vector3.zero, Vector3.up);
        ///     Draw.Box(Vector3.zero, Vector3.one);
        /// }
        /// </code>
        ///
        /// is equivalent to
        /// <code>
        /// using (Draw.WithMatrix(transform.localToWorldMatrix))
        /// {
        ///     Draw.Line(Vector3.zero, Vector3.up);
        ///     Draw.Box(Vector3.zero, Vector3.one);
        /// }
        /// </code>
        /// <seealso cref="WithMatrix"/>
        /// </summary>
        public static DrawScope<float4x4> InLocalSpace(Transform transform)
        {
            return new DrawScope<float4x4>(Matrix, transform.localToWorldMatrix, previousValue => Matrix = previousValue);
        }
        
        /// <summary>
        /// Safe scope to draw multiple shapes in position.<br/>
        /// All next shapes will be in the given position
        /// <code>
        /// using (Draw.InPosition(transform.position))
        /// {
        ///     Draw.Line(Vector3.zero, Vector3.up);
        ///     Draw.Box(Vector3.zero, Vector3.one);
        /// }
        /// </code>
        ///
        /// is equivalent to <see cref="InLocalSpace"/> except that only position is taken into account.
        /// </summary>
        public static DrawScope<float4x4> InPosition(float3 position)
        {
            return new DrawScope<float4x4>(Matrix, float4x4.TRS(position, quaternion.identity, new float3(1)), previousValue => Matrix = previousValue);
        }

        /// <summary>
        /// Draw a line between two points.
        /// <code>
        /// Draw.Line(new float3(0, 0, 0), new float3(0, 1, 0));
        /// </code>
        /// </summary>
        public static unsafe void Line(float3 point1, float3 point2)
        {
            CurrentCommandBuffer->Line(point1, point2);
        }
        
        /// <summary>
        /// Draw a line between two points.
        /// <code>
        /// Draw.Line(new Vector3(0, 0, 0), new Vector3(0, 1, 0));
        /// </code>
        /// </summary>
        public static unsafe void Line(Vector3 point1, Vector3 point2)
        {
            CurrentCommandBuffer->Line(point1, point2);
        }

        /// <summary>
        /// Draw multiple lines connected by 2 points.<br/>
        /// The array length must be a multiple of 2.
        /// <code>
        /// Draw.Lines(new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 2, 0), new Vector3(1, 0, 0) });
        /// </code>
        /// <seealso cref="Lines(NativeArray{float3})"/>
        /// </summary>
        public static unsafe void Lines(float3[] lines)
        {
            CurrentCommandBuffer->Lines(lines);
        }
        
        /// <summary>
        /// Draw multiple lines connected by 2 points.<br/>
        /// The array length must be a multiple of 2.
        /// <code>
        /// NativeArray&lt;float3&gt; lines = new NativeArray&lt;float3&gt;(4, Allocator.Temp);
        /// lines[0] = new float3(0, 0, 0);
        /// lines[1] = new float3(0, 1, 0);
        /// lines[2] = new float3(0, 2, 0);
        /// lines[3] = new float3(1, 0, 0);
        ///  
        /// Draw.Lines(lines);
        /// </code>
        /// <seealso cref="Lines(float3[])"/>
        /// </summary>
        public static unsafe void Lines(NativeArray<float3> lines)
        {
            CurrentCommandBuffer->Lines(lines);
        }
        
        /// <summary>
        /// Draw a rectangle
        /// <code>
        /// Draw.Rectangle(new Vector3(0, 0, 0), new Vector3(1, 2), quaternion.identity);
        /// </code>
        /// </summary>
        public static unsafe void Rectangle(float3 position, float2 scale, quaternion rotation)
        {
            CurrentCommandBuffer->Rectangle(position, scale, rotation);
        }
        
        /// <summary>
        /// Draw a rectangle created by two points.
        /// <code>
        /// Draw.Rectangle(new Vector3(0, 0, 0), new Vector3(1, 1, 0), quaternion.identity);
        /// </code>
        /// </summary>
        public static unsafe void Rectangle(float3 point1, float3 point2, quaternion rotation)
        {
            CurrentCommandBuffer->Rectangle(point1, point2, rotation);
        }
        
        /// <summary>
        /// Draw a rectangle created by two points, in 2D space.
        /// <code>
        /// Draw.Rectangle(new Vector3(0, 0), new Vector3(1, 1), quaternion.identity);
        /// </code>
        /// </summary>
        public static unsafe void Rectangle(float2 point1, float2 point2, quaternion rotation)
        {
            CurrentCommandBuffer->Rectangle(new float3(point1, 0), new float3(point2, 0), rotation);
        }
        
        /// <summary>
        /// Draw a rectangle created by two points, in 2D space.
        /// <code>
        /// Draw.Rectangle(new Vector3(0, 0), new Vector3(1, 1));
        /// </code>
        /// </summary>
        public static unsafe void Rectangle(float2 point1, float2 point2)
        {
            CurrentCommandBuffer->Rectangle(new float3(point1, 0), new float3(point2, 0), quaternion.identity);
        }
        
        /// <summary>
        /// Draw a circle.
        /// <code>
        /// Draw.Circle(new Vector3(0, 0, 0), 0.5f, Quaternion.identity);
        /// </code>
        /// </summary>
        public static unsafe void Circle(float3 position, float radius, quaternion rotation)
        {
            CurrentCommandBuffer->Circle(position, radius, rotation);
        }
        
        /// <summary>
        /// Draw a circle.
        /// <code>
        /// Draw.Circle(new Vector3(0, 0, 0), 0.5f);
        /// </code>
        /// </summary>
        public static unsafe void Circle(float3 position, float radius)
        {
            CurrentCommandBuffer->Circle(position, radius, quaternion.identity);
        }
        
        /// <summary>
        /// Draw a circle with a hole at the center.
        /// <code>
        /// Draw.HollowCircle(new Vector3(0, 0, 0), 0.2f, 0.5f, Quaternion.identity);
        /// </code>
        /// </summary>
        public static unsafe void HollowCircle(float3 position, float innerRadius, float outerRadius, quaternion rotation)
        {
            CurrentCommandBuffer->HollowCircle(position, innerRadius, outerRadius, rotation);
        }
        
        /// <summary>
        /// Draw a 2D Capsule.
        /// <code>
        /// Draw.Capsule(new Vector3(0, 0, 0), new Vector2(1, 2), Quaternion.identity, true);
        /// </code>
        /// </summary>
        public static unsafe void Capsule(float3 position, float2 size, quaternion rotation, bool isVertical)
        {
            CurrentCommandBuffer->Capsule(position, size, rotation, isVertical);
        }
        
        /// <summary>
        /// Draw a Cube with a given scale.
        /// <code>
        /// Draw.Box(new Vector3(0, 0, 0), new Vector3(1, 1, 1));
        /// </code>
        /// </summary>
        public static unsafe void Box(float3 position, float scale)
        {
            CurrentCommandBuffer->Box(position, scale, quaternion.identity);
        }
        
        /// <summary>
        /// Draw a Box.
        /// <code>
        /// Draw.Box(new Vector3(0, 0, 0), new Vector3(1, 1, 1));
        /// </code>
        /// </summary>
        public static unsafe void Box(float3 position, float3 scale)
        {
            CurrentCommandBuffer->Box(position, scale, quaternion.identity);
        }

        /// <summary>
        /// Draw a Box.
        /// <code>
        /// Draw.Box(new Vector3(0, 0, 0), new Vector3(1, 1, 1), Quaternion.identity);
        /// </code>
        /// </summary>
        public static unsafe void Box(float3 position, float3 scale, quaternion rotation)
        {
            CurrentCommandBuffer->Box(position, scale, rotation);
        }

        /// <summary>
        /// Draw a Sphere.
        /// <code>
        /// Draw.Sphere(new Vector3(0, 0, 0), 0.5f);
        /// </code>
        /// </summary>
        public static unsafe void Sphere(float3 position, float radius)
        {
            CurrentCommandBuffer->Sphere(position, radius);
        }
        
        /// <summary>
        /// Draw a Cylinder.
        /// <code>
        /// Draw.Cylinder(new Vector3(0, 0, 0), 0.5f, 2f, Quaternion.identity);
        /// </code>
        /// </summary>
        public static unsafe void Cylinder(float3 position, float radius, float height, quaternion rotation)
        {
            CurrentCommandBuffer->Cylinder(position, radius, height, rotation);
        }
        
        /// <summary>
        /// Draw a Capsule 3D.
        /// <code>
        /// Draw.Capsule3D(new Vector3(0, 0, 0), 0.5f, 2f, Quaternion.identity);
        /// </code>
        /// </summary>
        public static unsafe void Capsule3D(float3 position, float radius, float height, quaternion rotation)
        {
            CurrentCommandBuffer->Capsule3D(position, radius, height, rotation);
        }
        
        /// <summary>
        /// Draw a 3D point.<br/>
        /// There are basically 3 lines on each axis crossing.
        /// <code>
        /// Draw.Point(new Vector3(0, 0, 0));
        /// </code>
        /// </summary>
        public static unsafe void Point(float3 position, float size = 0.4f)
        {
            CurrentCommandBuffer->Point(position, size);
        }
        
        /// <summary>
        /// Draw a 2D point.<br/>
        /// There are basically 2 lines on each axis crossing.
        /// <code>
        /// Draw.Point(new Vector2(0, 0));
        /// </code>
        /// </summary>
        public static unsafe void Point(float2 position, float size = 0.4f)
        {
            CurrentCommandBuffer->Point(position, size);
        }
    }
}