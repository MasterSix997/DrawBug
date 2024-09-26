using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Drawbug
{
    [BurstCompile]
    internal unsafe partial struct ProcessCommandsJob : IJob
    {
        //Input
        [NativeDisableUnsafePtrRestriction] public UnsafeAppendBuffer* Buffer;
        
        //Output
        public WireBuffer WireBuffer;
        public SolidBuffer SolidBuffer;
        public NativeList<DrawCommandBuffer.StyleData> StyleData;

        private bool _firstStyle;
        private uint _currentStyleId;
        
        private DrawMode _currentDrawMode;
        private Matrix4x4 _currentMatrix;
        
        [NativeDisableUnsafePtrRestriction] public float3* temp3;
        [NativeDisableUnsafePtrRestriction] public int* tempT;

        private void AddStyle(DrawCommandBuffer.StyleData styleData)
        {
            if (!_firstStyle)
                _currentStyleId++;
            _firstStyle = false;
            
            StyleData.Add(styleData);
        }


        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void Next(ref UnsafeAppendBuffer.Reader reader)
        {
            var command = reader.ReadNext<DrawCommandBuffer.Command>();
            switch (command)
            {
                case DrawCommandBuffer.Command.Style:
                    AddStyle(reader.ReadNext<DrawCommandBuffer.StyleData>());
                    break;
                case DrawCommandBuffer.Command.DrawMode:
                    _currentDrawMode = reader.ReadNext<DrawMode>();
                    break;
                case DrawCommandBuffer.Command.Matrix:
                    _currentMatrix = reader.ReadNext<float4x4>();
                    WireBuffer.Matrix = _currentMatrix;
                    SolidBuffer.Matrix = _currentMatrix;
                    break;
                case DrawCommandBuffer.Command.Line:
                    AddLine(reader.ReadNext<DrawCommandBuffer.LineData>());
                    break;
                case DrawCommandBuffer.Command.Lines:
                    var arrayLength = reader.ReadNext<int>();
                    var sizeOfArray = UnsafeUtility.SizeOf<float3>() * arrayLength;
                    
                    var linesPtr = (float3*)(reader.Ptr + reader.Offset);
                    AddLines(linesPtr, arrayLength);
                    reader.Offset += sizeOfArray;
                    break;
                case DrawCommandBuffer.Command.Rectangle:
                    var rectData = reader.ReadNext<DrawCommandBuffer.RectangleData>();
                    if (_currentDrawMode is DrawMode.Wire or DrawMode.Both)
                        AddRectangle(rectData);
                    if (_currentDrawMode is DrawMode.Solid or DrawMode.Both)
                        AddSolidRectangle(rectData);
                    break;
                case DrawCommandBuffer.Command.Circle:
                    var circleData = reader.ReadNext<DrawCommandBuffer.CircleData>();
                    if (_currentDrawMode is DrawMode.Wire or DrawMode.Both)
                        AddCircle(circleData);
                    if (_currentDrawMode is DrawMode.Solid or DrawMode.Both)
                        AddSolidCircle(circleData);
                    break;
                case DrawCommandBuffer.Command.HollowCircle:
                    var hollowCircleData = reader.ReadNext<DrawCommandBuffer.HollowCircleData>();
                    if (_currentDrawMode is DrawMode.Wire or DrawMode.Both)
                        AddHollowCircle(hollowCircleData);
                    if (_currentDrawMode is DrawMode.Solid or DrawMode.Both)
                        AddSolidHollowCircle(hollowCircleData);
                    break;
                case DrawCommandBuffer.Command.Capsule:
                    var capsuleData = reader.ReadNext<DrawCommandBuffer.CapsuleData>();
                    if (_currentDrawMode is DrawMode.Wire or DrawMode.Both)
                        AddCapsule(capsuleData);
                    if (_currentDrawMode is DrawMode.Solid or DrawMode.Both)
                        AddSolidCapsule(capsuleData);
                    break;
                case DrawCommandBuffer.Command.Box:
                    var boxData = reader.ReadNext<DrawCommandBuffer.BoxData>();
                    if (_currentDrawMode is DrawMode.Wire or DrawMode.Both)
                        AddBox(boxData);
                    if (_currentDrawMode is DrawMode.Solid or DrawMode.Both)
                        AddSolidBox(boxData);
                    break;
                case DrawCommandBuffer.Command.Sphere:
                    var sphereData = reader.ReadNext<DrawCommandBuffer.SphereData>();
                    if (_currentDrawMode is DrawMode.Wire or DrawMode.Both)
                        AddSphere(sphereData);
                    if (_currentDrawMode is DrawMode.Solid or DrawMode.Both)
                        AddSolidSphere(sphereData);
                    break;
                case DrawCommandBuffer.Command.Cylinder:
                    var cylinderData = reader.ReadNext<DrawCommandBuffer.CylinderData>();
                    if (_currentDrawMode is DrawMode.Wire or DrawMode.Both)
                        AddCylinder(cylinderData);
                    if (_currentDrawMode is DrawMode.Solid or DrawMode.Both)
                        AddSolidCylinder(cylinderData);
                    break;
                case DrawCommandBuffer.Command.Capsule3D:
                    var capsule3DData = reader.ReadNext<DrawCommandBuffer.CylinderData>();
                    if (_currentDrawMode is DrawMode.Wire or DrawMode.Both)
                        AddCapsule3D(capsule3DData);
                    if (_currentDrawMode is DrawMode.Solid or DrawMode.Both)
                        AddSolidCapsule3D(capsule3DData);
                    break;
                // case DrawCommandBuffer.Command.:
                //     var  = reader.ReadNext<DrawCommandBuffer.>();
                //     if (_currentDrawMode is DrawMode.Wire or DrawMode.Both)
                //         Add();
                //     break;
                default:
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                    throw new Exception("Unknown command");
#else
				    break;
#endif
            }
        }
        
        public void Execute()
        {
            WireBuffer.Clear();
            SolidBuffer.Clear();
            StyleData.Clear();
            _firstStyle = true;
            _currentDrawMode = DrawMode.Wire;
            _currentMatrix = Matrix4x4.identity;
            
            var reader = Buffer->AsReader();
            while (reader.Offset < reader.Size)
            {
                Next(ref reader);
            }

            if (reader.Offset != reader.Size)
            {
                throw new Exception("Didn't reach the end of the buffer");
            }
        }
    }
}