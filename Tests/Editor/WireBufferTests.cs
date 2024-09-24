using System;
using Drawbug;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

[TestFixture]
public class WireBufferTests
{
    private WireBuffer _wireBuffer;

    [SetUp]
    public void Setup()
    {
        _wireBuffer = new WireBuffer(10, Allocator.Temp);
    }

    [TearDown]
    public void TearDown()
    {
        if (_wireBuffer.IsCreated)
            _wireBuffer.Dispose();
    }

    [Test]
    public void Constructor_InitialCapacity_ShouldInitializeBuffer()
    {
        Assert.IsTrue(_wireBuffer.IsCreated);
        Assert.AreEqual(0, _wireBuffer.Length);
        Assert.AreEqual(10, _wireBuffer.Capacity);
    }

    [Test]
    public void Constructor_NegativeInitialCapacity_ShouldThrowException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            var wireBuffer = new WireBuffer(-1, Allocator.Temp);
        });
    }

    [Test]
    public void Constructor_InvalidAllocator_ShouldThrowException()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            var wireBuffer = new WireBuffer(10, Allocator.Invalid);
        });
    }

    [Test]
    public void Clear_ShouldResetBuffer()
    {
        _wireBuffer.Submit(new float3(1, 2, 3), new float3(4, 5, 6), 0);
        Assert.AreEqual(2, _wireBuffer.Length);
        
        _wireBuffer.Clear();
        Assert.AreEqual(0, _wireBuffer.Length);
    }

    [Test]
    public void Submit_ValidPoints_ShouldIncreaseLength()
    {
        _wireBuffer.Submit(new float3(1, 2, 3), new float3(4, 5, 6), 0);
        Assert.AreEqual(2, _wireBuffer.Length);
    }

    [Test]
    public void Submit_InvalidAccess_ShouldThrowExceptionWhenDisposed()
    {
        _wireBuffer.Dispose();
        Assert.Throws<ObjectDisposedException>(() => _wireBuffer.Submit(new float3(1, 2, 3), new float3(4, 5, 6), 0));
    }

    [Test]
    public void Submit_OddSizeNativeArray_ShouldThrowException()
    {
        var points = new NativeArray<float3>(1, Allocator.Temp);
        points[0] = new float3(1, 2, 3);
        
        Assert.Throws<ArgumentException>(() =>_wireBuffer.Submit(points, points.Length, 0));
        
        points.Dispose();
    }
    
    [Test]
    public void Submit_NativeArray_ShouldIncreaseLength()
    {
        var points = new NativeArray<float3>(2, Allocator.Temp);
        points[0] = new float3(1, 2, 3);
        points[1] = new float3(4, 5, 6);
        
        _wireBuffer.Submit(points, points.Length, 0);
        Assert.AreEqual(2, _wireBuffer.Length);
        
        points.Dispose();
    }

    [Test]
    public void Submit_AboveCapacity_MustDoubleCapacity()
    {
        _wireBuffer.Submit(new float3(1, 2, 3), new float3(4, 5, 6), 0);
        _wireBuffer.Submit(new float3(7, 8, 9), new float3(10, 11, 12), 0);
        _wireBuffer.Submit(new float3(13, 14, 15), new float3(16, 17, 18), 0);
        _wireBuffer.Submit(new float3(19, 20, 21), new float3(22, 23, 24), 0);
        _wireBuffer.Submit(new float3(25, 26, 27), new float3(28, 29, 30), 0);
        Assert.AreEqual(10, _wireBuffer.Length);
        Assert.AreEqual(10, _wireBuffer.Capacity);
        
        _wireBuffer.Submit(new float3(31, 32, 33), new float3(34, 35, 36), 0);
        Assert.AreEqual(12, _wireBuffer.Length);
        Assert.AreEqual(20, _wireBuffer.Capacity);
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
        
        _wireBuffer.Submit(new float3(1, 2, 3), new float3(4, 5, 6), 0);
        Assert.AreEqual(2, _wireBuffer.Length);
        Assert.AreEqual(10, _wireBuffer.Capacity);
        
        _wireBuffer.Submit(points, points.Length, 0);
        Assert.AreEqual(28, _wireBuffer.Length);
        Assert.AreEqual(28, _wireBuffer.Capacity);
        
        points.Dispose();
    }

    [Test]
    public unsafe void FillBuffer_ShouldNotThrow_WhenCalledWithValidBuffer()
    {
        _wireBuffer.Submit(new float3(1, 2, 3), new float3(4, 5, 6), 0);
        var graphicsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Vertex, 10, sizeof(float3));
        Assert.DoesNotThrow(() => _wireBuffer.FillBuffer(graphicsBuffer));
        graphicsBuffer.Dispose();
    }

    [Test]
    public void ToArray_ShouldReturnCorrectArray()
    {
        _wireBuffer.Submit(new float3(1, 2, 3), new float3(4, 5, 6), 0);
        var array = _wireBuffer.ToArray();
        Assert.AreEqual(2, array.Length);
        Assert.AreEqual(new float3(1, 2, 3), array[0].Position);
        Assert.AreEqual(new float3(4, 5, 6), array[1].Position);
    }
    
    [Test]
    public void Dispose_ShouldFreeResources()
    {
        _wireBuffer.Dispose();
        Assert.IsFalse(_wireBuffer.IsCreated);
    }
}
