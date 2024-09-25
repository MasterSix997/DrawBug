using System;
using Drawbug.PhysicsExtension;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

[TestFixture]
public class SolidBufferTests
{
    private SolidBuffer _solidBuffer;

    [SetUp]
    public void SetUp()
    {
        _solidBuffer = new SolidBuffer(10, 10, Allocator.Temp);
    }

    [TearDown]
    public void TearDown()
    {
        if (_solidBuffer.IsCreated)
            _solidBuffer.Dispose();
    }

    [Test]
    public void Constructor_ValidParameters_CreatesBuffer()
    {
        Assert.IsTrue(_solidBuffer.IsCreated);
        Assert.AreEqual(10, _solidBuffer.VerticesCapacity);
        Assert.AreEqual(10, _solidBuffer.TrianglesCapacity);
    }

    [Test]
    public void Constructor_InvalidAllocator_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new SolidBuffer(10, 10, Allocator.Invalid));
    }

    [Test]
    public void Constructor_NegativeInitialVerticesCapacity_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SolidBuffer(-1, 10));
    }

    [Test]
    public void Constructor_NegativeInitialTrianglesCapacity_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SolidBuffer(10, -1));
    }

    [Test]
    public void Clear_ShouldResetBuffer()
    {
        _solidBuffer.Submit(new float3(1, 1, 1), 0);
        Assert.AreEqual(1, _solidBuffer.VerticesLength);
        _solidBuffer.Clear();
        Assert.AreEqual(0, _solidBuffer.VerticesLength);
    }

    [Test]
    public void Submit_ValidPoint_IncreasesVerticesLength()
    {
        _solidBuffer.Submit(new float3(1, 1, 1), 0);
        Assert.AreEqual(1, _solidBuffer.VerticesLength);
    }
    
    [Test]
    public void Submit_AboveCapacity_MustDoubleCapacity()
    {
        _solidBuffer.Submit(new float3(1, 1, 1), 0);
        _solidBuffer.Submit(new float3(2, 2, 2), 0);
        _solidBuffer.Submit(new float3(3, 3, 3), 0);
        _solidBuffer.Submit(new float3(4, 4, 4), 0);
        _solidBuffer.Submit(new float3(5, 5, 5), 0);
        _solidBuffer.Submit(new float3(6, 6, 6), 0);
        _solidBuffer.Submit(new float3(7, 7, 7), 0);
        _solidBuffer.Submit(new float3(8, 8, 8), 0);
        _solidBuffer.Submit(new float3(9, 9, 9), 0);
        _solidBuffer.Submit(new float3(10, 10, 10), 0);
        Assert.AreEqual(10, _solidBuffer.VerticesLength);
        Assert.AreEqual(10, _solidBuffer.VerticesCapacity);
        
        _solidBuffer.Submit(new float3(11, 11, 11), 0);
        _solidBuffer.Submit(new float3(12, 12, 12), 0);
        Assert.AreEqual(12, _solidBuffer.VerticesLength);
        Assert.AreEqual(20, _solidBuffer.VerticesCapacity);
    }
    
    [Test]
    public void Submit_LargeNativeArray_CapacityMustAddArraySize()
    {
        var points = new NativeArray<float3>(26, Allocator.Temp);
        points[0] = new float3(1, 2, 3);
        points[1] = new float3(4, 5, 6);
        points[2] = new float3(7, 8, 9);
        points[3] = new float3(10, 11, 12);
        points[4] = new float3(13, 14, 15);
        points[5] = new float3(16, 17, 18);
        points[6] = new float3(19, 20, 21);
        points[7] = new float3(22, 23, 24);
        points[8] = new float3(25, 26, 27);
        points[9] = new float3(28, 29, 30);
        points[10] = new float3(31, 32, 33);
        points[11] = new float3(34, 35, 36);
        points[12] = new float3(37, 38, 39);
        points[13] = new float3(40, 41, 42);
        points[14] = new float3(43, 44, 45);
        points[15] = new float3(46, 47, 48);
        points[16] = new float3(49, 50, 51);
        points[17] = new float3(52, 53, 54);
        points[18] = new float3(55, 56, 57);
        points[19] = new float3(58, 59, 60);
        points[20] = new float3(61, 62, 63);
        points[21] = new float3(64, 65, 66);
        points[22] = new float3(67, 68, 69);
        points[23] = new float3(70, 71, 72);
        points[24] = new float3(73, 74, 75);
        points[25] = new float3(76, 77, 78);
        
        _solidBuffer.Submit(new float3(1, 1, 1), 0);
        _solidBuffer.Submit(new float3(2, 2, 2), 0);
        Assert.AreEqual(2, _solidBuffer.VerticesLength);
        Assert.AreEqual(10, _solidBuffer.VerticesCapacity);
        
        _solidBuffer.Submit(points, points.Length, 0);
        Assert.AreEqual(28, _solidBuffer.VerticesLength);
        Assert.AreEqual(28, _solidBuffer.VerticesCapacity);
        
        points.Dispose();
    }
    
    [Test]
    public void Submit_InvalidAccess_ShouldThrowExceptionWhenDisposed()
    {
        _solidBuffer.Dispose();
        Assert.Throws<ObjectDisposedException>(() => _solidBuffer.Submit(new float3(1, 1, 1), 0));
    }

    [Test]
    public unsafe void FillBuffer_ShouldNotThrow_WhenCalledWithValidBuffer()
    {
        _solidBuffer.Submit(new float3(1, 1, 1), 0);
        var graphicsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Vertex, 10, sizeof(float3));
        Assert.DoesNotThrow(() => _solidBuffer.FillVerticesBuffer(graphicsBuffer));
        graphicsBuffer.Dispose();
    }
    
    [Test]
    public void Dispose_BufferIsDisposed()
    {
        _solidBuffer.Dispose();
        Assert.IsFalse(_solidBuffer.IsCreated);
    }
}