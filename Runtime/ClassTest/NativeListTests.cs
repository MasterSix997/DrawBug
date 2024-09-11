using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Drawbug.ClassTest
{
    public class NativeListTests : MonoBehaviour
    {
        public int startBufferLength = 1024;
        public bool fillBufferWithArray = true;
        public int bufferWritePerSeconds = 100;
        [Min(-1)] public int copyBufferCount = -1;
        
        private NativeList<float3> _wireBuffer;

        private void Start()
        {
            _wireBuffer = new NativeList<float3>(startBufferLength, Allocator.Persistent);
        }

        private void Update()
        {
            _wireBuffer.Clear();
            WriteToBuffer(copyBufferCount, _wireBuffer);
            Debug.Log("=====================");
            Debug.Log("Buffer data:");
            Debug.Log("Length: " +  _wireBuffer.Length);
            Debug.Log("Capacity: " +  _wireBuffer.Capacity);
        }

        private void OnDisable()
        {
            _wireBuffer.Dispose();
        }

        private void WriteToBuffer(int currentCopy, NativeList<float3> bufferCopy)
        {
            if (currentCopy == -1)
            {
                if (fillBufferWithArray)
                {
                    _wireBuffer.AddRange(BufferData(bufferWritePerSeconds));
                }
                else
                {
                    for (int i = 0; i < bufferWritePerSeconds; i++)
                    {
                        _wireBuffer.Add(new float3(i, i, i * 2));
                        _wireBuffer.Add(new float3(i, i, i * 3));
                    }
                }
                return;
            }

            if (currentCopy == 0)
            {
                if (fillBufferWithArray)
                {
                    bufferCopy.AddRange(BufferData(bufferWritePerSeconds));
                }
                else
                {
                    for (int i = 0; i < bufferWritePerSeconds; i++)
                    {
                        bufferCopy.Add(new float3(i, i, i * 2));
                        bufferCopy.Add(new float3(i, i, i * 3));
                    }
                }
                return;
            }
            
            WriteToBuffer(currentCopy - 1, bufferCopy);
        }

        private NativeArray<float3> BufferData(int count)
        {
            var data = new NativeArray<float3>(count * 2, Allocator.Temp);
            for (int i = 0; i < data.Length; i += 2)
            {
                data[i] = new float3(i, i, i * 2);
                data[i + 1] = new float3(i, i, i * 3);
            }

            return data;
        }
    }
}